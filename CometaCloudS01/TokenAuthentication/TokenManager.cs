﻿using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CometaCloudS01.TokenAuthentication
{
    public class TokenManager : ITokenManager
    {

        private JwtSecurityTokenHandler tokenHandler;
        private byte[] secretKey;

        public TokenManager()
        {
            tokenHandler = new JwtSecurityTokenHandler();
            secretKey = Encoding.ASCII.GetBytes("traunarosaeunaviolacistabenepureungiglio");
        }

        public bool Authenticate(string username, string password)
        {
            if (!string.IsNullOrWhiteSpace(username) &&
                !string.IsNullOrWhiteSpace(password) &&
                username.ToLower() == "admin" && password == "password")
                return true;
            else
                return false;
        }

        public string NewToken()
        {
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "Dynamics CRM") }),
                Expires = DateTime.UtcNow.AddMinutes(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey),
                SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtString = tokenHandler.WriteToken(token);
            return jwtString;
        }

        public ClaimsPrincipal VerifyToken(string token)
        {
            var claims = tokenHandler.ValidateToken(token,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                    ValidateLifetime = true,
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validateToken);
            return claims;
        }
    }
}
