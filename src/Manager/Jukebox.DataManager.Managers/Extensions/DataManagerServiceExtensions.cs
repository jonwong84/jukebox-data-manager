using Jukebox.DataManager.Contracts;
using Jukebox.DataManager.Managers.Mapping.AutoMapper.Album;
using Jukebox.DataManager.Managers.Mapping.AutoMapper.Artist;
using Jukebox.DataManager.Managers.Mapping.AutoMapper.Song;
using Microsoft.Extensions.DependencyInjection;

namespace Jukebox.DataManager.Managers.Extensions;

public static class DataManagerServiceExtensions
{
    public static IServiceCollection AddDataManager(this IServiceCollection services)
    {
        services.AddScoped<ISongManager, SongManager>();
        services.AddScoped<IArtistManager, ArtistManager>();
        services.AddScoped<IAlbumManager, AlbumManager>();
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<SongProfile>();
            cfg.AddProfile<ArtistProfile>();
            cfg.AddProfile<AlbumProfile>();
        });

        return services;
    }
}