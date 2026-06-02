using AutoMapper;
using DAL = Jukebox.DataAccess.Contracts.DataContracts;
using BLL = Jukebox.DataManager.Contracts.DataContracts;

namespace Jukebox.DataManager.Managers.Mapping.AutoMapper.Artist
{
    public class ArtistProfile : Profile
    {
        public ArtistProfile()
        {
            CreateMap<DAL.Artist.ArtistSummary, BLL.Artist.ArtistSummary>();
            CreateMap<DAL.Artist.ArtistDetails, BLL.Artist.ArtistDetails>();
            CreateMap<DAL.Album.AlbumSummary, BLL.Album.AlbumSummary>();
            CreateMap<BLL.Artist.AddArtistRequest, DAL.Artist.AddArtistRequest>();
            CreateMap<BLL.Artist.UpdateArtistRequest, DAL.Artist.UpdateArtistRequest>();
            CreateMap<BLL.Artist.ListArtistsRequest, DAL.Artist.ListArtistsRequest>();
        }
    }
}