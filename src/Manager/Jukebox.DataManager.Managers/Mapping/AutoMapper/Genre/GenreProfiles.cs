using AutoMapper;
using DAL = Jukebox.DataAccess.Contracts.DataContracts;
using BLL = Jukebox.DataManager.Contracts.DataContracts;

namespace Jukebox.DataManager.Managers.Mapping.AutoMapper.Genre;

public class GenreProfile : Profile
{
    public GenreProfile()
    {
        CreateMap<DAL.Genre.GenreSummary, BLL.Genre.GenreSummary>();
        CreateMap<DAL.Genre.GenreDetails, BLL.Genre.GenreDetails>();
        CreateMap<BLL.Genre.AddGenreRequest, DAL.Genre.AddGenreRequest>();
        CreateMap<BLL.Genre.UpdateGenreRequest, DAL.Genre.UpdateGenreRequest>();
        CreateMap<BLL.Genre.ListGenresRequest, DAL.Genre.ListGenresRequest>();
    }
}