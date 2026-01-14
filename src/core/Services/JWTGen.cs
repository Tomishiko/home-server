namespace core.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using core.Models;
using System.Diagnostics;

public class JWTGen
{
    private IOptions<JWT> _jwtOptions;
    private JwtSecurityTokenHandler _handler;

    public JWTGen(IOptions<JWT> jwtOptions)
    {
        _jwtOptions = jwtOptions;
        _handler = new JwtSecurityTokenHandler();
    }

    public string GenerateNewToken(User user)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(user.Uname);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(user.Role);
        ArgumentNullException.ThrowIfNull(user.Id);


        var jwt = _jwtOptions.Value;
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        Claim[] claims = [new Claim(ClaimTypes.Name,user.Uname),
                                     new Claim(ClaimTypes.Role,user.Role),
                                     new Claim("Id",user.Id.ToString()!)];

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
