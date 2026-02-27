using DAM.Api.Base;
using DAM.Core.Common;
using DAM.Core.DTOs.Common;
using DAM.Core.DTOs.Invoices;
using DAM.Core.Features.Invoices.Queries;
using DAM.Infrastructure.CQRS;

namespace DAM.Api.Features.Invoices
{
    /// <summary>
    /// 💰 Obtiene el listado paginado de facturas generadas.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>🔍 Detalles del endpoint:</b>
    /// <list type="bullet">
    /// <item><description><b>Método:</b> GET</description></item>
    /// <item><description><b>Ruta:</b> /invoices</description></item>
    /// <item><description><b>Autenticación:</b> Requerida (JWT Bearer)</description></item>
    /// <item><description><b>Roles permitidos:</b> Manager</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>📊 Paginación y Filtros:</b>
    /// Permite filtrar por rango de fechas, estado de la factura y cliente. 
    /// Retorna un objeto <c>PaginatedList</c> con metadata de navegación.
    /// </para>
    /// </remarks>
    /// <response code="200">✅ Listado de facturas recuperado con éxito</response>
    /// <response code="400">❌ Parámetros de filtro inválidos</response>
    /// <response code="401">❌ No autenticado</response>
    /// <response code="403">❌ Acceso denegado</response>
    public class GetInvoicesEndpoint(IDispatcher d) : BaseEndpoint<InvoiceFilter, PaginatedList<InvoiceDto>>
    {
        public override void Configure() {

            Get("/invoices");
            Roles("Manager");

            Description(x => x
                .Produces<PaginatedList<InvoiceDto>>(200)
                .ProducesProblem(400)
                .ProducesProblem(401)
                .ProducesProblem(403)
                .WithTags("💰 Facturación"));

            Summary(s =>
            {
                s.Summary = "💰 [Facturación] Listado paginado";
                s.Description = "Consulta el historial global de facturación con soporte para filtros avanzados.";
            });

        }
        public override async Task HandleAsync(InvoiceFilter r, CancellationToken ct)
        => await SendSuccessAsync(await d.QueryAsync(new GetInvoicesQuery(r), ct),
            "✅ Facturas recuperadas correctamente", ct);
    }
}
