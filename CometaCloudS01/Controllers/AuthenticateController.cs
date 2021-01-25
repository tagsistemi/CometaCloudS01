using CometaCloudS01.TokenAuthentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CometaCloudS01.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly ITokenManager tokenManager;

        public AuthenticateController(ITokenManager tokenManager)
        {
            this.tokenManager = tokenManager;
        }

        [HttpGet]
        public IActionResult Authenticate(string user, string pwd)
        {
            if(tokenManager.Authenticate(user, pwd))
            {
                return Ok(new { Token = tokenManager.NewToken() });
            }
            else
            {
                ModelState.AddModelError("Unauthorized", "non sei autorizzato.");
                return Unauthorized(ModelState);
            }
        }
    }
}
