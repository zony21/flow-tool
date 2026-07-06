using FlowDesigner.Application.DTOs;

namespace FlowDesigner.Application.Interfaces.Services;

public interface IHealthService
{
    HealthResponse GetHealth();
}
