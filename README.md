# Note Tecniche

Le funzioni fondamentali per l'autenticazione di un utente sono svolte dalla classe TokenManager
essa si occupa di 

Autenticare 
```csharp
public bool Authenticate(string username, string password)
        {
            if (!string.IsNullOrWhiteSpace(username) &&
                !string.IsNullOrWhiteSpace(password) &&
                username.ToLower() == "admin" && password == "password")
                return true;
            else
                return false;
        }
```

Creare un nuovo token:
```csharp
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
```
Verificare la validità del token
```csharp
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
```

Questa classe viene registrata nello startup del progetto e tramite la dependency injection è utilizzabile nei controller della web api e nei filtri 

Controller di richiesta autenticazione (AuthenticateController)

Il controller ha un metodo get che , passando username e password restituisce un token 

```csharp
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
```
il tutto utilizzando sempre il token manager

E' stato creato un filtro dedicato all'autorizzazione delle richieste. Una classe derivata da IAuthorizationFilter che implementa il metodo OnAuthorization ad ogni chiamata http all'endpoint del controller che lo usano


```csharp
public class TokenAuthenticationFilter : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {

            var tokenManager = (ITokenManager)context.HttpContext.RequestServices.GetService(typeof(ITokenManager));

            var result = true;
            if (!context.HttpContext.Request.Headers.ContainsKey("Authorization"))
                result = false;
            string token = string.Empty;
            if(result)
            {
                token = context.HttpContext.Request.Headers.First(x => x.Key == "Authorization").Value;

                try
                {
                    var claimPrinciple = tokenManager.VerifyToken(token);
                }
                catch (Exception ex)
                {
                    result = false;
                    context.ModelState.AddModelError("Unauthorized", ex.ToString());
                }
            }
            if(!result)
            {
                context.Result = new UnauthorizedObjectResult(context.ModelState);
            }
        }
    }
```
nel metodo OnAuthorization viene estratto l'Hedare "Authorization" dal contesto http e viene verificato il token 


Il filtro di autenticazione  viene utilizzato nei controller 

```csharp
[ApiController]
    [Route("[controller]")]
    [TokenAuthenticationFilter]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
```

tramite l'attributo [TokenAuthenticationFilter]

