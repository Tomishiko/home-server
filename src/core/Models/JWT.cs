namespace core.Models;

public class JWT
{
    public string key { get; set; }
    public string issuer { get; set; }
    public string expiration { get; set; }
}
