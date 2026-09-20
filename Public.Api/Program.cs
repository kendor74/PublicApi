using Public.Api.Middlerware;
using Public.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddInfrastructure(configuration);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(
        builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if(app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options.Title = "Portal Api";
        options.Favicon = "favicon.svg";
        options.Theme = ScalarTheme.BluePlanet;
        options.Layout = ScalarLayout.Modern;
        options.ShowSidebar = true;
        options.HideTestRequestButton = false;
        options.HideSearch = false;
        options.DefaultOpenAllTags = true;
        options.ExpandAllResponses = true;
        options.OperationTitleSource = OperationTitleSource.Summary;
        options.DefaultFonts = true;
        options.HideClientButton = true;
        options.ExpandAllModelSections = false;
        options.OrderRequiredPropertiesFirst = true;
        options.HideDarkModeToggle = false;
        options.DocumentDownloadType = DocumentDownloadType.Both;

    });
}


app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

// Don't run your normal API rate-limit middleware
// against MCP transport requests.
app.UseWhen(
    context => !context.Request.Path.StartsWithSegments("/api/mcp"),
    branch =>
    {
        branch.UseMiddleware<ApiRateLimitMiddleware>();
    });

app.UseWhen(
    context =>
        context.Request.Path.Equals(
            "/api/Portal/sisos-custom-chain",
            StringComparison.OrdinalIgnoreCase),
    branch =>
    {
        branch.UseMiddleware<EnvironmentHeaderMiddleware>();
        branch.UseMiddleware<ApiTokenMiddleware>();
    });

app.UseAuthorization();

app.MapControllers();

app.MapReverseProxy();

app.MapGet("/", () => Results.Ok(new
{
    Status = "Public API is running 🚀",
    Time = DateTime.Now,
    Message = "Welcome to the Public API! Everything is operational. 🌟"
}));

app.Run();