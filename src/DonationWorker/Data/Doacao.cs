namespace DonationWorker.Data;

public class Doacao
{
    public Guid Id { get; set; }
    public Guid CampanhaId { get; set; }
    public Guid DoadorId { get; set; }
    public decimal Valor { get; set; }
    public DateTime ProcessadoEm { get; set; }
}
