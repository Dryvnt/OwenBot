using System.Text.Json.Serialization;

namespace OwenBot;

public record Wow(
    [property: JsonPropertyName("movie")] string Movie,
    [property: JsonPropertyName("year")] int Year,
    [property: JsonPropertyName("release_date")]
    string ReleaseDate,
    [property: JsonPropertyName("director")]
    string Director,
    [property: JsonPropertyName("character")]
    string Character,
    [property: JsonPropertyName("movie_duration")]
    string MovieDuration,
    [property: JsonPropertyName("timestamp")]
    string Timestamp,
    [property: JsonPropertyName("full_line")]
    string FullLine,
    [property: JsonPropertyName("current_wow_in_movie")]
    int CurrentWowInMovie,
    [property: JsonPropertyName("total_wow_in_movie")]
    int TotalWowsInMovie,
    [property: JsonPropertyName("poster")] Uri Poster,
    [property: JsonPropertyName("video")] VideoLinkCollection VideoLinkCollection,
    [property: JsonPropertyName("audio")] Uri Audio
);

public record VideoLinkCollection(
    [property: JsonPropertyName("1080p")] Uri Video1080p,
    [property: JsonPropertyName("720p")] Uri Video720p,
    [property: JsonPropertyName("480p")] Uri Video480p,
    [property: JsonPropertyName("360p")] Uri Video360p
);
