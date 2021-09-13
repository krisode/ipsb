//using IPSB.Infrastructure.Contexts;
using IPSB.Infrastructure.Contexts;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static IPSB.Utils.Constants;

namespace IPSB.Utils
{
    public interface IJwtTokenProvider
    {
        Task<string> GetAccessToken(List<Claim> additionalClaims);
        Task<string> GetRefreshToken(List<Claim> additionalClaims);
        List<Claim> GetAdditionalClaims(Account account);
        int GetIdFromToken(string tokenString);

    }
    public class JwtTokenProvider : IJwtTokenProvider
    {
        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;
        private readonly IConfiguration _configuration;

        public JwtTokenProvider(IConfiguration configuration)
        {
            _configuration = configuration;
            _jwtSecurityTokenHandler = _jwtSecurityTokenHandler ?? new JwtSecurityTokenHandler();
        }

        public Task<string> GenerateToken(List<Claim> additionalClaims, DateTime expiredPeriod)
        {
            return Task.Run(() =>
            {
                var jwtKey = _configuration[Constants.Config.KEY];
                var jwtIssuer = _configuration[Constants.Config.ISSUER];
                var jwtAudience = _configuration[Constants.Config.AUDIENCE];

                var symmectricSecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey));

                //signing credentials
                var signingCredentials = new SigningCredentials(symmectricSecurityKey, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                        issuer: jwtIssuer,
                        audience: jwtAudience,
                        expires: expiredPeriod,
                        claims: additionalClaims,
                        signingCredentials: signingCredentials
                    );

                //return token
                return _jwtSecurityTokenHandler.WriteToken(token);
            });

        }

        public Task<string> GetAccessToken(List<Claim> additionalClaims)
        {
            return GenerateToken(additionalClaims, DateTime.Now.AddMinutes(Constants.TokenParams.MINUTE_TO_EXPIRES));
        }

        public Task<string> GetRefreshToken(List<Claim> additionalClaims)
        {
            return GenerateToken(additionalClaims, DateTime.Now.AddDays(Constants.TokenParams.DAY_TO_EXPIRES));
        }

        public List<Claim> GetAdditionalClaims(Account account)
        {
            var additionalClaims = new List<Claim>();
            additionalClaims.Add(new Claim(ClaimTypes.Role, account.Role));
            additionalClaims.Add(new Claim(ClaimTypes.Name, account.Id.ToString()));
            return additionalClaims;
        }

        public int GetIdFromToken(string tokenString)
        {
            _jwtSecurityTokenHandler.ValidateToken(
               tokenString,
               JwtBearerTokenConfig.GetTokenValidationParameters(_configuration),
               out SecurityToken validatedToken
            );
            var jwtToken = (JwtSecurityToken)validatedToken;
            var accountId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);
            // return account id from JWT token if validation successful
            return accountId;
        }
    }
}
