using DAM.Core.Abstractions;
using DAM.Core.DTOs.Users;
using DAM.Core.Features.Users.Queries;
using DAM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DAM.Infrastructure.Features.Users.Handlers;

public class GetProfileHandler(DeviceActivityDbContext db)  : IQueryHandler<GetProfileQuery, ProfileResponse>
{
    //TODO: Se debe inyectar el repositorio de usuarios ya implementado!!!
    public async Task<ProfileResponse> HandleAsync(GetProfileQuery query, CancellationToken ct)
    {
        //var user = await db.Users
        //    .AsNoTracking()
        //    .FirstOrDefaultAsync(u => u.Id == query.UserId, ct);

        //if (user == null) throw new KeyNotFoundException("Usuario no encontrado");

        //// En un caso real, las preferencias podrían venir de un JSON en la BD
        var preferences = new UserPreferences("Dark", "es-ES");

        //return new ProfileResponse(
        //    user.Id,
        //    user.Username,
        //    user.Email,
        //    user.Role.ToString(),
        //    user.IsTwoFactorEnabled,
        //    new List<KeyValuePair<string, string>> {
        //        new("CreatedAt", user.InsertedAt.ToString("O"))
        //    },
        //    preferences
        //);

        return new ProfileResponse(Guid.NewGuid(), "potlitel", "potlitel@gmail.com", "Manager", true, new List<KeyValuePair<string, string>>
        {
                    new("CreatedAt", "")
                },
                preferences);
        }
}