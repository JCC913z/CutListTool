using System.Text.Json.Serialization;
using CutListTool.Api.Contracts;
using CutListTool.Core.Models.OutsideDataHandlers;
using CutListTool.Core.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter()
    );
});

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPost(
    "/api/cutlists/generate",
    (CutListApiRequest apiRequest) =>
    {
        CutListResult result = CutListEngine.Generate(
            apiRequest.Input,
            apiRequest.Request
        );

        return Results.Ok(result);
    }
)
.WithName("GenerateCutList");

app.Run();
