using System.Text;
using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace TwinApp.ProjectService.API.Models.Mappers;

public static class ProjectMappers
{
    // Sections that map to ECS entities in the 3D simulation
    public static readonly string[] EcsSections =
    {
        "Lights",
        "Parts",
        "Fixtures",
        "CellItems",
        "Features",
        "Machines",
        "Sensors",
        "SecurityDevices",
        "Targets",
        "Workspaces",
        "Workstation",
        "PLC" // Here Optionally, need to check if worthy
    };

    // Sections that are purely metadata / descriptive
    public static readonly string[] MetadataSections =
    {
        "$id",
        "$type",
        "Version",
        "Programs",
        "Measurements",
        "ReportItems",
        "MetrologyInfo",
        "Id",
        "Name",
        "SalesInfo"
    };

    public static MemoryStream FileToBsonDoc(IFormFile file)
    {
        using var reader = new StreamReader(file.OpenReadStream());
        var json = reader.ReadToEnd();

        // Parse JSON to BsonDocument
        var doc = BsonDocument.Parse(json);

        // Convert to BSON bytes
        var bsonBytes = doc.ToBson();

        // Write bytes to MemoryStream
        var ms = new MemoryStream(bsonBytes);
        ms.Position = 0; // Reset stream
        return ms;
    }

    public static Stream BsonToJsonStreamAsync(Stream bsonStream)
    {
        if (bsonStream == null)
            throw new ArgumentNullException(nameof(bsonStream));

        // Ensure stream is at the beginning
        if (bsonStream.CanSeek)
            bsonStream.Position = 0;

        // Deserialize BSON to BsonDocument
        var doc = BsonSerializer.Deserialize<BsonDocument>(bsonStream);
        
        // Convert BsonDocument to JSON string
        var json = doc.ToJson(new JsonWriterSettings { Indent = true });

        // Convert JSON string to UTF-8 stream
        var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        memoryStream.Position = 0;
        return memoryStream;
    }
    
    public static (List<BsonDocument> ecsEntities, List<BsonDocument> metadata) DocumentToProjectEntities(BsonDocument doc)
    {
        var ecsEntities = new List<BsonDocument>();
        var metadata = new List<BsonDocument>();

        foreach (var section in EcsSections.Concat(MetadataSections))
        {
            if (!doc.Contains(section)) 
                continue;

            var value = doc[section];

            if (EcsSections.Contains(section))
            {
                // Don't add top-level ECS entity
                // Instead, process only nested items if they exist
                if (value.AsBsonDocument.Contains("$values"))
                {
                    var parentId = Guid.NewGuid().ToString();
                    var nestedEntities = ExtractNestedEntities(new BsonDocument
                    {
                        { "Components", value },
                        { "SectionType", section },
                        { "ProjectId", doc.Contains("Id") ? doc["Id"].ToString() : Guid.NewGuid().ToString() }
                    });

                    ecsEntities.AddRange(nestedEntities);
                }
            }
            else
            {
                // Metadata section
                metadata.Add(new BsonDocument
                {
                    { "SectionType", section },
                    { "Data", value },
                    { "CreatedAt", DateTime.UtcNow }
                });
            }
        }


        return (ecsEntities, metadata);
    }
    
    public static List<BsonDocument> ExtractNestedEntities(BsonDocument sectionEntity)
    {
        var nestedEntities = new List<BsonDocument>();

        if (sectionEntity["Components"].AsBsonDocument.Contains("$values"))
        {
            // Step A: assign _id if missing
            if (!sectionEntity.Contains("_id"))
            {
                sectionEntity["_id"] = "";
            }

            foreach (var item in sectionEntity["Components"]["$values"].AsBsonArray)
            {
                var entity = new BsonDocument
                {
                    { "EntityType", item["$type"].AsString.Split(',')[0].Split('.').Last() }, // e.g., RobotExtension
                    { "SectionType", sectionEntity["SectionType"].AsString },
                    { "ParentId", sectionEntity["_id"] }, // Link to parent section
                    { "ProjectId", sectionEntity["ProjectId"] },
                    { "Components", item.AsBsonDocument }, // Keep nested components
                    { "CreatedAt", DateTime.UtcNow }
                };
                nestedEntities.Add(entity);
            }
        }

        return nestedEntities;
    }


}