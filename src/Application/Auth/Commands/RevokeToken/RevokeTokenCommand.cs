using MediatR;
using SocialLogin.Domain.Common;

namespace SocialLogin.Application.Auth.Commands.RevokeToken;

public sealed record RevokeTokenCommand(string RefreshToken, string UserId, string? IpAddress) : IRequest<Result>;
