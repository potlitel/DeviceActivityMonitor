using DAM.Core.Common;
using Microsoft.EntityFrameworkCore;

namespace DAM.Infrastructure.Extensions;

/// <summary>
/// Provee métodos de extensión para facilitar la paginación eficiente en IQueryable.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Convierte una consulta IQueryable en una lista paginada aplicando proyección de datos.
    /// </summary>
    /// <typeparam name="TSource">Tipo de la entidad de origen (Base de datos).</typeparam>
    /// <typeparam name="TDestination">Tipo del DTO de destino.</typeparam>
    /// <param name="mapFunc">Función de mapeo (usualmente .Adapt de Mapster).</param>
    public static async Task<PaginatedList<TDestination>> ToPaginatedListAsync<TSource, TDestination>(
        this IQueryable<TSource> query,
        int pageNumber,
        int pageSize,
        Func<TSource, TDestination> mapFunc,
        CancellationToken ct)
    {
        var count = await query.CountAsync(ct);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PaginatedList<TDestination>([.. items.Select(mapFunc)], count, pageNumber, pageSize);
    }
}