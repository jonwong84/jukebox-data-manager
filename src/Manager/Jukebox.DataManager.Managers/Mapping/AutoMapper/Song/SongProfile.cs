using AutoMapper;
using DAL = Jukebox.DataAccess.Contracts.DataContracts;
using BLL = Jukebox.DataManager.Contracts.DataContracts;

namespace Jukebox.DataManager.Managers.Mapping.AutoMapper.Song;

public class SongProfile : Profile
{
    public SongProfile()
    {
        CreateMap<DAL.Artist.ArtistSummary, BLL.Artist.ArtistSummary>();
        CreateMap<DAL.Album.AlbumSummary, BLL.Album.AlbumSummary>();
        CreateMap<DAL.Genre.GenreSummary, BLL.Genre.GenreSummary>();
        CreateMap<DAL.Song.SongSummary, BLL.Song.SongSummary>();
        CreateMap<DAL.Song.SongDetails, BLL.Song.SongDetails>();
        CreateMap<BLL.Song.AddSongRequest, DAL.Song.AddSongRequest>();
        CreateMap<BLL.Song.UpdateSongRequest, DAL.Song.UpdateSongRequest>();
        CreateMap<BLL.Song.ListSongsRequest, DAL.Song.ListSongsRequest>();
    }
}