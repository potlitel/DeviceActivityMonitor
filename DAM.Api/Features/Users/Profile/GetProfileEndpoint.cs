using DAM.Api.Base;
using DAM.Core.Abstractions;
using DAM.Core.DTOs.Users;
using DAM.Core.Features.Users.Queries;
using DAM.Infrastructure.CQRS;
using FastEndpoints;
using FastEndpoints.Security;
using System.Security.Claims;

namespace DAM.Api.Features.Users.Profile;

public class GetProfileEndpoint(IDispatcher dispatcher)
    : BaseEndpoint<EmptyRequest, ProfileResponse>
{
    public override void Configure()
    {
        Get("/identity/profile"); // GET /api/profile
        Claims("UserId");
        Description(x => x.WithTags("Users"));
        Summary(s => s.Summary = "Obtiene el perfil del usuario actual.");
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
            // Agregamos un error manual a la colección de fallos
            AddError("El identificador de usuario no está presente en el token.");
            await SendErrorsAsync(401, ct);
            return;
        }

        var profile = await dispatcher.QueryAsync(new GetProfileQuery(Guid.Parse(userId)), ct);

        if (profile == null)
        {
            AddError("El perfil de usuario no pudo ser localizado.");
            await SendErrorsAsync(404, ct);
            return;
        }

        await SendSuccessAsync(profile, "Perfil recuperado correctamente", ct);
    }
}