using Microsoft.Azure.Cosmos;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace DurableUserProvisioning.Functions;

public class SystemTextJsonCosmosSerializer : CosmosSerializer
{
    private readonly JsonSerializerOptions _options;

    public SystemTextJsonCosmosSerializer()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // Convert PascalCase to camelCase
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };
    }

    public override T FromStream<T>(Stream stream)
    {
        if (stream == null || stream.CanRead == false)
        {
            return default!;
        }

        return JsonSerializer.Deserialize<T>(stream, _options)!;
    }

    public override Stream ToStream<T>(T input)
    {
        var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        JsonSerializer.Serialize(writer, input, _options);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }
}