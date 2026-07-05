using Public.Infrastructure;
using Scalar.AspNetCore;
var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddInfrastructure(configuration);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "Public GateWay Api v1.";
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
app.UseCors("AllowedOrigins");

app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => Results.Ok(new
{
    Status = "Public Gateway API is running 🚀",
    Time = DateTime.Now,
    Message = "Welcome to the Portal API! Everything is operational. 🌟",
}));
app.Run();
