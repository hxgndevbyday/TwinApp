using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TwinApp.ProjectService.API.Models;
public record ProjectEntity(
    [property: BsonId, BsonRepresentation(BsonType.String)] string Id,
    string ProjectId,
    string ParentId,
    string Name,
    BsonDocument Data,
    ObjectId GridFsId // id for the raw .bf JSON 
);

public record SectionEntity(
    string EntityType,
    string SectionType,
    string ParentId,
    BsonDocument Components,
    DateTime CreatedAt
);
