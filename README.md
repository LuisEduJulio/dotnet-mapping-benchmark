# 🚀 Benchmark .NET: AutoMapper vs. Manual vs. Classe Abstrata vs. Mapperly

[![mapping-benchmark](https://github.com/LuisEduJulio/dotnet-mapping-benchmark/actions/workflows/benchmark.yml/badge.svg)](https://github.com/LuisEduJulio/dotnet-mapping-benchmark/actions/workflows/benchmark.yml)

Quanto custa transformar entidade em DTO quando a API recebe **muitas requisições por segundo**?
Este repositório mede 4 formas de fazer o mesmo mapeamento, com resultados gerados pelo GitHub Actions
a cada push em [`results/BENCHMARK.md`](results/BENCHMARK.md).

## Cenário

Pedido de uma rede de farmácias → `OrderDto` da API:

- dados do pedido + **status (enum → texto)**
- cliente **achatado** (`CustomerName`, `CustomerCity`, `CustomerUf`)
- lista de **5 itens** (`OrderItemDto`)

Antes de medir, o programa confere que **as 4 implementações geram exatamente o mesmo JSON**.

## As implementações

| | Como funciona | Prós | Contras |
|---|---|---|---|
| **Manual** | método estático escrito à mão | o mais rápido e explícito | código repetitivo; esquecer um campo não dá erro |
| **Classe abstrata** | `MapperBase<TSource, TDest>` com métodos virtuais, injetado pelo tipo base | padroniza o time, fácil de testar | chamadas virtuais e o mesmo código manual por baixo |
| **AutoMapper 14** | convenção + reflection na configuração, expressões compiladas no runtime | pouco código | mais lento, aloca mais, erro só aparece em runtime e **licença comercial a partir da v15** |
| **Mapperly** | *source generator*: o mapeamento é **gerado na compilação** | performance de código manual, sem reflection, erro vira warning de build | mapeamentos muito dinâmicos exigem métodos manuais |

> **Licença do AutoMapper:** desde a versão 15 (julho/2025, Lucky Penny Software) o AutoMapper é dual license
> (RPL-1.5 ou comercial). A linha 14.x continua MIT e é a usada aqui.

## Duas medições

1. **BenchmarkDotNet:** tempo médio e memória alocada por chamada, mapeando 1, 100 e 1.000 pedidos.
2. **Alto volume:** 200 mil requisições em paralelo, cada uma mapeando uma página de 20 pedidos.
   Mede tempo total, requisições/s, memória alocada, coletas do Garbage Collector e tempo de pausa do GC.
   Em produção, **alocação vira coleta de lixo, e coleta vira latência** para todo mundo.

## Como rodar

```bash
dotnet run -c Release --project src/MappingBenchmarks -- --throughput --requests 200000
dotnet run -c Release --project src/MappingBenchmarks -- --filter '*'
```

## O que levar para aplicações com alto volume

1. **Prefira mapeamento resolvido em compilação** (Mapperly ou manual) nos caminhos quentes.
2. **Meça a alocação, não só o tempo.** Sob carga, o GC é quem cobra a conta.
3. **Projete direto no banco** (`Select` para DTO no EF Core) e evite carregar a entidade inteira só para mapear depois.
4. **Pré-dimensione listas** (`new List<T>(count)`) e evite `Enum.ToString()` em loop.
5. **Configure o AutoMapper uma vez** (singleton) e use `AssertConfigurationIsValid` em teste, se continuar com ele.

---

Feito por Luis Eduardo · .NET · C# · performance
