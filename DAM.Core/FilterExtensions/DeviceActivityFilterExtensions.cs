using DAM.Core.Entities;

using DAM.Core.Enums;

namespace DAM.Core.FilterExtensions
{

    /// <summary>
    /// Provee métodos de extensión para aplicar filtros de negocio a consultas de actividades.
    /// </summary>
    public static class DeviceActivityFilterExtensions
    {
        /// <summary>
        /// Aplica filtros condicionales de búsqueda por Serial Number y Estado.
        /// </summary>
        /// <param name="query">Consulta base IQueryable.</param>
        /// <param name="filter">Objeto que contiene los criterios de filtrado.</param>
        /// <returns>La consulta con los filtros aplicados si los valores son válidos.</returns>
        /// <remarks>
        /// Implementa una evaluación perezosa (Lazy Evaluation) donde los filtros 
        /// solo se agregan al árbol de expresión si el input no es nulo o vacío.
        /// </remarks>
        public static IQueryable<DeviceActivity> ApplyActivityFilters(
            this IQueryable<DeviceActivity> query,
            ActivityFilter filter)
        {
            return query
                .FilterBySerialNumber(filter.SerialNumber)
                .FilterByStatus(filter.Status);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="serialNumber"></param>
        /// <returns></returns>
        private static IQueryable<DeviceActivity> FilterBySerialNumber(this IQueryable<DeviceActivity> query, string? serialNumber)
        {
            return string.IsNullOrWhiteSpace(serialNumber)
                ? query
                : query.Where(x => x.SerialNumber.Contains(serialNumber));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private static IQueryable<DeviceActivity> FilterByStatus(this IQueryable<DeviceActivity> query, ActivityStatus? status)
        {
            return !status.HasValue
                ? query
                : query.Where(x => x.Status == status.Value);
        }
    }
}