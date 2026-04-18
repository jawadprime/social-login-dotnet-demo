using MediatR;
using SocialLogin.Application.Auth.DTOs;
using SocialLogin.Domain.Common;

namespace SocialLogin.Application.Auth.Commands.RefreshToken;

public sealed record RefreshTokenCommand(string AccessToken, string RefreshToken, string? IpAddress)
    : IRequest<Result<AuthResponse>>;
