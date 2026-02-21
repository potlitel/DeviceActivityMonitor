using DAM.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAM.Core.FilterExtensions
{
    /// <summary>
    /// Proporciona métodos de extensión para aplicar filtros de negocio a consultas de facturas.
    /// </summary>
    public static class InvoiceFilterExtensions
    {
        /// <summary>
        /// Aplica filtros condicionales de búsqueda por monto mínimo.
        /// </summary>
        /// <param name="query">Consulta base IQueryable de Invoice.</param>
        /// <param name="filter">Objeto que contiene los criterios de filtrado.</param>
        /// <returns>La consulta con los filtros aplicados si los valores son válidos.</returns>
        /// <remarks>
        /// Implementa una evaluación perezosa (Lazy Evaluation) donde el filtro 
        /// solo se agrega al árbol de expresión si el MinAmount tiene un valor válido.
        /// </remarks>
        public static IQueryable<Invoice> ApplyInvoiceFilters(
            this IQueryable<Invoice> query,
            InvoiceFilter filter)
        {
            return query
                .FilterByMinAmount(filter.MinAmount);
        }

        /// <summary>
        /// Filtra las facturas por monto mínimo.
        /// </summary>
        /// <param name="query">Consulta base.</param>
        /// <param name="minAmount">Monto mínimo a filtrar (opcional).</param>
        /// <returns>Consulta filtrada por MinAmount si se proporcionó, o la consulta original.</returns>
        /// <remarks>
        /// Incluye facturas con TotalAmount mayor o igual al valor especificado.
        /// </remarks>
        private static IQueryable<Invoice> FilterByMinAmount(
            this IQueryable<Invoice> query,
            decimal? minAmount)
        {
            return !minAmount.HasValue
                ? query
                : query.Where(x => x.TotalAmount >= minAmount.Value);
        }
    }
}
