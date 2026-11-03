# Resultado do benchmark de mapeamento

Gerado pelo GitHub Actions em 2026-11-03 15:05 UTC.

## Tempo e memória por operação (BenchmarkDotNet)

`Orders` = quantidade de pedidos mapeados por chamada (cada pedido tem 5 itens).

| Method            | Orders | Mean         | Error       | StdDev      | Ratio | RatioSD | Rank | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------ |------- |-------------:|------------:|------------:|------:|--------:|-----:|-------:|-------:|----------:|------------:|
| Mapperly          | 1      |     205.7 ns |     2.31 ns |     2.05 ns |  1.00 |    0.02 |    1 | 0.0086 |      - |     712 B |        1.00 |
| Manual            | 1      |     206.2 ns |     4.09 ns |     4.01 ns |  1.00 |    0.03 |    1 | 0.0086 |      - |     712 B |        1.00 |
| &#39;Classe abstrata&#39; | 1      |     228.0 ns |     2.64 ns |     2.47 ns |  1.11 |    0.02 |    2 | 0.0076 |      - |     632 B |        0.89 |
| &#39;AutoMapper 14&#39;   | 1      |     400.2 ns |     2.40 ns |     2.24 ns |  1.94 |    0.04 |    3 | 0.0095 |      - |     800 B |        1.12 |
|                   |        |              |             |             |       |         |      |        |        |           |             |
| Manual            | 100    |  17,881.1 ns |   187.25 ns |   175.15 ns |  1.00 |    0.01 |    1 | 0.7324 | 0.0610 |   61696 B |        1.00 |
| Mapperly          | 100    |  17,947.1 ns |   214.09 ns |   189.78 ns |  1.00 |    0.01 |    1 | 0.7324 | 0.0610 |   61696 B |        1.00 |
| &#39;Classe abstrata&#39; | 100    |  20,605.8 ns |   173.71 ns |   162.49 ns |  1.15 |    0.01 |    2 | 0.7019 | 0.0610 |   57656 B |        0.93 |
| &#39;AutoMapper 14&#39;   | 100    |  29,058.0 ns |   292.53 ns |   259.32 ns |  1.63 |    0.02 |    3 | 0.8850 | 0.0916 |   73393 B |        1.19 |
|                   |        |              |             |             |       |         |      |        |        |           |             |
| Mapperly          | 1000   | 184,138.7 ns | 1,350.36 ns | 1,197.06 ns |  0.97 |    0.01 |    1 | 7.3242 | 3.4180 |  616096 B |        1.00 |
| Manual            | 1000   | 189,474.4 ns | 2,165.76 ns | 1,919.89 ns |  1.00 |    0.01 |    1 | 7.3242 | 3.4180 |  616096 B |        1.00 |
| &#39;Classe abstrata&#39; | 1000   | 210,663.8 ns | 4,101.56 ns | 3,836.60 ns |  1.11 |    0.02 |    2 | 6.8359 | 3.1738 |  576056 B |        0.94 |
| &#39;AutoMapper 14&#39;   | 1000   | 289,259.9 ns | 1,481.10 ns | 1,312.95 ns |  1.53 |    0.02 |    3 | 8.7891 | 3.9063 |  728606 B |        1.18 |

## Teste de alto volume

- **Data (UTC):** 2026-11-03 15:00
- **Carga:** 200,000 requisições em paralelo, cada uma mapeando uma página de 20 pedidos (5 itens cada)
- **Método:** aquecimento de todos, 5 rodadas com ordem rotacionada, valores = mediana das rodadas
- **Máquina:** 4 vCPUs · Unix 7.0.0.1017 · .NET 8.0.33 · Server GC: True
- **Inicialização do AutoMapper** (montar e validar a configuração, uma vez por aplicação): 94 ms

| Implementação | Tempo total | Requisições/s | Tempo vs. manual | Memória alocada | Coletas GC (gen0/1/2) | Pausa do GC |
|---|---:|---:|---:|---:|---:|---:|
| Manual | 0.28 s | 724,458 | 1.00x | 2,368 MB | 30/1/0 | 8 ms |
| Classe abstrata | 0.30 s | 668,362 | 1.08x | 2,208 MB | 28/1/0 | 8 ms |
| AutoMapper 14 | 0.50 s | 402,987 | 1.80x | 2,832 MB | 35/2/0 | 12 ms |
| Mapperly | 0.28 s | 721,959 | 1.00x | 2,368 MB | 30/1/0 | 11 ms |
