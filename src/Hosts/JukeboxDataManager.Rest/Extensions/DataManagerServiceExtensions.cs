using Jukebox.DataManager.Contracts;
using Jukebox.DataManager.Managers;
using Jukebox.DataManager.Managers.Mapping.AutoMapper.Song;

namespace Jukebox.DataManager.Rest.Extensions;

public static class DataManagerServiceExtensions
{
    public static IServiceCollection AddDataManager(this IServiceCollection services)
    {
        services.AddScoped<ISongManager, SongManager>();
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<SongProfile>();
        });

        return services;
    }
}