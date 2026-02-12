using DAM.Api.Base;
using DAM.Core.Abstractions;
using DAM.Core.DTOs.Users;
using DAM.Core.Features.Users.Queries;
using DAM.Infrastructure.CQRS;
using FastEndpoints;
using FastEndpoints.Security;
using System.Security.Claims;

namespace DAM.Api.Features.Users.Profile;

/// <summary>
/// 👤 Obtiene el perfil completo del usuario autenticado.
/// </summary>
/// <remarks>
/// <para>
/// <b>🔍 Detalles del endpoint:</b>
/// <list type="bullet">
/// <item><description><b>Método:</b> GET</description></item>
/// <item><description><b>Ruta:</b> /identity/profile</description></item>
/// <item><description><b>Autenticación:</b> Requerida (JWT Bearer)</description></item>
/// <item><description><b>Claims requeridos:</b> UserId</description></item>
/// </list>
/// </para>
/// <para>
/// <b>📋 Información retornada:</b>
/// <list type="bullet">
/// <item><description>📧 Email y nombre de usuario</description></item>
/// <item><description>🔑 Rol y permisos asignados</description></item>
/// <item><description>🔐 Estado de 2FA</description></item>
/// <item><description>⚙️ Preferencias de usuario (tema, idioma)</description></item>
/// <item><description>📜 Claims adicionales del token</description></item>
/// </list>
/// </para>
/// </remarks>
public class GetProfileEndpoint(IDispatcher dispatcher)
    : BaseEndpoint<EmptyRequest, ProfileResponse>
{
    public override void Configure()
    {
        Get("/identity/profile");
        Claims("UserId");

        Description(x => x
            .Produces<ProfileResponse>(200)
            .ProducesProblem(401)
            .ProducesProblem(404)
            .WithTags("👤 Perfil"));

        Summary(s => s.Summary = "👤 [Perfil] Obtiene perfil del usuario actual");
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        // Extraemos el ID del usuario del token JWT
        //var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //if (!Guid.TryParse(userIdStr, out var userId))
        //{
        //    await SendErrorsAsync(401, ct);
        //    return;
        //}

        //var result = await dispatcher.QueryAsync(new GetProfileQuery(userId), ct);
        //await SendSuccessAsync(result, "Perfil recuperado");

        var userId = User.ClaimValue("UserId");

        if (string.IsNullOrEmpty(userId))
        {
            AddError("🔒 El identificador de usuario no está presente en el token.");
            await SendErrorsAsync(401, ct);
            return;
        }

        var profile = await dispatcher.QueryAsync(new GetProfileQuery(Guid.Parse(userId)), ct);

        if (profile == null)
        {
            AddError("👤 El perfil de usuario no pudo ser localizado.");
            await SendErrorsAsync(404, ct);
            return;
        }

        await SendSuccessAsync(profile, "✅ Perfil recuperado correctamente", ct);
    }
}