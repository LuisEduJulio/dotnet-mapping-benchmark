# Teste de alto volume

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
