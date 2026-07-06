using FlowDesigner.Application.DTOs;
using FlowDesigner.Application.Interfaces.Services;

namespace FlowDesigner.Application.Services;

public class HealthService : IHealthService
{
    public HealthResponse GetHealth()
    {
        return new HealthResponse("ok", DateTime.UtcNow.ToString("O"));
    }
}
