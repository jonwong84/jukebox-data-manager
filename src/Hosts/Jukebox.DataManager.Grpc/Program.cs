using Jukebox.DataAccess.Extensions;
using Jukebox.DataManager.Grpc.Services;
using Jukebox.DataManager.Managers.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

builder.Services.AddDataAccess();
builder.Services.AddDataManager();
builder.Services.AddGrpcReflection();

var app = builder.Build();

app.MapGrpcService<SongServiceImpl>();
app.MapGrpcService<ArtistServiceImpl>();
//app.MapGrpcService<AlbumServiceImpl>();

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.MapGet("/", () => "gRPC endpoint. Use a gRPC client to communicate.");

app.Run();