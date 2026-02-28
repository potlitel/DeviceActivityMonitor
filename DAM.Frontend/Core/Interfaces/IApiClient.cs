using DAM.Frontend.Core.Models.Activities;
using DAM.Frontend.Core.Models.Audit;
using DAM.Frontend.Core.Models.Auth;
using DAM.Frontend.Core.Models.Common;
using DAM.Frontend.Core.Models.Invoices;
using DAM.Frontend.Core.Models.Presence;
using DAM.Frontend.Core.Models.System;

namespace DAM.Frontend.Core.Interfaces
{
    /// <summary>
    /// 🌐 Contrato unificado para comunicación con DAM.API
    /// </summary>
    public interface IApiClient
    {
        // 🔐 Autenticación
        Task<AuthResponse?> LoginAsync(LoginRequest request);
        Task LogoutAsync();
        Task<bool> RefreshTokenAsync();

        // 📱 Actividades
        Task<PaginatedList<ActivityDto>> GetActivitiesAsync(ActivityFilter filter);
        Task<ActivityDto?> GetActivityByIdAsync(int id);

        // 👤 Presencia
        Task<PaginatedList<PresenceDto>> GetPresenceAsync(PresenceFilter filter);
        Task<PresenceDto?> GetPresenceByIdAsync(int id);

        // 💰 Facturas
        Task<PaginatedList<InvoiceDto>> GetInvoicesAsync(InvoiceFilter filter);
        Task<InvoiceDto?> GetInvoiceByIdAsync(int id);

        // 📋 Auditoría
        Task<PaginatedList<AuditLogDto>> GetAuditLogsAsync(AuditFilter filter);
        Task<AuditLogDto?> GetAuditLogByIdAsync(Guid id);

        // 📊 Sistema
        Task<PaginatedList<ServiceEventDto>> GetServiceEventsAsync(ServiceEventFilter filter);
        Task<ServiceEventDto?> GetServiceEventByIdAsync(int id);

        // 👤 Perfil
        Task<ProfileResponse?> GetProfileAsync();
        Task<Setup2FAResponse?> Setup2FAAsync();
    }
}
