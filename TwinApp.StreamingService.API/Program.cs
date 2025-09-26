using System.Text;
using Microsoft.Extensions.Options;
using NATS.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(p => p.AddDefaultPolicy(policy =>
    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
// Bind NATS section
builder.Services.Configure<NatsOptions>(builder.Configuration.GetSection("NATS"));

var app = builder.Build();

app.UseCors();

// Health endpoint
app.MapGet("/health", () => "StreamingService alive");

// Background NATS subscription
var lifetime = app.Lifetime;
lifetime.ApplicationStarted.Register(() =>
{
    // Get NATS options
    var options = app.Services.GetRequiredService<IOptions<NatsOptions>>().Value;

    var opts = ConnectionFactory.GetDefaultOptions();
    opts.Url = options.Url;

    if (!string.IsNullOrEmpty(options.User))
        opts.User = options.User;
    if (!string.IsNullOrEmpty(options.Password))
        opts.Password = options.Password;

    var conn = new ConnectionFactory().CreateConnection(opts);


    var sub1 = conn.SubscribeAsync("robots.*.state");
    sub1.MessageHandler += (sender, args) =>
    {
        var json = Encoding.UTF8.GetString(args.Message.Data);
        Console.WriteLine($"[NATS] Robot state received: {json}");
        // TO-DO: push to SignalR clients
    };
    sub1.Start();
    
    var sub2 = conn.SubscribeAsync("test");
    sub2.MessageHandler += (sender, args) =>
    {
        var json = Encoding.UTF8.GetString(args.Message.Data);
        Console.WriteLine($"[NATS] Robot state received: {json}");
        // TO-DO: push to SignalR clients
    };
    sub2.Start();
    
    var sub3 = conn.SubscribeAsync("robots.state");
    sub3.MessageHandler += (sender, args) =>
    {
        var json = Encoding.UTF8.GetString(args.Message.Data);
        Console.WriteLine($"[NATS] robots.state: {json}");
        // TO-DO: push to SignalR clients
    };
    sub3.Start();
});

app.Run();

public record NatsOptions
{
    public string Url { get; set; } = "";
    public string User { get; set; } = "";
    public string Password { get; set; } = "";
}
