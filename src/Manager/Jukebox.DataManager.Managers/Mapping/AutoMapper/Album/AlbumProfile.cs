using AutoMapper;
using DAL = Jukebox.DataAccess.Contracts.DataContracts;
using BLL = Jukebox.DataManager.Contracts.DataContracts;

namespace Jukebox.DataManager.Managers.Mapping.AutoMapper.Album;

public class AlbumProfile : Profile
{
    public AlbumProfile()
    {
        CreateMap<DAL.Album.AlbumDetails, BLL.Album.AlbumDetails>();
        CreateMap<DAL.Album.AlbumSummary, BLL.Album.AlbumSummary>();
        CreateMap<BLL.Album.AddAlbumRequest, DAL.Album.AddAlbumRequest>();
        CreateMap<BLL.Album.UpdateAlbumRequest, DAL.Album.UpdateAlbumRequest>();
        CreateMap<BLL.Album.ListAlbumsRequest, DAL.Album.ListAlbumsRequest>();
    }
}