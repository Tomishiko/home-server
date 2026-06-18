namespace core.Domain;

public class ValidTokenDetail
{
    public int TokenId { get; set; }
    public long IssuerId { get; set; }
    public string IssuerName { get; set; } = null!;
    public byte[] TokenHash { get; set; } = null!;
}
