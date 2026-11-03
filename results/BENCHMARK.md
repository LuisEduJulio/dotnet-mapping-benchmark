# Resultado do benchmark de mapeamento

Gerado pelo GitHub Actions em 2026-11-03 14:56 UTC.

## Tempo e memória por operação (BenchmarkDotNet)

`Orders` = quantidade de pedidos mapeados por chamada (cada pedido tem 5 itens).

| Method                        | Orders | Mean         | Error       | StdDev      | Ratio | RatioSD | Rank | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------------------ |------- |-------------:|------------:|------------:|------:|--------:|-----:|-------:|-------:|----------:|------------:|
| Manual                        | 1      |     167.3 ns |     3.38 ns |     4.02 ns |  1.00 |    0.03 |    1 | 0.0076 |      - |     632 B |        1.00 |
| &#39;Mapperly (source generator)&#39; | 1      |     191.0 ns |     3.88 ns |     4.16 ns |  1.14 |    0.04 |    2 | 0.0081 |      - |     672 B |        1.06 |
| &#39;Classe abstrata&#39;             | 1      |     223.8 ns |     4.47 ns |     5.15 ns |  1.34 |    0.04 |    3 | 0.0076 |      - |     632 B |        1.00 |
| &#39;AutoMapper 14&#39;               | 1      |     338.6 ns |     4.24 ns |     3.97 ns |  2.03 |    0.05 |    4 | 0.0091 |      - |     760 B |        1.20 |
|                               |        |              |             |             |       |         |      |        |        |           |             |
| Manual                        | 100    |  14,895.0 ns |   119.74 ns |   106.15 ns |  1.00 |    0.01 |    1 | 0.7019 | 0.0610 |   57656 B |        1.00 |
| &#39;Mapperly (source generator)&#39; | 100    |  17,526.8 ns |   159.42 ns |   141.32 ns |  1.18 |    0.01 |    2 | 0.7324 | 0.0610 |   61656 B |        1.07 |
| &#39;Classe abstrata&#39;             | 100    |  20,432.5 ns |   277.27 ns |   259.36 ns |  1.37 |    0.02 |    3 | 0.7019 | 0.0610 |   57656 B |        1.00 |
| &#39;AutoMapper 14&#39;               | 100    |  22,672.6 ns |   356.89 ns |   333.83 ns |  1.52 |    0.02 |    4 | 0.8240 | 0.0610 |   69393 B |        1.20 |
|                               |        |              |             |             |       |         |      |        |        |           |             |
| Manual                        | 1000   | 157,551.3 ns | 2,005.71 ns | 1,778.01 ns |  1.00 |    0.02 |    1 | 6.8359 | 3.1738 |  576056 B |        1.00 |
| &#39;Mapperly (source generator)&#39; | 1000   | 190,365.4 ns | 3,737.10 ns | 3,670.33 ns |  1.21 |    0.03 |    2 | 7.3242 | 3.6621 |  616056 B |        1.07 |
| &#39;Classe abstrata&#39;             | 1000   | 216,029.7 ns | 1,604.65 ns | 1,422.48 ns |  1.37 |    0.02 |    3 | 6.8359 | 2.9297 |  576056 B |        1.00 |
| &#39;AutoMapper 14&#39;               | 1000   | 232,735.8 ns | 1,619.42 ns | 1,264.34 ns |  1.48 |    0.02 |    4 | 8.3008 | 3.9063 |  688605 B |        1.20 |

## Teste de alto volume

- **Data (UTC):** 2026-11-03 14:52
- **Carga:** 200,000 requisições em paralelo, cada uma mapeando uma página de 20 pedidos (5 itens cada)
- **Máquina:** 4 vCPUs · Unix 7.0.0.1017 · .NET 8.0.33 · Server GC: True
- **Custo de inicialização do AutoMapper** (montar a configuração): 2 ms

| Implementação | Tempo total | Requisições/s | vs. manual | Memória alocada | Coletas GC (gen0/1/2) | Pausa do GC |
|---|---:|---:|---:|---:|---:|---:|
| Manual | 0.67 s | 297,686 | 1.00x | 2,208 MB | 28/0/0 | 38 ms |
| Classe abstrata | 0.42 s | 470,785 | 0.63x | 2,208 MB | 28/1/0 | 9 ms |
| AutoMapper 14 | 0.58 s | 343,982 | 0.87x | 2,680 MB | 34/2/0 | 15 ms |
| Mapperly (source generator) | 0.38 s | 521,790 | 0.57x | 2,361 MB | 30/1/0 | 18 ms |
