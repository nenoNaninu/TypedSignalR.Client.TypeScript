using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class EchoController : ControllerBase
{
    [HttpGet]
    public string? Echo(string? param)
    {
        return param;
    }
}
