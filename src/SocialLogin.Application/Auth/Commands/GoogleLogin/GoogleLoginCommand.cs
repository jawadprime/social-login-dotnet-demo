using MediatR;
using SocialLogin.Application.Auth.DTOs;
using SocialLogin.Domain.Common;

namespace SocialLogin.Application.Auth.Commands.GoogleLogin;

public sealed record GoogleLoginCommand(string IdToken, string? IpAddress) : IRequest<Result<AuthResponse>>;
