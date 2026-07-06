using FlowDesigner.Api.Controllers;
using FlowDesigner.Application.DTOs.Exports;
using FlowDesigner.Application.DTOs.Auth;
using FlowDesigner.Application.DTOs.Flows;
using FlowDesigner.Application.DTOs.Projects;
using FlowDesigner.Application.DTOs.Versions;
using FlowDesigner.Application.Interfaces.Authorization;
using FlowDesigner.Application.Interfaces.Services;
using FlowDesigner.Domain.Entities.Auth;
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
            [new SaveStageRequest(stageId, "Read", 1)],
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

    [Theory]
    [InlineData("Flow.Update")]
    [InlineData("Node.Update")]
    [InlineData("Link.Update")]
    [InlineData("Comment.Update")]
    public async Task SaveStructure_WhenAnyRequiredPermissionIsMissing_ReturnsForbidden(string deniedPermission)
    {
        await using var fixture = await TestFixture.CreateAsync(deniedPermission: deniedPermission);
        var flow = await fixture.CreateProjectAndFlowAsync();

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

        var forbidden = Assert.IsType<ObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status403Forbidden, forbidden.StatusCode);
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
            [new SaveStageRequest(stageId, "Stage", 1)],
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
            [new SaveStageRequest(stageId, "Stage", 1)],
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
            [new SaveStageRequest(Guid.NewGuid(), "Stage", 1)],
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
        Assert.Equal("Pick", document.RootElement.GetProperty("nodes")[0].GetProperty("name").GetString());
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

        public async Task<FlowDetailDto> CreateProjectAndFlowAsync()
        {
            var projectResult = await CreateProjectsController().Create(
                new CreateProjectRequest("Project", null),
                CancellationToken.None);
            var project = Assert.IsType<ProjectDetailDto>(Assert.IsType<CreatedAtActionResult>(projectResult.Result).Value);

            var flowResult = await CreateFlowsController().Create(
                project.ProjectId,
                new CreateFlowRequest("Flow", null),
                CancellationToken.None);

            return Assert.IsType<FlowDetailDto>(Assert.IsType<CreatedAtActionResult>(flowResult.Result).Value);
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
