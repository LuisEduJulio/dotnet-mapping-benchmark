# Teste de alto volume

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
