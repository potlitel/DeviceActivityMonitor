using DAM.Core.Abstractions;
using DAM.Core.Common;
using DAM.Core.DTOs.Invoices;
using DAM.Core.Features.Invoices.Queries;
using DAM.Core.FilterExtensions;
using DAM.Core.Interfaces;
using DAM.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace DAM.Infrastructure.Features.Invoices.Handlers
{
    /// <summary>
    /// Handler para consultar facturas con paginación.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Implementa la lógica de negocio para recuperar facturas paginadas,
    /// aplicando filtros por monto mínimo cuando se especifica.
    /// </para>
    /// <para>
    /// <b>Optimizaciones:</b>
    /// <list type="bullet">
    /// <item><description>Usa <c>AsNoTracking()</c> para lecturas eficientes (implícito en el repositorio)</description></item>
    /// <item><description>Proyección selectiva a DTO para minimizar data transfer</description></item>
    /// <item><description>Ordenamiento descendente por timestamp para mostrar facturas más recientes primero</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <param name="repository">Repositorio de facturas inyectado por DI.</param>
    public class GetInvoicesHandler(IInvoiceRepository repository)
        : IQueryHandler<GetInvoicesQuery, PaginatedList<InvoiceDto>>
    {
        /// <inheritdoc/>
        public async Task<PaginatedList<InvoiceDto>> HandleAsync(
            GetInvoicesQuery query,
            CancellationToken cancellationToken)
        {
            //return await repository.GetAllQueryable()
            //    .ApplyInvoiceFilters(query.Filter)
            //    .OrderByDescending(x => x.Timestamp)
            //    .ToPaginatedListAsync(
            //        query.Filter.PageNumber,
            //        query.Filter.PageSize,
            //        entity => new InvoiceDto(
            //            entity.Id,
            //            entity.SerialNumber,
            //            entity.Timestamp,
            //            entity.TotalAmount,
            //            entity.Description
            //        ),
            //        cancellationToken
            //    );

            return await repository.GetAllQueryable(query => query.Include(x => x.DeviceActivity))
                .ApplyInvoiceFilters(query.Filter)
                .OrderByDescending(x => x.Timestamp)
                .ToPaginatedListAsync(
                    query.Filter.PageNumber,
                    query.Filter.PageSize,
                    entity => InvoiceDto.FromEntity(entity),
                    cancellationToken
                );
        }
    }
}
