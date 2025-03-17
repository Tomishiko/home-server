using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using web.Models;
using web.Services;
public class JWTGen
{
    private IOptions<JWT> _jwtOptions;
    private JwtSecurityTokenHandler _handler;
    public JWTGen(IOptions<JWT> jwtOptions)
    {
        _jwtOptions = jwtOptions;
        _handler = new JwtSecurityTokenHandler();
    }
    public string GenerateNewToken(string username)
    {
        var jwt = _jwtOptions.Value;
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        IEnumerable<Claim> claims = [new Claim("name", username),new Claim("role", "manager")];
        //if(username == "admin")
        //{
        //    claims.Append(new Claim("role", "manager"));
        //}
        double expires;
        if (!double.TryParse(jwt.expiration, out expires))
            expires = 120.0;

        var sectoken = new JwtSecurityToken(jwt.issuer,
          jwt.issuer,
          claims,
          expires: DateTime.Now.AddMinutes(expires),
          signingCredentials: credentials);
        return _handler.WriteToken(sectoken);
    }

}
