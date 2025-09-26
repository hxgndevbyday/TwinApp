namespace TwinApp.ProjectService.API.Models.Mappers.Pipelines;

public static class PipelineRegistry
{
    private static Dictionary<string, ISectionPipeline> _pipelines = new Dictionary<string, ISectionPipeline>()
    {
        {"Programs", new ProgramsPipeline()},
        {"Commands", new CommandsPipeline()},
    };

    public static ISectionPipeline? GetPipeline(string sectionType) =>
        _pipelines.TryGetValue(sectionType, out var pipeline) ? pipeline : null;
}

