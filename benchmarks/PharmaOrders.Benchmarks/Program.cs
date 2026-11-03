using System.Diagnostics;
using System.Globalization;
using System.Runtime;
using System.Text;
using BenchmarkDotNet.Running;
using PharmaOrders.Application.Orders;
using PharmaOrders.Benchmarks;
using PharmaOrders.Domain.Orders;
using PharmaOrders.Infrastructure;
using PharmaOrders.Infrastructure.Persistence;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

if (args.Contains("--throughput"))
{
    var output = args.SkipWhile(a => a != "--out").Skip(1).FirstOrDefault() ?? "results/THROUGHPUT.md";
    var requests = int.TryParse(args.SkipWhile(a => a != "--requests").Skip(1).FirstOrDefault(), out var r) ? r : 200_000;
    Throughput.Run(requests, rounds: 5, output);
    return;
}

BenchmarkSwitcher.FromAssembly(typeof(MappingBenchmark).Assembly).Run(args);

/// <summary>
/// Alto volume: N requisições em paralelo, cada uma mapeando uma página de 20 pedidos.
/// Para não favorecer ninguém: aquecimento paralelo de todos, 5 rodadas com a ordem rotacionada
/// e o resultado é a MEDIANA das rodadas.
/// </summary>
internal static class Throughput
{
    private const int PageSize = 20;
    private static long _sink;

    public static void Run(int requests, int rounds, string outputPath)
    {
        var page = SampleOrders.Create(PageSize);

        var sw = Stopwatch.StartNew();
        _ = DependencyInjection.CreateMapper(MappingStrategy.AutoMapper);
        sw.Stop();
        var autoMapperStartupMs = sw.Elapsed.TotalMilliseconds;

        var mappers = DependencyInjection.AllMappers();

        // aquecimento: JIT, tiered compilation e thread pool já no tamanho certo para todos
        foreach (var m in mappers)
            Parallel.For(0, 20_000, _ => Sink(m.MapList(page)));

        var samples = mappers.ToDictionary(m => m.Name, _ => new List<Sample>());
        for (var round = 0; round < rounds; round++)
        {
            foreach (var m in Rotate(mappers, round))
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                var gen0 = GC.CollectionCount(0);
                var gen1 = GC.CollectionCount(1);
                var gen2 = GC.CollectionCount(2);
                var allocated = GC.GetTotalAllocatedBytes(precise: true);
                var pause = GC.GetTotalPauseDuration();

                var timer = Stopwatch.StartNew();
                Parallel.For(0, requests, _ => Sink(m.MapList(page)));
                timer.Stop();

                samples[m.Name].Add(new Sample(
                    timer.Elapsed,
                    (GC.GetTotalAllocatedBytes(precise: true) - allocated) / (1024.0 * 1024.0),
                    GC.CollectionCount(0) - gen0,
                    GC.CollectionCount(1) - gen1,
                    GC.CollectionCount(2) - gen2,
                    GC.GetTotalPauseDuration() - pause));
            }
            Console.WriteLine($"rodada {round + 1}/{rounds} ok");
        }

        var rows = mappers.Select(m => Row.From(m.Name, requests, samples[m.Name])).ToList();
        var md = ToMarkdown(requests, rounds, autoMapperStartupMs, rows);

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath))!);
        File.WriteAllText(outputPath, md);
        Console.WriteLine(md);
    }

    private static IEnumerable<IOrderMapper> Rotate(IReadOnlyList<IOrderMapper> items, int offset) =>
        Enumerable.Range(0, items.Count).Select(i => items[(i + offset) % items.Count]);

    private static void Sink(List<OrderDto> result) => Interlocked.Add(ref _sink, result.Count);

    private static string ToMarkdown(int requests, int rounds, double autoMapperStartupMs, List<Row> rows)
    {
        var manual = rows.First(r => r.Name == "Manual");
        var sb = new StringBuilder();
        sb.AppendLine("# Teste de alto volume");
        sb.AppendLine();
        sb.AppendLine($"- **Data (UTC):** {DateTime.UtcNow:yyyy-MM-dd HH:mm}");
        sb.AppendLine($"- **Carga:** {requests:N0} requisições em paralelo, cada uma mapeando uma página de {PageSize} pedidos (5 itens cada)");
        sb.AppendLine($"- **Método:** aquecimento de todos, {rounds} rodadas com ordem rotacionada, valores = mediana das rodadas");
        sb.AppendLine($"- **Máquina:** {Environment.ProcessorCount} vCPUs · {Environment.OSVersion} · .NET {Environment.Version} · Server GC: {GCSettings.IsServerGC}");
        sb.AppendLine($"- **Inicialização do AutoMapper** (montar e validar a configuração, uma vez por aplicação): {autoMapperStartupMs:F0} ms");
        sb.AppendLine();
        sb.AppendLine("| Implementação | Tempo total | Requisições/s | Tempo vs. manual | Memória alocada | Coletas GC (gen0/1/2) | Pausa do GC |");
        sb.AppendLine("|---|---:|---:|---:|---:|---:|---:|");
        foreach (var r in rows)
        {
            sb.AppendLine(
                $"| {r.Name} | {r.Elapsed.TotalSeconds:F2} s | {r.RequestsPerSecond:N0} | {r.Elapsed / manual.Elapsed:F2}x | " +
                $"{r.AllocatedMb:N0} MB | {r.Gen0}/{r.Gen1}/{r.Gen2} | {r.GcPause.TotalMilliseconds:F0} ms |");
        }
        return sb.ToString();
    }

    private sealed record Sample(TimeSpan Elapsed, double AllocatedMb, int Gen0, int Gen1, int Gen2, TimeSpan GcPause);

    private sealed record Row(string Name, TimeSpan Elapsed, double RequestsPerSecond, double AllocatedMb,
        int Gen0, int Gen1, int Gen2, TimeSpan GcPause)
    {
        public static Row From(string name, int requests, List<Sample> s)
        {
            var median = s.OrderBy(x => x.Elapsed).ElementAt(s.Count / 2);
            return new Row(name, median.Elapsed, requests / median.Elapsed.TotalSeconds, median.AllocatedMb,
                median.Gen0, median.Gen1, median.Gen2, median.GcPause);
        }
    }
}
