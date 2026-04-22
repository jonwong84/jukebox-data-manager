using JukeboxDataManager.Data;
using JukeboxDataManager.Grpc.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddDbContext<JukeboxDbContext>(options =>
    options.UseInMemoryDatabase("JukeboxGrpcDev"));

var app = builder.Build();

app.MapGrpcService<JukeboxServiceImpl>();
app.MapGet("/", () => "gRPC endpoint. Use a gRPC client to communicate.");

app.Run();
