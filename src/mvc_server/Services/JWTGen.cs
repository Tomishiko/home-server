using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using mvc_server.Models;
using mvc_server.Services;
public class JWTGen
{
    private IConfiguration _config;
    private JwtSecurityTokenHandler _handler;
    public JWTGen(IConfiguration config)
    {
        _config = config;
        _handler = new JwtSecurityTokenHandler();
    }
    public string GenerateNewToken(string username)
    {

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        IEnumerable<Claim> claims = [new Claim("name", username)];
        double expires;
        if (!double.TryParse(_config["JWT:expiration"], out expires))
            expires = 120.0;

        var Sectoken = new JwtSecurityToken(_config["JWT:issuer"],
          _config["JWT:issuer"],
          claims,
          expires: DateTime.Now.AddMinutes(expires),
          signingCredentials: credentials);
        return _handler.WriteToken(Sectoken);
    }

}
