using MongoDB.Bson;
using TwinApp.ProjectService.API.Repository;

namespace TwinApp.ProjectService.API.Models.Mappers.Pipelines;

public class ProgramsPipeline : ISectionPipeline
{
    public string SectionType => "Programs";

    public async Task<IEnumerable<BsonDocument>> ExtractAsync(BsonDocument root, CancellationToken ct)
    {
        var _2programs = ProjectMappers.DummyExtractionMethod(root);

        
        Guid id = Guid.NewGuid();
        DateTime creationTime = DateTime.UtcNow;
        string sectionName = root["SectionType"].AsString;
        
        BsonDocument _commandsDocument = new BsonDocument
        {
            { "", "" },
        };
        BsonDocument _programsDocument = new  BsonDocument
        {
            { "EntityType", "Programs" }, // e.g., RobotExtension
            { "SectionType", sectionName },
            { "ParentId", "" }, // Link to parent section
            { "ProjectId", root["ProjectId"] },
            // Components will be added on next step
            { "Components", id.ToString() },
            { "CreatedAt", creationTime }
        };
        
        if (root["Components"] != null)
        {
            _commandsDocument.Add("EntityType", "Programs");
            _commandsDocument.Add("ParentId", id.ToString());
            _commandsDocument.Add("CreatedAt", creationTime);
            
            foreach (var item in root["Components"].AsBsonDocument.Elements)
            {
                if (item.Name.StartsWith("$")) { }
                else if (item.Name == "Children")
                {
                    await ExtractChildren(item, id.ToString(), ct);
                }
                else {_commandsDocument.Add(item.Name, item.Value);}
            }
            
            // async Task ExctractChildrens(BsonElement childrens)
            // {
            //     // Add a reference Id to the program 
            //     string commandsReferenceId = Guid.NewGuid().ToString();
            //     _commandsDocument.Add("CommandsReferenceId",commandsReferenceId );
            //         
            //     var commandsComponents = 
            //         await (PipelineRegistry.GetPipeline("Commands")
            //             ?.ProcessAsync(new BsonDocument(childrens) ,ct)!);
            //
            //     foreach (var command in commandsComponents)
            //     {
            //         _commandsDocument.Add("ParentId",commandsReferenceId );
            //     }
            //     // if your ProjectSection.Data is a dictionary converted from BsonDocument,
            //     // you can convert it back to BsonDocument or adapt the Flatten function to accept dictionary.
            //     var list = ProgramFlattening.FlattenProgramSection(new BsonDocument(childrens), commandsReferenceId);
            //     
            //     new BsonDocument(new Dictionary<string, BsonValue>("", ))
            //     return list;
            // }
            async Task<List<ProgramFlattening.ProjectEntityDto>> ExtractChildren(BsonElement childrens, string parentId, CancellationToken ct)
            {
                string commandsRefId = Guid.NewGuid().ToString();

                // Delegate to the Commands pipeline (returns BsonDocuments)
                var commandDocs = await (PipelineRegistry.GetPipeline("Commands")
                                             ?.ProcessAsync(new BsonDocument(childrens), ct) 
                                         ?? Task.FromResult(Enumerable.Empty<BsonDocument>()));

                // Convert docs -> DTOs
                var dtos = commandDocs
                    .Select(d => new ProgramFlattening.ProjectEntityDto(
                        ProjectId: root["ProjectId"].ToString(),
                        EntityType: d["EntityType"].AsString,
                        EntityId: Guid.NewGuid().ToString(),
                        ParentId: commandsRefId,
                        Components: d.ToDictionaryRecursive()
                    ))
                    .ToList();

                // Flatten nested children (optional, depending on use case)
                var flattened = ProgramFlattening.FlattenProgramSection(new BsonDocument(childrens), commandsRefId);
                dtos.AddRange(flattened);

                return dtos;
            }

            
        }
        
        return _2programs;
    }

    public Task<IEnumerable<BsonDocument>> TransformAsync(IEnumerable<BsonDocument> extracted, CancellationToken ct)
    {
        // Example: normalize children, flatten commands, etc.
        var transformed = extracted.Select(doc =>
        {
            doc["EntityType"] = "Program";
            return doc;
        });

        return Task.FromResult(transformed);
    }

    public async Task<IEnumerable<BsonDocument>> ProcessAsync(BsonDocument inputSection, CancellationToken ct)
    {
        return await TransformAsync(await ExtractAsync(inputSection, ct), ct);
    }
    
}
