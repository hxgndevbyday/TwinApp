
namespace TwinApp.ProjectService.Shared.DTOs
{
    
    public record BfProjectProcessingsDtos
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
        public string Version { get; init; } = string.Empty;

        public LightsSection Lights { get; init; } = new();
        public PartsSection Parts { get; init; } = new();
        public FixturesSection Features { get; init; } = new();
        public CellItemsSection CellItems { get; init; } = new();
        public FeaturesSection FeaturesSection { get; init; } = new();
        public MachinesSection MachinesSection { get; init; } = new();
        public SensorsSection SensorsSection { get; init; } = new();
        public SecurityDevicesSection SecurityDevicesSection { get; init; } = new();
        public ProgramsSection ProgramsSection { get; init; } = new();
        public TargetsSection TargetsSection { get; init; } = new();
        public WorkspacesSection WorkspacesSection { get; init; } = new();
        public WorkstationSection WorkstationSection { get; init; } = new();
        public PlcSection PlcSection { get; init; } = new();
        public MeasurementsSection MeasurementsSection { get; init; } = new();
        public ReportItemsSection ReportItemsSection { get; init; } = new();
        public MetrologyInfoSection MetrologyInfoSection { get; init; } = new();
    }

    public record LightsSection
    {
        public List<LightItem> lightItems { get; init; } = new();
    }

    public record LightItem
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
    }

    public record PartsSection
    {
        // to do 
    }

    public record FixturesSection
    {
        // to do 

    }

    public record CellItemsSection
    {
        // to do 

    }

    public record FeaturesSection
    {
        // to do 

    }

    public record MachinesSection
    {
        // to do 

    }

    public record SensorsSection
    {
        // to do 

    }

    public record SecurityDevicesSection
    {
        // to do 

    }

    public record ProgramsSection
    {
        // to do 

    }

    public record TargetsSection
    {
        // to do 

    }

    public record WorkspacesSection
    {
        // to do 

    }

    public record WorkstationSection
    {
        // to do 

    }

    public record PlcSection
    {
        // to do 

    }

    public record MeasurementsSection
    {
        // to do 

    }

    public record ReportItemsSection
    {
        // to do 

    }

    public record MetrologyInfoSection
    {
        // to do 

    }
}
