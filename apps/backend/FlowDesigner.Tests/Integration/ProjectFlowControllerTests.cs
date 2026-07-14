using FlowDesigner.Api.Controllers;
using FlowDesigner.Application.DTOs.Exports;
using FlowDesigner.Application.DTOs.Auth;
using FlowDesigner.Application.DTOs.Flows;
using FlowDesigner.Application.DTOs.Projects;
using FlowDesigner.Application.DTOs.Transport;
using FlowDesigner.Application.DTOs.Versions;
using FlowDesigner.Application.Interfaces.Authorization;
using FlowDesigner.Application.Interfaces.Services;
using FlowDesigner.Domain.Entities.Auth;
using FlowDesigner.Domain.Entities.Core;
using FlowDesigner.Domain.Entities.Transport;
using FlowDesigner.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using Xunit;

namespace FlowDesigner.Tests.Integration;

public sealed class ProjectFlowControllerTests
{
    [Fact]
    public async Task CreateProject_ThenCreateFlow_PersistsProjectFlowAndDefaultStructure()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var projectsController = fixture.CreateProjectsController();

        var projectResponse = await projectsController.Create(
            new CreateProjectRequest(" Warehouse ", "Inbound flow"),
            CancellationToken.None);

        var createdProject = Assert.IsType<CreatedAtActionResult>(projectResponse.Result);
        var project = Assert.IsType<ProjectDetailDto>(createdProject.Value);
        Assert.Equal("Warehouse", project.Name);

        var flowsController = fixture.CreateFlowsController();
        var flowResponse = await flowsController.Create(
            project.ProjectId,
            new CreateFlowRequest("Receiving", "RFID read"),
            CancellationToken.None);

        var createdFlow = Assert.IsType<CreatedAtActionResult>(flowResponse.Result);
        var flow = Assert.IsType<FlowDetailDto>(createdFlow.Value);
        Assert.Equal(project.ProjectId, flow.ProjectId);
        Assert.Equal("Receiving", flow.Name);
        Assert.Single(flow.Lanes);
        Assert.Single(flow.Stages);

