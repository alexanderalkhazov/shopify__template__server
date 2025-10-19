using AutoMapper;

namespace API.Mapping;

/// <summary>
/// Generic mapping extensions and helpers for AutoMapper
/// </summary>
public static class MappingExtensions
{
    /// <summary>
    /// Maps a collection of objects to another collection type
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <param name="mapper">AutoMapper instance</param>
    /// <param name="source">Source collection</param>
    /// <returns>Mapped collection</returns>
    public static List<TDestination> MapList<TSource, TDestination>(this IMapper mapper, IEnumerable<TSource> source)
    {
        return mapper.Map<List<TDestination>>(source);
    }

    /// <summary>
    /// Maps an object and returns null if source is null
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <param name="mapper">AutoMapper instance</param>
    /// <param name="source">Source object</param>
    /// <returns>Mapped object or null</returns>
    public static TDestination? MapOrNull<TSource, TDestination>(this IMapper mapper, TSource? source)
        where TSource : class
        where TDestination : class
    {
        return source == null ? null : mapper.Map<TDestination>(source);
    }
}

/// <summary>
/// Configuration class for all AutoMapper profiles
/// </summary>
public static class MappingConfig
{
    /// <summary>
    /// Creates AutoMapper configuration with all profiles
    /// </summary>
    /// <returns>MapperConfiguration</returns>
    public static MapperConfiguration CreateConfiguration()
    {
        return new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<Profiles.EntityProfile>();
            // Add more profiles here as you create them
        });
    }
}