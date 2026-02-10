using DAM.Core.Abstractions;
using DAM.Core.DTOs.Users;

namespace DAM.Core.Features.Users.Queries;

public record GetProfileQuery(Guid UserId) : IQuery<ProfileResponse>;