        Assert.Equal(1, await fixture.DbContext.Projects.CountAsync());
        Assert.Equal(1, await fixture.DbContext.Flows.CountAsync());
        Assert.Equal(1, await fixture.DbContext.Lanes.CountAsync());
        Assert.Equal(1, await fixture.DbContext.Stages.CountAsync());
    }

    [Fact]
    public async Task SaveStructure_PersistsLaneStageNodeLinkCommentAndIncrementsRevision()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var flow = await fixture.CreateProjectAndFlowAsync();
        var laneId = Guid.NewGuid();
        var stageId = Guid.NewGuid();
        var startNodeId = Guid.NewGuid();
        var endNodeId = Guid.NewGuid();
        var linkId = Guid.NewGuid();
        var commentId = Guid.NewGuid();

        var request = new SaveFlowStructureRequest(
            flow.FlowId,
            flow.CurrentRevision,
            [new SaveLaneRequest(laneId, "Operator", 1)],
            [new SaveStageRequest(stageId, "Read", "AUTO", 1)],
            [
                new SaveNodeRequest(startNodeId, laneId, stageId, "Start", "Start", null, 10, 20),
                new SaveNodeRequest(endNodeId, laneId, stageId, "End", "End", "Done", 110, 20),
            ],
            [new SaveLinkRequest(linkId, startNodeId, endNodeId, "next", null)],
            [new SaveCommentRequest(commentId, startNodeId, "Check label", 20, 40)],
            true,
            "initial structure");

        var response = await fixture.CreateFlowsController().SaveStructure(
            flow.ProjectId,
            flow.FlowId,
            request,
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        var saveResult = Assert.IsType<SaveFlowStructureResponse>(ok.Value);
        Assert.Equal(flow.FlowId, saveResult.FlowId);
        Assert.Equal(flow.CurrentRevision + 1, saveResult.ServerRevision);

        Assert.Equal(1, await fixture.DbContext.Lanes.CountAsync(lane => lane.FlowId == flow.FlowId));
        Assert.Equal(1, await fixture.DbContext.Stages.CountAsync(stage => stage.FlowId == flow.FlowId));
        Assert.Equal(2, await fixture.DbContext.Nodes.CountAsync(node => node.FlowId == flow.FlowId));
        Assert.Equal(1, await fixture.DbContext.Links.CountAsync(link => link.FlowId == flow.FlowId));
        Assert.Equal(1, await fixture.DbContext.Comments.CountAsync(comment => comment.FlowId == flow.FlowId));
        Assert.Equal(1, await fixture.DbContext.Versions.CountAsync(version => version.FlowId == flow.FlowId));
    }

    [Fact]
    public async Task SaveStructure_WhenPermissionIsMissing_ReturnsForbidden()
    {
        await using var fixture = await TestFixture.CreateAsync(deniedPermission: "Flow.Update");
        var flow = await fixture.CreateProjectAndFlowAsync();

        var request = new SaveFlowStructureRequest(
            flow.FlowId,
            flow.CurrentRevision,
            [],
            [],
            [],
            [],
            [],
            false,
            null);

        var response = await fixture.CreateFlowsController().SaveStructure(
            flow.ProjectId,
            flow.FlowId,
            request,
            CancellationToken.None);

        var forbidden = Assert.IsType<ObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status403Forbidden, forbidden.StatusCode);
        Assert.Equal(0, await fixture.DbContext.Versions.CountAsync());
    }

    [Fact]
    public async Task UpdateProject_AndUpdateFlow_PersistChanges()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var flow = await fixture.CreateProjectAndFlowAsync();

        var projectResponse = await fixture.CreateProjectsController().Update(
            flow.ProjectId,
            new UpdateProjectRequest("Updated project", "Updated project description"),
            CancellationToken.None);

        var projectOk = Assert.IsType<OkObjectResult>(projectResponse.Result);
        var project = Assert.IsType<ProjectDetailDto>(projectOk.Value);
        Assert.Equal("Updated project", project.Name);
        Assert.Equal("Updated project description", project.Description);

        var flowResponse = await fixture.CreateFlowsController().Update(
            flow.ProjectId,
            flow.FlowId,
            new UpdateFlowRequest("Updated flow", "Updated flow description"),
            CancellationToken.None);

        var flowOk = Assert.IsType<OkObjectResult>(flowResponse.Result);
        var updatedFlow = Assert.IsType<FlowDetailDto>(flowOk.Value);
        Assert.Equal("Updated flow", updatedFlow.Name);
        Assert.Equal("Updated flow description", updatedFlow.Description);

        Assert.Equal("Updated project", (await fixture.DbContext.Projects.SingleAsync()).Name);
        Assert.Equal("Updated flow", (await fixture.DbContext.Flows.SingleAsync()).Name);
    }

    [Fact]
    public async Task DuplicateFlow_CopiesStructureWithNewIds()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var flow = await fixture.CreateProjectAndFlowAsync();
        var laneId = Guid.NewGuid();
        var stageId = Guid.NewGuid();
        var startNodeId = Guid.NewGuid();
        var endNodeId = Guid.NewGuid();
        var linkId = Guid.NewGuid();
        var commentId = Guid.NewGuid();

        await fixture.SaveStructureAsync(
            flow,
            [new SaveLaneRequest(laneId, "Lane", 1)],
            [new SaveStageRequest(stageId, "Stage", "AUTO", 1)],
            [
                new SaveNodeRequest(startNodeId, laneId, stageId, "Start", "Start", null, 10, 20),
                new SaveNodeRequest(endNodeId, laneId, stageId, "End", "End", null, 110, 20),
            ],
            [new SaveLinkRequest(linkId, startNodeId, endNodeId, "next", null)],
            [new SaveCommentRequest(commentId, startNodeId, "Check label", 20, 40)],
            createVersion: false);

        var response = await fixture.CreateFlowsController().Duplicate(
            flow.ProjectId,
            flow.FlowId,
            new DuplicateFlowRequest(null),
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        var duplicate = Assert.IsType<FlowDetailDto>(ok.Value);

        Assert.NotEqual(flow.FlowId, duplicate.FlowId);
        Assert.Equal("Flow Copy", duplicate.Name);
        Assert.Single(duplicate.Lanes);
        Assert.Single(duplicate.Stages);
        Assert.Equal(2, duplicate.Nodes.Count);
        Assert.Single(duplicate.Links);
        Assert.Single(duplicate.Comments);
        Assert.DoesNotContain(duplicate.Lanes, lane => lane.LaneId == laneId);
        Assert.DoesNotContain(duplicate.Stages, stage => stage.StageId == stageId);
        Assert.DoesNotContain(duplicate.Nodes, node => node.NodeId == startNodeId || node.NodeId == endNodeId);
        Assert.DoesNotContain(duplicate.Links, link => link.LinkId == linkId);
        Assert.DoesNotContain(duplicate.Comments, comment => comment.CommentId == commentId);

        var duplicateLink = Assert.Single(duplicate.Links);
        Assert.Contains(duplicate.Nodes, node => node.NodeId == duplicateLink.SourceNodeId);
        Assert.Contains(duplicate.Nodes, node => node.NodeId == duplicateLink.TargetNodeId);

        var duplicateComment = Assert.Single(duplicate.Comments);
        Assert.Contains(duplicate.Nodes, node => node.NodeId == duplicateComment.NodeId);

    }

    [Fact]
    public async Task DeleteFlow_WithStructureAndVersions_RemovesFlowGraph()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var flow = await fixture.CreateProjectAndFlowAsync();
        var firstNodeId = Guid.NewGuid();
        var secondNodeId = Guid.NewGuid();

        await fixture.SaveStructureAsync(
            flow,
            [new SaveLaneRequest(Guid.NewGuid(), "Lane", 1)],
            [new SaveStageRequest(Guid.NewGuid(), "Stage", "AUTO", 1)],
            [
                new SaveNodeRequest(firstNodeId, null, null, "Start", "Start", null, 10, 20),
                new SaveNodeRequest(secondNodeId, null, null, "End", "End", null, 110, 20),
            ],
            [new SaveLinkRequest(Guid.NewGuid(), firstNodeId, secondNodeId, "next", null)],
            [new SaveCommentRequest(Guid.NewGuid(), firstNodeId, "Check label", 20, 40)],
            createVersion: true);

        var response = await fixture.CreateFlowsController().Delete(flow.ProjectId, flow.FlowId, CancellationToken.None);

        Assert.IsType<NoContentResult>(response);
        Assert.Equal(0, await fixture.DbContext.Flows.CountAsync());
        Assert.Equal(0, await fixture.DbContext.Lanes.CountAsync());
        Assert.Equal(0, await fixture.DbContext.Stages.CountAsync());
        Assert.Equal(0, await fixture.DbContext.Nodes.CountAsync());
        Assert.Equal(0, await fixture.DbContext.Links.CountAsync());
        Assert.Equal(0, await fixture.DbContext.Comments.CountAsync());
        Assert.Equal(0, await fixture.DbContext.Versions.CountAsync());
    }

    [Fact]
    public async Task FlowVersions_CreateCompareAndRestore_PersistExpectedSnapshots()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var flow = await fixture.CreateProjectAndFlowAsync();
        var laneId = Guid.NewGuid();
        var stageId = Guid.NewGuid();
        var firstNodeId = Guid.NewGuid();
        var secondNodeId = Guid.NewGuid();

        await fixture.SaveStructureAsync(
            flow,
            [new SaveLaneRequest(laneId, "Lane", 1)],
            [new SaveStageRequest(stageId, "Stage", "AUTO", 1)],
            [new SaveNodeRequest(firstNodeId, laneId, stageId, "Task", "Pick", null, 10, 10)],
            [],
            [],
            createVersion: false);

        var versionController = fixture.CreateFlowVersionsController();
        var firstVersion = await CreateVersionAsync(versionController, flow.ProjectId, flow.FlowId, "before change");

        var currentFlow = await fixture.CreateFlowsController().Get(flow.ProjectId, flow.FlowId, CancellationToken.None);
        var current = Assert.IsType<FlowDetailDto>(Assert.IsType<OkObjectResult>(currentFlow.Result).Value);

        await fixture.SaveStructureAsync(
            current,
            [new SaveLaneRequest(laneId, "Lane", 1)],
            [new SaveStageRequest(stageId, "Stage", "AUTO", 1)],
            [
                new SaveNodeRequest(firstNodeId, laneId, stageId, "Task", "Pick updated", null, 20, 10),
                new SaveNodeRequest(secondNodeId, laneId, stageId, "Task", "Pack", null, 120, 10),
            ],
            [new SaveLinkRequest(Guid.NewGuid(), firstNodeId, secondNodeId, "next", null)],
            [],
            createVersion: false);

        var secondVersion = await CreateVersionAsync(versionController, flow.ProjectId, flow.FlowId, "after change");

        var compareResponse = await versionController.Compare(
            flow.ProjectId,
            flow.FlowId,
            firstVersion.VersionId,
            secondVersion.VersionId,
            CancellationToken.None);
        var compareOk = Assert.IsType<OkObjectResult>(compareResponse.Result);
        var compare = Assert.IsType<FlowVersionCompareResponse>(compareOk.Value);
        Assert.Contains(compare.NodeDiffs, diff => diff.ChangeType == "Updated" && diff.Label == "Pick updated");
        Assert.Contains(compare.NodeDiffs, diff => diff.ChangeType == "Added" && diff.Label == "Pack");
        Assert.Contains(compare.LinkDiffs, diff => diff.ChangeType == "Added");

        var restoreResponse = await versionController.Restore(
            flow.ProjectId,
            flow.FlowId,
            firstVersion.VersionId,
            CancellationToken.None);
        var restoreOk = Assert.IsType<OkObjectResult>(restoreResponse.Result);
        var restore = Assert.IsType<RestoreFlowVersionResponse>(restoreOk.Value);
        Assert.Equal(flow.FlowId, restore.FlowId);

        Assert.Equal(1, await fixture.DbContext.Nodes.CountAsync(node => node.FlowId == flow.FlowId));
        Assert.Equal("Pick", (await fixture.DbContext.Nodes.SingleAsync(node => node.FlowId == flow.FlowId)).Name);
        Assert.Equal(3, await fixture.DbContext.Versions.CountAsync(version => version.FlowId == flow.FlowId));
    }

    [Fact]
    public async Task SaveStructure_WhenClientRevisionIsStale_ReturnsConflict()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var flow = await fixture.CreateProjectAndFlowAsync();
        await fixture.SaveStructureAsync(flow, [], [], [], [], [], createVersion: false);

        var response = await fixture.CreateFlowsController().SaveStructure(
            flow.ProjectId,
            flow.FlowId,
            new SaveFlowStructureRequest(
                flow.FlowId,
                flow.CurrentRevision,
                [],
                [],
                [],
                [],
                [],
                false,
                null),
            CancellationToken.None);

        var conflict = Assert.IsType<ConflictObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status409Conflict, conflict.StatusCode);
    }

    [Fact]
    public async Task VersionCompare_WhenVersionDoesNotExist_ReturnsNotFound()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var flow = await fixture.CreateProjectAndFlowAsync();
        var versionController = fixture.CreateFlowVersionsController();
        var version = await CreateVersionAsync(versionController, flow.ProjectId, flow.FlowId, "baseline");

        var response = await versionController.Compare(
            flow.ProjectId,
            flow.FlowId,
            version.VersionId,
            Guid.NewGuid(),
            CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFound.StatusCode);
    }

    [Fact]
    public async Task VersionRestore_WhenVersionDoesNotExist_ReturnsNotFound()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var flow = await fixture.CreateProjectAndFlowAsync();

        var response = await fixture.CreateFlowVersionsController().Restore(
            flow.ProjectId,
            flow.FlowId,
            Guid.NewGuid(),
            CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFound.StatusCode);
    }

    [Fact]
    public async Task ExportJson_ReturnsStructuredFlowFile()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var flow = await fixture.CreateProjectAndFlowAsync();
        var nodeId = Guid.NewGuid();

        await fixture.SaveStructureAsync(
            flow,
            [new SaveLaneRequest(Guid.NewGuid(), "Lane", 1)],
            [new SaveStageRequest(Guid.NewGuid(), "Stage", "AUTO", 1)],
            [new SaveNodeRequest(nodeId, null, null, "Task", "Pick", "Pick item", 10, 20)],
            [],
            [],
            createVersion: false);

        var response = await fixture.CreateExportsController().ExportJson(flow.ProjectId, flow.FlowId, CancellationToken.None);

        var file = Assert.IsType<FileContentResult>(response);
        Assert.Equal("application/json", file.ContentType);

        using var document = JsonDocument.Parse(Encoding.UTF8.GetString(file.FileContents));
        Assert.Equal(1, document.RootElement.GetProperty("schemaVersion").GetInt32());
        Assert.Equal(flow.FlowId, document.RootElement.GetProperty("flow").GetProperty("flowId").GetGuid());
        Assert.Equal("Pick", document.RootElement.GetProperty("raw").GetProperty("nodes")[0].GetProperty("name").GetString());
    }

    [Fact]
    public async Task ExportMermaid_ReturnsFlowchartContent()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var flow = await fixture.CreateProjectAndFlowAsync();
        var firstNodeId = Guid.NewGuid();
        var secondNodeId = Guid.NewGuid();

        await fixture.SaveStructureAsync(
            flow,
            [],
            [],
            [
                new SaveNodeRequest(firstNodeId, null, null, "Task", "Pick", null, 10, 20),
                new SaveNodeRequest(secondNodeId, null, null, "Task", "Pack", null, 110, 20),
            ],
            [new SaveLinkRequest(Guid.NewGuid(), firstNodeId, secondNodeId, "next", null)],
            [new SaveCommentRequest(Guid.NewGuid(), firstNodeId, "Check label", 20, 40)],
            createVersion: false);

        var response = await fixture.CreateExportsController().ExportMermaid(
            flow.ProjectId,
            flow.FlowId,
            new MermaidExportRequest("flowchart", "LR", true),
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        var result = Assert.IsType<TextExportResponse>(ok.Value);
        Assert.EndsWith(".mmd", result.FileName);
        Assert.Contains("flowchart LR", result.Content);
        Assert.Contains("Pick", result.Content);
        Assert.Contains("-->|\"next\"|", result.Content);
        Assert.Contains("%% Comment: Check label", result.Content);
    }

    [Fact]
    public async Task TransportStructure_SaveLoadAndVersionSnapshot_PreserveTransportAttributes()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var flow = await fixture.CreateProjectAndFlowAsync(FlowTypes.Transport);
        var (commandId, equipmentId, locationId) = await fixture.CreateTransportMastersAsync(flow.ProjectId);
        var nodeId = Guid.NewGuid();

        await fixture.SaveStructureAsync(
            flow,
            [],
            [],
            [new SaveNodeRequest(nodeId, null, null, "Task", "Move", null, 10, 20, commandId, locationId, equipmentId, "WRITE")],
            [],
            [],
            createVersion: true);

        var getResponse = await fixture.CreateFlowsController().Get(flow.ProjectId, flow.FlowId, CancellationToken.None);
        var loaded = Assert.IsType<FlowDetailDto>(Assert.IsType<OkObjectResult>(getResponse.Result).Value);
        var node = Assert.Single(loaded.Nodes);
        Assert.Equal(commandId, node.CommandId);
        Assert.Equal(equipmentId, node.EquipmentId);
        Assert.Equal(locationId, node.LocationId);
        Assert.Equal("WRITE", node.RwType);

        var version = Assert.Single(await fixture.DbContext.Versions.Where(item => item.FlowId == flow.FlowId).ToListAsync());
        using var snapshot = JsonDocument.Parse(version.SnapshotJson);
        var snapshotNode = snapshot.RootElement.GetProperty("nodes")[0];
        Assert.Equal(commandId, snapshotNode.GetProperty("commandId").GetGuid());
        Assert.Equal(equipmentId, snapshotNode.GetProperty("equipmentId").GetGuid());
        Assert.Equal(locationId, snapshotNode.GetProperty("locationId").GetGuid());
        Assert.Equal("WRITE", snapshotNode.GetProperty("rwType").GetString());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task TransportStructure_InvalidProjectOrDeletedLocation_ReturnsValidationError(bool deleted)
    {
        await using var fixture = await TestFixture.CreateAsync();
        var flow = await fixture.CreateProjectAndFlowAsync(FlowTypes.Transport);
        var locationProjectId = flow.ProjectId;
        if (!deleted)
        {
            var otherProjectResult = await fixture.CreateProjectsController().Create(new CreateProjectRequest("Other", null), CancellationToken.None);
            locationProjectId = Assert.IsType<ProjectDetailDto>(Assert.IsType<CreatedAtActionResult>(otherProjectResult.Result).Value).ProjectId;
        }

        var location = fixture.CreateLocation(locationProjectId, deleted);
        fixture.DbContext.TransportLocations.Add(location);
        await fixture.DbContext.SaveChangesAsync();

        var response = await fixture.CreateFlowsController().SaveStructure(
            flow.ProjectId,
            flow.FlowId,
            new SaveFlowStructureRequest(
                flow.FlowId,
                flow.CurrentRevision,
                [],
                [],
                [new SaveNodeRequest(Guid.NewGuid(), null, null, "Task", "Move", null, 10, 20, LocationId: location.LocationId)],
                [],
                [],
                false,
                null),
            CancellationToken.None);

        var validation = Assert.IsType<UnprocessableEntityObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, validation.StatusCode);
        Assert.Contains("location", validation.Value?.ToString() ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task NormalStructure_WithTransportValues_ClearsAllTransportAttributes()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var flow = await fixture.CreateProjectAndFlowAsync(FlowTypes.Normal);

        await fixture.SaveStructureAsync(
            flow,
            [],
            [],
            [new SaveNodeRequest(Guid.NewGuid(), null, null, "Task", "Normal", null, 10, 20, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "READ")],
            [],
            [],
            createVersion: false);

        var node = Assert.Single(await fixture.DbContext.Nodes.Where(item => item.FlowId == flow.FlowId).ToListAsync());
        Assert.Null(node.CommandId);
        Assert.Null(node.EquipmentId);
        Assert.Null(node.LocationId);
        Assert.Equal("NONE", node.RwType);
    }

    [Fact]
    public async Task UpdateFlow_FromTransportToNormal_ClearsPersistedTransportAttributes()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var flow = await fixture.CreateProjectAndFlowAsync(FlowTypes.Transport);
        var (commandId, equipmentId, locationId) = await fixture.CreateTransportMastersAsync(flow.ProjectId);

        await fixture.SaveStructureAsync(
            flow,
            [],
            [],
            [new SaveNodeRequest(Guid.NewGuid(), null, null, "Task", "Move", null, 10, 20, commandId, locationId, equipmentId, "READ")],
            [],
            [],
            createVersion: false);

        var update = await fixture.CreateFlowsController().Update(
            flow.ProjectId,
            flow.FlowId,
            new UpdateFlowRequest(flow.Name, flow.Description, FlowTypes.Normal),
            CancellationToken.None);
        var updated = Assert.IsType<FlowDetailDto>(Assert.IsType<OkObjectResult>(update.Result).Value);
        var node = Assert.Single(updated.Nodes);
        Assert.Null(node.CommandId);
        Assert.Null(node.EquipmentId);
        Assert.Null(node.LocationId);
        Assert.Equal("NONE", node.RwType);
    }

    [Fact]
    public async Task DuplicateTransportFlow_RetainsMasterReferencesAndRemapsNodeLinks()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var flow = await fixture.CreateProjectAndFlowAsync(FlowTypes.Transport);
        var (commandId, equipmentId, locationId) = await fixture.CreateTransportMastersAsync(flow.ProjectId);
        var firstNodeId = Guid.NewGuid();
        var secondNodeId = Guid.NewGuid();

        await fixture.SaveStructureAsync(
            flow,
            [],
            [],
            [
                new SaveNodeRequest(firstNodeId, null, null, "Task", "Move", null, 10, 20, commandId, locationId, equipmentId, "WRITE"),
                new SaveNodeRequest(secondNodeId, null, null, "Task", "End", null, 110, 20),
            ],
            [new SaveLinkRequest(Guid.NewGuid(), firstNodeId, secondNodeId, null, null)],
            [],
            createVersion: false);

        var response = await fixture.CreateFlowsController().Duplicate(flow.ProjectId, flow.FlowId, new DuplicateFlowRequest(null), CancellationToken.None);
        var duplicate = Assert.IsType<FlowDetailDto>(Assert.IsType<OkObjectResult>(response.Result).Value);
        Assert.NotEqual(flow.FlowId, duplicate.FlowId);
        var copiedNode = Assert.Single(duplicate.Nodes, item => item.Name == "Move");
        Assert.NotEqual(firstNodeId, copiedNode.NodeId);
        Assert.Equal(commandId, copiedNode.CommandId);
        Assert.Equal(equipmentId, copiedNode.EquipmentId);
        Assert.Equal(locationId, copiedNode.LocationId);
        Assert.Equal("WRITE", copiedNode.RwType);
        var link = Assert.Single(duplicate.Links);
        Assert.Contains(duplicate.Nodes, item => item.NodeId == link.SourceNodeId);
        Assert.Contains(duplicate.Nodes, item => item.NodeId == link.TargetNodeId);
    }

    [Fact]
    public async Task VehicleModel_GlobalCrud_ValidatesTypeAndDuplicateCode()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var manufacturer = new TransportManufacturer { ManufacturerId = Guid.NewGuid(), Name = "Maker", VehicleType = "AGF", SortOrder = 1, CreatedAtUtc = DateTime.UtcNow, UpdatedAtUtc = DateTime.UtcNow };
        fixture.DbContext.TransportManufacturers.Add(manufacturer); await fixture.DbContext.SaveChangesAsync();
        var controller = fixture.CreateTransportMastersController();
        var request = new SaveTransportVehicleModelRequest(manufacturer.ManufacturerId, "AGF", " model-1 ", "Model 1", null, null, true);
        var created = Assert.IsType<TransportVehicleModelDto>(Assert.IsType<OkObjectResult>((await controller.CreateVehicleModel(request, CancellationToken.None)).Result).Value);
        Assert.Equal("MODEL-1", created.ModelCode);
        Assert.True(created.IsActive);
        Assert.IsType<UnprocessableEntityObjectResult>((await controller.CreateVehicleModel(request, CancellationToken.None)).Result);
        var invalid = request with { VehicleType = "AMR", ModelCode = "AMR-1" };
        Assert.IsType<UnprocessableEntityObjectResult>((await controller.CreateVehicleModel(invalid, CancellationToken.None)).Result);
    }

    private static async Task<FlowVersionSummaryDto> CreateVersionAsync(
        FlowVersionsController controller,
        Guid projectId,
        Guid flowId,
        string comment)
    {
        var response = await controller.Create(projectId, flowId, new CreateFlowVersionRequest(comment), CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(response.Result);
        return Assert.IsType<FlowVersionSummaryDto>(ok.Value);
    }

    private sealed class TestFixture : IAsyncDisposable
    {
        private readonly SqliteConnection connection;
        private readonly FakeCurrentUserService currentUserService;
        private readonly FakePermissionService permissionService;

        private TestFixture(
            SqliteConnection connection,
            AppDbContext dbContext,
            FakeCurrentUserService currentUserService,
            FakePermissionService permissionService)
        {
            this.connection = connection;
            DbContext = dbContext;
            this.currentUserService = currentUserService;
            this.permissionService = permissionService;
        }

        public AppDbContext DbContext { get; }

        public static async Task<TestFixture> CreateAsync(string? deniedPermission = null)
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

            var dbContext = new AppDbContext(options);
            await dbContext.Database.EnsureCreatedAsync();

            var userId = Guid.NewGuid();
            dbContext.Users.Add(new User
            {
                UserId = userId,
                GitHubId = userId.ToString("N"),
                UserName = "tester",
                DisplayName = "Tester",
                CreatedAtUtc = DateTime.UtcNow,
            });
            dbContext.Roles.Add(new Role
            {
                RoleId = Guid.NewGuid(),
                RoleCode = "OWNER",
                Name = "Owner",
            });
            await dbContext.SaveChangesAsync();

            return new TestFixture(
                connection,
                dbContext,
                new FakeCurrentUserService(userId),
                new FakePermissionService(deniedPermission));
        }

        public ProjectsController CreateProjectsController()
        {
            return WithHttpContext(new ProjectsController(DbContext, currentUserService));
        }

        public FlowsController CreateFlowsController()
        {
            return WithHttpContext(new FlowsController(DbContext, currentUserService, permissionService));
        }

        public FlowVersionsController CreateFlowVersionsController()
        {
            return WithHttpContext(new FlowVersionsController(DbContext, currentUserService));
        }

        public ExportsController CreateExportsController()
        {
            return WithHttpContext(new ExportsController(DbContext));
        }

        public TransportMastersController CreateTransportMastersController() => WithHttpContext(new TransportMastersController(DbContext, currentUserService));

        public async Task<FlowDetailDto> CreateProjectAndFlowAsync(string? flowType = null)
        {
            var projectResult = await CreateProjectsController().Create(
                new CreateProjectRequest("Project", null),
                CancellationToken.None);
            var project = Assert.IsType<ProjectDetailDto>(Assert.IsType<CreatedAtActionResult>(projectResult.Result).Value);

            var flowResult = await CreateFlowsController().Create(
                project.ProjectId,
                new CreateFlowRequest("Flow", null, flowType),
                CancellationToken.None);

            return Assert.IsType<FlowDetailDto>(Assert.IsType<CreatedAtActionResult>(flowResult.Result).Value);
        }

        public TransportLocation CreateLocation(Guid projectId, bool deleted = false)
        {
            var now = DateTime.UtcNow;
            return new TransportLocation
            {
                LocationId = Guid.NewGuid(),
                ProjectId = projectId,
                Name = "P1",
                LocationType = "経由点",
                SortOrder = 1,
                IsDeleted = deleted,
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
                DeletedAtUtc = deleted ? now : null,
            };
        }

        public async Task<(Guid CommandId, Guid EquipmentId, Guid LocationId)> CreateTransportMastersAsync(Guid projectId)
        {
            var now = DateTime.UtcNow;
            var manufacturer = new TransportManufacturer
            {
                ManufacturerId = Guid.NewGuid(),
                Name = "Maker",
                VehicleType = "AGV",
                SortOrder = 1,
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
            };
            var command = new TransportCommand
            {
                CommandId = Guid.NewGuid(),
                ManufacturerId = manufacturer.ManufacturerId,
                CommandName = "Move",
                ProcessType = "移動",
                SortOrder = 1,
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
            };
            var equipment = new TransportEquipment
            {
                EquipmentId = Guid.NewGuid(),
                ProjectId = projectId,
                Name = "PLC1",
                Category = "PLC",
                SortOrder = 1,
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
            };
            var location = CreateLocation(projectId);
            DbContext.TransportManufacturers.Add(manufacturer);
            DbContext.TransportCommands.Add(command);
            DbContext.TransportEquipments.Add(equipment);
            DbContext.TransportLocations.Add(location);
            await DbContext.SaveChangesAsync();
            return (command.CommandId, equipment.EquipmentId, location.LocationId);
        }

        public async Task<SaveFlowStructureResponse> SaveStructureAsync(
            FlowDetailDto flow,
            IReadOnlyList<SaveLaneRequest> lanes,
            IReadOnlyList<SaveStageRequest> stages,
            IReadOnlyList<SaveNodeRequest> nodes,
            IReadOnlyList<SaveLinkRequest> links,
            IReadOnlyList<SaveCommentRequest> comments,
            bool createVersion)
        {
            var response = await CreateFlowsController().SaveStructure(
                flow.ProjectId,
                flow.FlowId,
                new SaveFlowStructureRequest(
                    flow.FlowId,
                    flow.CurrentRevision,
                    lanes,
                    stages,
                    nodes,
                    links,
                    comments,
                    createVersion,
                    null),
                CancellationToken.None);
            var ok = Assert.IsType<OkObjectResult>(response.Result);
            return Assert.IsType<SaveFlowStructureResponse>(ok.Value);
        }

        public async ValueTask DisposeAsync()
        {
            await DbContext.DisposeAsync();
            await connection.DisposeAsync();
        }

        private static T WithHttpContext<T>(T controller)
            where T : ControllerBase
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext(),
            };
            return controller;
        }
    }

    private sealed class FakeCurrentUserService(Guid userId) : ICurrentUserService
    {
        public Task<CurrentUserDto?> GetCurrentUserAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<CurrentUserDto?>(null);
        }

        public Guid? GetCurrentUserId()
        {
            return userId;
        }

        public bool IsAuthenticated()
        {
            return true;
        }
    }

    private sealed class FakePermissionService(string? deniedPermission) : IPermissionService
    {
        public Task<bool> CanAsync(Guid userId, Guid projectId, string permissionCode, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(!string.Equals(permissionCode, deniedPermission, StringComparison.OrdinalIgnoreCase));
        }

        public Task<IReadOnlyList<string>> GetPermissionsAsync(Guid userId, Guid projectId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<string>>([]);
        }

        public Task<ProjectPermissionDto?> GetProjectPermissionAsync(Guid userId, Guid projectId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<ProjectPermissionDto?>(null);
        }
    }
}
