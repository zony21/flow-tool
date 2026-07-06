using FlowDesigner.Api.Attributes;
using FlowDesigner.Application.DTOs.Flows;
using FlowDesigner.Domain.Entities.Core;
using FlowDesigner.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlowDesigner.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/flows")]
[Authorize]
public sealed class FlowsController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [RequirePermission("flow.read")]
    public async Task<ActionResult<IReadOnlyList<FlowSummaryDto>>> List(Guid projectId, CancellationToken cancellationToken)
    {
        var flows = await dbContext.Flows
            .AsNoTracking()
            .Where(flow => flow.ProjectId == projectId)
            .OrderBy(flow => flow.SortOrder)
            .ThenBy(flow => flow.CreatedAtUtc)
            .Select(flow => new FlowSummaryDto(
                flow.FlowId,
                flow.ProjectId,
                flow.Name,
                flow.Description,
                flow.SortOrder,
                flow.CreatedAtUtc,
                flow.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        return Ok(flows);
    }

    [HttpPost]
    [RequirePermission("flow.write")]
    public async Task<ActionResult<FlowDetailDto>> Create(Guid projectId, [FromBody] CreateFlowRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Flow name is required." });
        }

        var projectExists = await dbContext.Projects.AnyAsync(project => project.ProjectId == projectId, cancellationToken);
        if (!projectExists)
        {
            return NotFound();
        }

        var sortOrder = await dbContext.Flows
            .Where(flow => flow.ProjectId == projectId)
            .Select(flow => (int?)flow.SortOrder)
            .MaxAsync(cancellationToken) ?? 0;

        var now = DateTime.UtcNow;
        var flow = new Flow
        {
            FlowId = Guid.NewGuid(),
            ProjectId = projectId,
            Name = request.Name.Trim(),
            Description = request.Description,
            SortOrder = sortOrder + 1,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        dbContext.Flows.Add(flow);
        await dbContext.SaveChangesAsync(cancellationToken);

        var detail = await BuildDetailDtoAsync(flow.FlowId, cancellationToken);
        return CreatedAtAction(nameof(Get), new { projectId, flowId = flow.FlowId }, detail);
    }

    [HttpGet("{flowId:guid}")]
    [RequirePermission("flow.read")]
    public async Task<ActionResult<FlowDetailDto>> Get(Guid projectId, Guid flowId, CancellationToken cancellationToken)
    {
        var flow = await dbContext.Flows
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.FlowId == flowId, cancellationToken);

        if (flow is null)
        {
            return NotFound();
        }

        return Ok(await BuildDetailDtoAsync(flowId, cancellationToken));
    }

    [HttpPut("{flowId:guid}")]
    [RequirePermission("flow.write")]
    public async Task<ActionResult<FlowDetailDto>> Update(Guid projectId, Guid flowId, [FromBody] UpdateFlowRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Flow name is required." });
        }

        var flow = await dbContext.Flows.FirstOrDefaultAsync(x => x.ProjectId == projectId && x.FlowId == flowId, cancellationToken);
        if (flow is null)
        {
            return NotFound();
        }

        flow.Name = request.Name.Trim();
        flow.Description = request.Description;
        flow.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(await BuildDetailDtoAsync(flowId, cancellationToken));
    }

    [HttpDelete("{flowId:guid}")]
    [RequirePermission("flow.write")]
    public async Task<IActionResult> Delete(Guid projectId, Guid flowId, CancellationToken cancellationToken)
    {
        var flow = await dbContext.Flows.FirstOrDefaultAsync(x => x.ProjectId == projectId && x.FlowId == flowId, cancellationToken);
        if (flow is null)
        {
            return NotFound();
        }

        dbContext.Flows.Remove(flow);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private async Task<FlowDetailDto> BuildDetailDtoAsync(Guid flowId, CancellationToken cancellationToken)
    {
        var flow = await dbContext.Flows.AsNoTracking().FirstAsync(x => x.FlowId == flowId, cancellationToken);
        var lanes = await dbContext.Lanes.AsNoTracking()
            .Where(x => x.FlowId == flowId)
            .OrderBy(x => x.SortOrder)
            .Select(x => new LaneDto(x.LaneId, x.FlowId, x.Name, x.SortOrder))
            .ToListAsync(cancellationToken);

        var stages = await dbContext.Stages.AsNoTracking()
            .Where(x => x.FlowId == flowId)
            .OrderBy(x => x.SortOrder)
            .Select(x => new StageDto(x.StageId, x.FlowId, x.Name, x.SortOrder))
            .ToListAsync(cancellationToken);

        var nodes = await dbContext.Nodes.AsNoTracking()
            .Where(x => x.FlowId == flowId)
            .OrderBy(x => x.Name)
            .Select(x => new NodeDto(x.NodeId, x.FlowId, x.LaneId, x.StageId, x.NodeType, x.Name, x.Description, x.X, x.Y))
            .ToListAsync(cancellationToken);

        var links = await dbContext.Links.AsNoTracking()
            .Where(x => x.FlowId == flowId)
            .Select(x => new LinkDto(x.LinkId, x.FlowId, x.SourceNodeId, x.TargetNodeId, x.Label, x.Condition))
            .ToListAsync(cancellationToken);

        var comments = await dbContext.Comments.AsNoTracking()
            .Where(x => x.FlowId == flowId)
            .Select(x => new CommentDto(x.CommentId, x.FlowId, x.NodeId, x.Text, x.X, x.Y))
            .ToListAsync(cancellationToken);

        var metadata = await dbContext.MetadataItems.AsNoTracking()
            .Where(x => x.FlowId == flowId)
            .Select(x => new MetadataDto(x.MetadataId, x.FlowId, x.MetaKey, x.MetaValue))
            .ToListAsync(cancellationToken);

        return new FlowDetailDto(
            flow.FlowId,
            flow.ProjectId,
            flow.Name,
            flow.Description,
            flow.SortOrder,
            lanes,
            stages,
            nodes,
            links,
            comments,
            metadata);
    }
}
