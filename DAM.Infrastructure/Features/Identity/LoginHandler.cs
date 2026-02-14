using DAM.Core.Abstractions;
using DAM.Core.DTOs.Login;
using DAM.Core.Repositories;
using DAM.Infrastructure.Identity;
using Microsoft.Extensions.Logging;

namespace DAM.Infrastructure.Features.Identity
{
    public class LoginHandler(
    IUserRepository users,
    IIdentityService identity,
    IPasswordHasher hasher, // <--- Nueva abstracción inyectada
    ILogger<LoginHandler> logger)
    : ICommandHandler<LoginRequest, LoginResponse?>
    {
        public async Task<LoginResponse?> HandleAsync(LoginRequest cmd, CancellationToken ct)
        {
            var user = await users.GetByEmailAsync(cmd.Email, ct);

            // La verificación es ahora limpia y desacoplada
            if (user == null || !hasher.Verify(cmd.Password, user.PasswordHash))
            {
                logger.LogWarning("Fallo de autenticación: {Email}", cmd.Email);
                return null;
            }

            var token = identity.CreateJwtToken(user);

            return new LoginResponse(
                Token: token,
                UserEmail: user.Email,
                ExpiresAt: DateTime.UtcNow.AddHours(8)
            );
        }
    }
}
