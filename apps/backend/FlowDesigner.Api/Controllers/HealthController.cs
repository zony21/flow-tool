using FlowDesigner.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowDesigner.Api.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController(IHealthService healthService) : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var response = healthService.GetHealth();
        return Ok(response);
    }
}
