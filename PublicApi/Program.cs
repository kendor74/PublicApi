using Public.Infrastructure;
using Scalar.AspNetCore;
using System;
var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddInfrastructure(configuration);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.Title = "Portal Api v1.";
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

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
