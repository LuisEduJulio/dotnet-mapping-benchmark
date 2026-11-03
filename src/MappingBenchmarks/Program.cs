using System.Diagnostics;
using System.Globalization;
using System.Runtime;
using System.Text;
using System.Text.Json;
using BenchmarkDotNet.Running;
using MappingBenchmarks;
using MappingBenchmarks.Domain;
using MappingBenchmarks.Mappers;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

// 0) Garantia: as 4 implementações produzem exatamente o mesmo JSON.
ValidateEquivalence();

if (args.Contains("--throughput"))
{
    var output = args.SkipWhile(a => a != "--out").Skip(1).FirstOrDefault() ?? "results/THROUGHPUT.md";
    var requests = int.TryParse(args.SkipWhile(a => a != "--requests").Skip(1).FirstOrDefault(), out var r) ? r : 200_000;
    Throughput.Run(requests, output);
    return;
}

BenchmarkSwitcher.FromAssembly(typeof(MappingBenchmark).Assembly).Run(args);

static void ValidateEquivalence()
{
    var orders = SampleData.Orders(50);
    var autoMapper = AutoMapperSetup.Create();
    var abstractMapper = new OrderAbstractMapper(new OrderItemAbstractMapper());

    var expected = JsonSerializer.Serialize(ManualMapping.ToDtos(orders));
    var candidates = new Dictionary<string, string>
    {
        ["Classe abstrata"] = JsonSerializer.Serialize(abstractMapper.MapList(orders)),
        ["AutoMapper"] = JsonSerializer.Serialize(autoMapper.Map<List<OrderDto>>(orders)),
        ["Mapperly"] = JsonSerializer.Serialize(MapperlyMapping.ToDtos(orders))
    };

    foreach (var (name, json) in candidates)
    {
        if (json != expected)
            throw new InvalidOperationException($"{name} gerou um JSON diferente do mapeamento manual.");
    }

    Console.WriteLine("OK: as 4 implementações geram o mesmo JSON.");
}

/// <summary>
/// Simula alto volume: N requisições em paralelo, cada uma mapeando uma página de 20 pedidos.
/// Mede o que pesa em produção: tempo total, requisições/s, memória alocada e coletas do GC.
/// </summary>
internal static class Throughput
{
    private const int PageSize = 20;

    public static void Run(int requests, string outputPath)
    {
        var page = SampleData.Orders(PageSize);

        var sw = Stopwatch.StartNew();
        var autoMapper = AutoMapperSetup.Create();
        sw.Stop();
        var autoMapperStartupMs = sw.Elapsed.TotalMilliseconds;

        var abstractMapper = new OrderAbstractMapper(new OrderItemAbstractMapper());

        var scenarios = new (string Name, Func<List<Order>, List<OrderDto>> Map)[]
        {
            ("Manual", ManualMapping.ToDtos),
            ("Classe abstrata", p => abstractMapper.MapList(p)),
            ("AutoMapper 14", p => autoMapper.Map<List<OrderDto>>(p)),
            ("Mapperly (source generator)", MapperlyMapping.ToDtos)
        };

        var rows = new List<Row>();
        foreach (var (name, map) in scenarios)
        {
            // aquecimento (JIT / tiered compilation)
            for (var i = 0; i < 5_000; i++) Sink(map(page));

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var gen0 = GC.CollectionCount(0);
            var gen1 = GC.CollectionCount(1);
            var gen2 = GC.CollectionCount(2);
            var allocated = GC.GetTotalAllocatedBytes(precise: true);
            var pause = GC.GetTotalPauseDuration();

            var timer = Stopwatch.StartNew();
            Parallel.For(0, requests, _ => Sink(map(page)));
            timer.Stop();

            rows.Add(new Row(
                name,
                timer.Elapsed,
                requests / timer.Elapsed.TotalSeconds,
                (GC.GetTotalAllocatedBytes(precise: true) - allocated) / (1024.0 * 1024.0),
                GC.CollectionCount(0) - gen0,
                GC.CollectionCount(1) - gen1,
                GC.CollectionCount(2) - gen2,
                GC.GetTotalPauseDuration() - pause));

            Console.WriteLine(rows[^1]);
        }

        var md = ToMarkdown(requests, autoMapperStartupMs, rows);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath))!);
        File.WriteAllText(outputPath, md);
        Console.WriteLine();
        Console.WriteLine(md);
    }

    private static long _sink;
    private static void Sink(List<OrderDto> result) => Interlocked.Add(ref _sink, result.Count);

    private static string ToMarkdown(int requests, double autoMapperStartupMs, List<Row> rows)
    {
        var manual = rows[0];
        var sb = new StringBuilder();
        sb.AppendLine("# Teste de alto volume");
        sb.AppendLine();
        sb.AppendLine($"- **Data (UTC):** {DateTime.UtcNow:yyyy-MM-dd HH:mm}");
        sb.AppendLine($"- **Carga:** {requests:N0} requisições em paralelo, cada uma mapeando uma página de {PageSize} pedidos (5 itens cada)");
        sb.AppendLine($"- **Máquina:** {Environment.ProcessorCount} vCPUs · {Environment.OSVersion} · .NET {Environment.Version} · Server GC: {GCSettings.IsServerGC}");
        sb.AppendLine($"- **Custo de inicialização do AutoMapper** (montar a configuração): {autoMapperStartupMs:F0} ms");
        sb.AppendLine();
        sb.AppendLine("| Implementação | Tempo total | Requisições/s | vs. manual | Memória alocada | Coletas GC (gen0/1/2) | Pausa do GC |");
        sb.AppendLine("|---|---:|---:|---:|---:|---:|---:|");
        foreach (var r in rows)
        {
            var ratio = r.Elapsed.TotalMilliseconds / manual.Elapsed.TotalMilliseconds;
            sb.AppendLine(
                $"| {r.Name} | {r.Elapsed.TotalSeconds:F2} s | {r.RequestsPerSecond:N0} | {ratio:F2}x | " +
                $"{r.AllocatedMb:N0} MB | {r.Gen0}/{r.Gen1}/{r.Gen2} | {r.GcPause.TotalMilliseconds:F0} ms |");
        }
        return sb.ToString();
    }

    private sealed record Row(
        string Name, TimeSpan Elapsed, double RequestsPerSecond, double AllocatedMb,
        int Gen0, int Gen1, int Gen2, TimeSpan GcPause)
    {
        public override string ToString() =>
            $"{Name,-30} {Elapsed.TotalSeconds,6:F2}s  {RequestsPerSecond,12:N0} req/s  {AllocatedMb,8:N0} MB  GC {Gen0}/{Gen1}/{Gen2}  pausa {GcPause.TotalMilliseconds:F0}ms";
    }
}
