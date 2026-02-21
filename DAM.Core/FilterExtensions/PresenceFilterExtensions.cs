using DAM.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAM.Core.FilterExtensions
{
    /// <summary>
    /// Proporciona métodos de extensión para aplicar filtros de negocio a consultas de presencia de dispositivos.
    /// </summary>
    public static class PresenceFilterExtensions
    {
        /// <summary>
        /// Aplica filtros condicionales de búsqueda por ActivityId.
        /// </summary>
        /// <param name="query">Consulta base IQueryable de DevicePresence.</param>
        /// <param name="filter">Objeto que contiene los criterios de filtrado.</param>
        /// <returns>La consulta con los filtros aplicados si los valores son válidos.</returns>
        /// <remarks>
        /// Implementa una evaluación perezosa (Lazy Evaluation) donde el filtro 
        /// solo se agrega al árbol de expresión si el ActivityId tiene un valor válido.
        /// </remarks>
        public static IQueryable<DevicePresence> ApplyPresenceFilters(
            this IQueryable<DevicePresence> query,
            PresenceFilter filter)
        {
            return query
                .FilterByActivityId(filter.ActivityId);
        }

        /// <summary>
        /// Filtra los eventos de presencia por el identificador de actividad.
        /// </summary>
        /// <param name="query">Consulta base.</param>
        /// <param name="activityId">Identificador de actividad a filtrar (opcional).</param>
        /// <returns>Consulta filtrada por ActivityId si se proporcionó, o la consulta original.</returns>
        /// <remarks>
        /// Aplica filtro por igualdad exacta del ActivityId.
        /// </remarks>
        private static IQueryable<DevicePresence> FilterByActivityId(
            this IQueryable<DevicePresence> query,
            int? activityId)
        {
            return !activityId.HasValue
                ? query
                : query.Where(x => x.DeviceActivityId == activityId.Value);
        }
    }
}
