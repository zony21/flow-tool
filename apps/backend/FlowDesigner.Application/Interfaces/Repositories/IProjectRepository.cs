using FlowDesigner.Domain.Entities.Core;

namespace FlowDesigner.Application.Interfaces.Repositories;

public interface IProjectRepository
{
    Task<IReadOnlyList<Project>> ListAsync(CancellationToken cancellationToken = default);
}
