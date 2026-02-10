using DAM.Api.Base;
using DAM.Core.DTOs.AuditLogs;
using DAM.Core.Features.Audit.Queries;
using DAM.Infrastructure.CQRS;

namespace DAM.Api.Features.Audit
{
    public class GetAuditLogByIdEndpoint(IDispatcher d) : BaseEndpoint<GetByIdRequest, AuditLogDto>
    {
        public override void Configure() { Get("/audit/{id}"); Roles("Manager"); }
        public override async Task HandleAsync(GetByIdRequest r, CancellationToken ct)
        {
            var res = await d.QueryAsync(new GetAuditLogByIdQuery(r.Id), ct);
            if (res == null) await SendErrorsAsync(404, ct); else await SendSuccessAsync(res, ct: ct);
        }
    }
}
