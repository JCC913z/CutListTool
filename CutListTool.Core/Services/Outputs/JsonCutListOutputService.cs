using System.Text.Json;
using System.Text.Json.Serialization;
using CutListTool.Core.Models.OutsideDataHandlers;

namespace CutListTool.Core.Services.Outputs;

public sealed class JsonCutListOutputService
{
    private readonly JsonSerializerOptions jsonOptions;

    public JsonCutListOutputService()
    {
        jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        jsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public string Generate(CutListOutputData outputData)
    {
        return JsonSerializer.Serialize(outputData, jsonOptions);
    }
}