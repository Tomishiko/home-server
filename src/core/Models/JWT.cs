namespace core.Models;

public sealed class JWT
{
    public string key { get; set; }
    public string issuer { get; set; }
    public string expiration { get; set; }
}
