namespace DonationWorker.Data;

public class Doacao
{
    public Guid Id { get; set; }
    public Guid CampanhaId { get; set; }
    public Guid DoadorId { get; set; }
    public decimal Valor { get; set; }
    public DateTime CriadoEm { get; set; }
    public DoacaoStatus Status { get; set; }
}
public enum DoacaoStatus { Pendente, Processada, Falha }