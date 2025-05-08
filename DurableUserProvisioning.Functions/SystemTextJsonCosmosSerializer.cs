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
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public JsonSerializerOptions Options => _options;

    public override T FromStream<T>(Stream stream)
    {
        using var reader = new StreamReader(stream);
        string json = reader.ReadToEnd();
        return JsonSerializer.Deserialize<T>(json, _options)!;
    }

    public override Stream ToStream<T>(T input)
    {
        MemoryStream stream = new();
        Utf8JsonWriter writer = new(stream);
        JsonSerializer.Serialize(writer, input, _options);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }
}
