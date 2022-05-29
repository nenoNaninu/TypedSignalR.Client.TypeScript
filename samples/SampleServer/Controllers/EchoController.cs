using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SampleServer.Controllers;

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
