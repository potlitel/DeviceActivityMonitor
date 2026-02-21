using DAM.Core.DTOs.Audit;
using DAM.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAM.Core.FilterExtensions
{
    /// <summary>
    /// Proporciona métodos de extensión para aplicar filtros de negocio a consultas de registros de auditoría.
    /// </summary>
    public static class AuditLogFilterExtensions
    {
        /// <summary>
        /// Aplica filtros condicionales de búsqueda por nombre de usuario y rango de fechas.
        /// </summary>
        /// <param name="query">Consulta base IQueryable de AuditLog.</param>
        /// <param name="filter">Objeto que contiene los criterios de filtrado.</param>
        /// <returns>La consulta con los filtros aplicados si los valores son válidos.</returns>
        /// <remarks>
        /// Implementa una evaluación perezosa (Lazy Evaluation) donde los filtros 
        /// solo se agregan al árbol de expresión si el input cumple las condiciones.
        /// </remarks>
        public static IQueryable<AuditLog> ApplyAuditFilters(
            this IQueryable<AuditLog> query,
            AuditLogFilter filter)
        {
            return query
                .FilterByUsername(filter.Username)
                .FilterByDateRange(filter.FromDate);
        }

        /// <summary>
        /// Filtra los registros de auditoría por nombre de usuario.
        /// </summary>
        /// <param name="query">Consulta base.</param>
        /// <param name="username">Nombre de usuario a filtrar (opcional).</param>
        /// <returns>Consulta filtrada por username si se proporcionó, o la consulta original.</returns>
        /// <remarks>
        /// Utiliza búsqueda por coincidencia exacta del nombre de usuario.
        /// </remarks>
        private static IQueryable<AuditLog> FilterByUsername(
            this IQueryable<AuditLog> query,
            string? username)
        {
            return string.IsNullOrWhiteSpace(username)
                ? query
                : query.Where(x => x.Username.Contains(username));
        }

        /// <summary>
        /// Filtra los registros de auditoría por fecha de inicio.
        /// </summary>
        /// <param name="query">Consulta base.</param>
        /// <param name="fromDate">Fecha de inicio para el filtro (opcional).</param>
        /// <returns>Consulta filtrada por fecha si se proporcionó, o la consulta original.</returns>
        /// <remarks>
        /// Incluye registros desde la fecha especificada (inclusive) hasta el momento actual.
        /// </remarks>
        private static IQueryable<AuditLog> FilterByDateRange(
            this IQueryable<AuditLog> query,
            DateTime? fromDate)
        {
            return !fromDate.HasValue
                ? query
                : query.Where(x => x.TimestampUtc >= fromDate.Value);
        }
    }
}
