using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TwinApp.ProjectService.API.Models;

public record BfProgramMetadata(
    [property: BsonId, BsonRepresentation(BsonType.String)] string Id,
    string ProgramName,
    DateTime CreatedAt,
    ObjectId GridFsId // id for the raw .bf JSON 
);