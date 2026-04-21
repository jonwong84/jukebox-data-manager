using Grpc.Core;
using JukeboxDataManager.Data;
using JukeboxDataManager.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace JukeboxDataManager.Grpc.Services;

public class JukeboxServiceImpl : JukeboxService.JukeboxServiceBase
{
    private readonly JukeboxDbContext _context;

    public JukeboxServiceImpl(JukeboxDbContext context)
    {
        _context = context;
    }

    // Songs
    public override async Task<ListSongsResponse> ListSongs(ListSongsRequest request, ServerCallContext context)
    {
        var songs = await _context.Songs.ToListAsync();
        var response = new ListSongsResponse();
        response.Songs.AddRange(songs.Select(MapSong));
        return response;
    }

    public override async Task<SongResponse> GetSong(GetSongRequest request, ServerCallContext context)
    {
        var song = await _context.Songs.FindAsync(request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Song {request.Id} not found"));
        return MapSong(song);
    }

    public override async Task<SongResponse> CreateSong(CreateSongRequest request, ServerCallContext context)
    {
        var song = new Song
        {
            Title = request.Title,
            ArtistId = request.ArtistId,
            AlbumId = request.AlbumId != 0 ? request.AlbumId : null,
            Duration = TimeSpan.FromSeconds(request.DurationSeconds),
            Genre = string.IsNullOrEmpty(request.Genre) ? null : request.Genre,
            TrackNumber = request.TrackNumber != 0 ? request.TrackNumber : null,
            CreatedAt = DateTime.UtcNow
        };
        _context.Songs.Add(song);
        await _context.SaveChangesAsync();
        return MapSong(song);
    }

    public override async Task<SongResponse> UpdateSong(UpdateSongRequest request, ServerCallContext context)
    {
        var song = await _context.Songs.FindAsync(request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Song {request.Id} not found"));
        song.Title = request.Title;
        song.ArtistId = request.ArtistId;
        song.AlbumId = request.AlbumId != 0 ? request.AlbumId : null;
        song.Duration = TimeSpan.FromSeconds(request.DurationSeconds);
        song.Genre = string.IsNullOrEmpty(request.Genre) ? null : request.Genre;
        song.TrackNumber = request.TrackNumber != 0 ? request.TrackNumber : null;
        await _context.SaveChangesAsync();
        return MapSong(song);
    }

    public override async Task<DeleteResponse> DeleteSong(DeleteSongRequest request, ServerCallContext context)
    {
        var song = await _context.Songs.FindAsync(request.Id);
        if (song == null) return new DeleteResponse { Success = false };
        _context.Songs.Remove(song);
        await _context.SaveChangesAsync();
        return new DeleteResponse { Success = true };
    }

    // Artists
    public override async Task<ListArtistsResponse> ListArtists(ListArtistsRequest request, ServerCallContext context)
    {
        var artists = await _context.Artists.ToListAsync();
        var response = new ListArtistsResponse();
        response.Artists.AddRange(artists.Select(MapArtist));
        return response;
    }

    public override async Task<ArtistResponse> GetArtist(GetArtistRequest request, ServerCallContext context)
    {
        var artist = await _context.Artists.FindAsync(request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Artist {request.Id} not found"));
        return MapArtist(artist);
    }

    public override async Task<ArtistResponse> CreateArtist(CreateArtistRequest request, ServerCallContext context)
    {
        var artist = new Artist
        {
            Name = request.Name,
            Bio = string.IsNullOrEmpty(request.Bio) ? null : request.Bio,
            CreatedAt = DateTime.UtcNow
        };
        _context.Artists.Add(artist);
        await _context.SaveChangesAsync();
        return MapArtist(artist);
    }

    public override async Task<ArtistResponse> UpdateArtist(UpdateArtistRequest request, ServerCallContext context)
    {
        var artist = await _context.Artists.FindAsync(request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Artist {request.Id} not found"));
        artist.Name = request.Name;
        artist.Bio = string.IsNullOrEmpty(request.Bio) ? null : request.Bio;
        await _context.SaveChangesAsync();
        return MapArtist(artist);
    }

    public override async Task<DeleteResponse> DeleteArtist(DeleteArtistRequest request, ServerCallContext context)
    {
        var artist = await _context.Artists.FindAsync(request.Id);
        if (artist == null) return new DeleteResponse { Success = false };
        _context.Artists.Remove(artist);
        await _context.SaveChangesAsync();
        return new DeleteResponse { Success = true };
    }

    // Albums
    public override async Task<ListAlbumsResponse> ListAlbums(ListAlbumsRequest request, ServerCallContext context)
    {
        var albums = await _context.Albums.ToListAsync();
        var response = new ListAlbumsResponse();
        response.Albums.AddRange(albums.Select(MapAlbum));
        return response;
    }

    public override async Task<AlbumResponse> GetAlbum(GetAlbumRequest request, ServerCallContext context)
    {
        var album = await _context.Albums.FindAsync(request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Album {request.Id} not found"));
        return MapAlbum(album);
    }

    public override async Task<AlbumResponse> CreateAlbum(CreateAlbumRequest request, ServerCallContext context)
    {
        var album = new Album
        {
            Title = request.Title,
            ArtistId = request.ArtistId,
            ReleaseDate = string.IsNullOrEmpty(request.ReleaseDate) ? null : DateTime.Parse(request.ReleaseDate),
            CreatedAt = DateTime.UtcNow
        };
        _context.Albums.Add(album);
        await _context.SaveChangesAsync();
        return MapAlbum(album);
    }

    public override async Task<AlbumResponse> UpdateAlbum(UpdateAlbumRequest request, ServerCallContext context)
    {
        var album = await _context.Albums.FindAsync(request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Album {request.Id} not found"));
        album.Title = request.Title;
        album.ArtistId = request.ArtistId;
        album.ReleaseDate = string.IsNullOrEmpty(request.ReleaseDate) ? null : DateTime.Parse(request.ReleaseDate);
        await _context.SaveChangesAsync();
        return MapAlbum(album);
    }

    public override async Task<DeleteResponse> DeleteAlbum(DeleteAlbumRequest request, ServerCallContext context)
    {
        var album = await _context.Albums.FindAsync(request.Id);
        if (album == null) return new DeleteResponse { Success = false };
        _context.Albums.Remove(album);
        await _context.SaveChangesAsync();
        return new DeleteResponse { Success = true };
    }

    // Mappers
    private static SongResponse MapSong(Song s) => new()
    {
        Id = s.Id,
        Title = s.Title,
        ArtistId = s.ArtistId,
        AlbumId = s.AlbumId ?? 0,
        DurationSeconds = (long)s.Duration.TotalSeconds,
        Genre = s.Genre ?? string.Empty,
        TrackNumber = s.TrackNumber ?? 0,
        CreatedAt = s.CreatedAt.ToString("O")
    };

    private static ArtistResponse MapArtist(Artist a) => new()
    {
        Id = a.Id,
        Name = a.Name,
        Bio = a.Bio ?? string.Empty,
        CreatedAt = a.CreatedAt.ToString("O")
    };

    private static AlbumResponse MapAlbum(Album a) => new()
    {
        Id = a.Id,
        Title = a.Title,
        ArtistId = a.ArtistId,
        ReleaseDate = a.ReleaseDate?.ToString("O") ?? string.Empty,
        CreatedAt = a.CreatedAt.ToString("O")
    };
}
