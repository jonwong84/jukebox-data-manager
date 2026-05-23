using AutoMapper;
using DAL = Jukebox.DataAccess.Contracts.DataContracts;
using BLL = Jukebox.DataManager.Contracts.DataContracts;

namespace Jukebox.DataManager.Managers.Mapping.AutoMapper.Song
{
    public class SongProfile : Profile
    {
        public SongProfile()
        {
            CreateMap<DAL.Common.ArtistSummary, BLL.Common.ArtistSummary>();
            CreateMap<DAL.Common.AlbumSummary, BLL.Common.AlbumSummary>();
            CreateMap<DAL.Common.GenreSummary, BLL.Common.GenreSummary>();
            CreateMap<DAL.Song.SongSummary, BLL.Song.SongSummary>();
            CreateMap<DAL.Song.SongDetails, BLL.Song.SongDetails>();
        }
    }
}