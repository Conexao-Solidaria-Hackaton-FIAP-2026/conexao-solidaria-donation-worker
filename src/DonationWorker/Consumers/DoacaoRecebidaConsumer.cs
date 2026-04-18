using DonationWorker.Data;
using DonationWorker.Domain;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Prometheus;

namespace DonationWorker.Consumers;

public class DoacaoRecebidaConsumer(AppDbContext db, ILogger<DoacaoRecebidaConsumer> logger) : IConsumer<DoacaoRecebidaEvent>
{
    private readonly AppDbContext _db = db;
    private readonly ILogger<DoacaoRecebidaConsumer> _logger = logger;

    // Contador de doacoes processadas exposto ao Prometheus
    private static readonly Counter DoacoesProcessadas = Metrics
        .CreateCounter("worker_doacoes_processadas_total", "Total de doacoes processadas com sucesso.");

    private static readonly Counter DoacoesComErro = Metrics
        .CreateCounter("worker_doacoes_erro_total", "Total de doacoes com erro no processamento.");

    public async Task Consume(ConsumeContext<DoacaoRecebidaEvent> context)
    {
        var evt = context.Message;

        _logger.LogInformation("Processando doacao {Id} | Campanha: {CampanhaId} | Valor: R${Valor}", evt.Id, evt.CampanhaId, evt.ValorDoacao);

        try
        {
            // 1. Registra a doacao na tabela propria do worker
            _db.Doacoes.Add(new Doacao
            {
                Id = evt.Id,
                CampanhaId = evt.CampanhaId,
                DoadorId = evt.DoadorId,
                Valor = evt.ValorDoacao,
                ProcessadoEm = DateTime.UtcNow
            });

            // 2. Atualiza ValorArrecadado na tabela Campanhas (banco compartilhado)
            // Usa SQL direto para nao precisar mapear a entidade Campanha aqui
            var rows = await _db.Database.ExecuteSqlRawAsync(
                "UPDATE dbo.Campanhas SET ValorArrecadado = ValorArrecadado + {0} WHERE Id = {1}",
                evt.ValorDoacao, evt.CampanhaId);

            if (rows == 0)
            {
                _logger.LogWarning("Campanha {CampanhaId} nao encontrada.", evt.CampanhaId);
                DoacoesComErro.Inc();
                return;
            }

            await _db.SaveChangesAsync();
            DoacoesProcessadas.Inc();

            _logger.LogInformation("Doacao {Id} processada com sucesso.", evt.Id);
        }
        catch (Exception ex)
        {
            DoacoesComErro.Inc();
            _logger.LogError(ex, "Erro ao processar doacao {Id}.", evt.Id);
            throw; // MassTransit recoloca na fila automaticamente
        }
    }
}
