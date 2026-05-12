# conexao-solidaria-donation-worker

Worker responsavel por consumir eventos de doacao da fila RabbitMQ e atualizar o valor arrecadado das campanhas.

## Tecnologias

- .NET 10 / Worker Service
- MassTransit 8 + RabbitMQ
- Entity Framework Core + SQL Server
- Prometheus para metricas

## Funcionamento

1. Escuta a fila `DoacaoRecebida` no RabbitMQ
2. Ao receber um `DoacaoRecebidaEvent`, busca a doacao no banco
3. Atualiza o campo `ValorArrecadado` da respectiva campanha
4. Incrementa os contadores de metricas do Prometheus

## Metricas expostas

| Metrica                           | Descricao                              |
|-----------------------------------|----------------------------------------|
| worker_doacoes_processadas_total  | Total de doacoes processadas com sucesso |
| worker_doacoes_erro_total         | Total de doacoes com erro              |

Disponiveis em: `http://localhost:9091/metrics`

## Contrato do evento

```csharp
public record DoacaoRecebidaEvent
{
    public Guid    Id          { get; init; }
    public Guid    CampanhaId  { get; init; }
    public Guid    DoadorId    { get; init; }
    public decimal ValorDoacao { get; init; }
    public DateTime CriadoEm  { get; init; }
}
```

## Variaveis de ambiente

| Variavel                              | Descricao                       |
|---------------------------------------|---------------------------------|
| ConnectionStrings__DefaultConnection  | Connection string do SQL Server |
| RabbitMQ__Host                        | Host do RabbitMQ                |
| RabbitMQ__Username                    | Usuario do RabbitMQ             |
| RabbitMQ__Password                    | Senha do RabbitMQ               |

## Rodar localmente

```bash
# Subir dependencias via Docker Compose (do repo de infra)
cd ../conexao-solidaria-infra
docker compose up -d sqlserver rabbitmq

# Rodar o worker
cd ../conexao-solidaria-donation-worker
dotnet run --project src/DonationWorker
```

## CI/CD

A cada push na branch `main`, o GitHub Actions:
1. Restaura dependencias
2. Compila o projeto
3. Gera a imagem Docker
4. Publica no Docker Hub: `brpeekz/conexao-solidaria-donation-worker`