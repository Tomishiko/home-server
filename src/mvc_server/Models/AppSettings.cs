namespace mvc_server.Models;

public class AppSettings
{
   public JWT jwtSettings { get; set; }
}

public class JWT
{
    public string key { get; set; }
    public string issuer { get; set; }
    public string expiration { get; set; }
}