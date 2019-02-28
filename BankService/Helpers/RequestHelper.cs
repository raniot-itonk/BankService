using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace BankService.Helpers
{
    public static class RequestHelper
    {

        private static Guid GetJwtFromHeader(HttpRequest request)
        {
            var keyValuePair = request.Headers.First(pair => pair.Key == "Authorization");
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var jwtSecurityToken = jwtSecurityTokenHandler.ReadJwtToken(keyValuePair.Value.ToString().Replace("bearer ", ""));
                var idFromJwt = Guid.Parse(jwtSecurityToken.Subject);
                return idFromJwt;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// Validate Id against id from header, this functionality is pr default disabled in Development
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        public static bool ValidateId(Guid id, HttpRequest request, IHostingEnvironment env)
        {
            return env.IsDevelopment() || id.Equals(GetJwtFromHeader(request));
        }
    }
}
