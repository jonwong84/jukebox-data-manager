using JukeboxDataManager.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<JukeboxDbContext>(options =>
    options.UseInMemoryDatabase("JukeboxDev"));

// Register SongManager for dependency injection
builder.Services.AddScoped<JukeboxDataManager.Data.Managers.ISongManager, JukeboxDataManager.Data.Managers.SongManager>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
