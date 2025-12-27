//using Application.Common.Dtos;
//using Application.Features.Auth.Dtos;
//using Application.Interfaces.Auth;
//using Application.Interfaces.Repositories;
//using Application.Interfaces.UnitOfWork;
//using Domain.Entities;
//using Domain.Enums;
//using Domain.ValueObject;
//using Google.Apis.Auth;
//using MediatR;
//using Microsoft.Extensions.Logging;

//namespace Application.Features.Auth.Commands.ContinueWithGoogle
//{

//    public class ContinueWithGoogleCommandHandler : IRequestHandler<ContinueWithGoogleCommand, Result<LoginResponseModel>>
//    {
//        private readonly IAuthService _authService;
//        private readonly IUserRepository _userRepository;
//        private readonly IUnitOfWork _unitOfWork;
//        private readonly ILogger<ContinueWithGoogleCommandHandler> _logger;

//        public ContinueWithGoogleCommandHandler(IAuthService authService, ILogger<ContinueWithGoogleCommandHandler> logger, IUserRepository userRepository, IUnitOfWork unitOfWork)
//        {
//            _authService = authService;
//            _userRepository = userRepository;
//            _logger = logger;
//            _unitOfWork = unitOfWork;
//        }
//        public async Task<Result<LoginResponseModel>> Handle(ContinueWithGoogleCommand request, CancellationToken cancellationToken)
//        {
//            GoogleJsonWebSignature.Payload? payload;
//            try
//            {
//                payload = await GoogleJsonWebSignature.ValidateAsync(request.Model.AccessToken);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Failed to validate Google access token.");
//                return Result<LoginResponseModel>.Failure("Invalid Google token.");
//            }

//            if (payload == null || string.IsNullOrWhiteSpace(payload.Email))
//            {
//                _logger.LogWarning("Google token payload is null or email is empty.");

//                return Result<LoginResponseModel>.Failure("Invalid Google token.");
//            }

//            var user = await _userRepository.GetAsync(x => x.Email == new Email(payload.Email));
//            if (user == null)
//            {
//                user = User.RegisterWithGoogle(payload.Name, new Email(payload.Email), payload.Picture, Gender.Other);
//                await _userRepository.AddAsync(user);
//                await _unitOfWork.SaveChangesAsync(cancellationToken);
//            }

//            if (!user.IsEmailVerified)
//            {
//                user.VerifyEmail();
//                await _userRepository.UpdateAsync(user);
//                await _unitOfWork.SaveChangesAsync(cancellationToken);
//            }

//            string token = _authService.GenerateToken(user.Id, user.Email.Value, user.Role.ToString());
//            return Result<LoginResponseModel>.Success(new LoginResponseModel(token, user.IsEmailVerified), "Login successful.");
//        }
//    }
//}


//using Application.Common.Dtos;
//using Application.Features.Auth.Dtos;
//using Application.Interfaces.Auth;
//using Application.Interfaces.Repositories;
//using Application.Interfaces.UnitOfWork;
//using Domain.Entities;
//using Domain.Enums;
//using Domain.ValueObject;
//using Google.Apis.Auth;
//using Google.Apis.Auth.OAuth2;
//using Google.Apis.Auth.OAuth2.Flows;
//using Google.Apis.Auth.OAuth2.Responses;
//using MediatR;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using System.Net.Http;
//using System.Net.Http.Json;

//namespace Application.Features.Auth.Commands.ContinueWithGoogle
//{
//    public class ContinueWithGoogleCommandHandler : IRequestHandler<ContinueWithGoogleCommand, Result<LoginResponseModel>>
//    {
//        private readonly IAuthService _authService;
//        private readonly IUserRepository _userRepository;
//        private readonly IUnitOfWork _unitOfWork;
//        private readonly ILogger<ContinueWithGoogleCommandHandler> _logger;
//        private readonly IConfiguration _configuration;
//        private readonly IHttpClientFactory _httpClientFactory;

//        public ContinueWithGoogleCommandHandler(
//            IAuthService authService,
//            ILogger<ContinueWithGoogleCommandHandler> logger,
//            IUserRepository userRepository,
//            IUnitOfWork unitOfWork,
//            IConfiguration configuration,
//            IHttpClientFactory httpClientFactory)
//        {
//            _authService = authService;
//            _userRepository = userRepository;
//            _logger = logger;
//            _unitOfWork = unitOfWork;
//            _configuration = configuration;
//            _httpClientFactory = httpClientFactory;
//        }

//        public async Task<Result<LoginResponseModel>> Handle(ContinueWithGoogleCommand request, CancellationToken cancellationToken)
//        {
//            GoogleJsonWebSignature.Payload? payload;

//            try
//            {
//                // Exchange authorization code for tokens
//                var tokenResponse = await ExchangeAuthorizationCodeAsync(request.Model.AccessToken, cancellationToken);

//                if (tokenResponse == null || string.IsNullOrWhiteSpace(tokenResponse.IdToken))
//                {
//                    _logger.LogWarning("Failed to exchange authorization code for tokens.");
//                    return Result<LoginResponseModel>.Failure("Failed to authenticate with Google.");
//                }

//                // Now validate the ID token
//                payload = await GoogleJsonWebSignature.ValidateAsync(tokenResponse.IdToken);
//            }
//            catch (InvalidJwtException ex)
//            {
//                _logger.LogError(ex, "Invalid Google JWT token.");
//                return Result<LoginResponseModel>.Failure("Invalid Google token.");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Failed to authenticate with Google.");
//                return Result<LoginResponseModel>.Failure("Google authentication failed.");
//            }

//            if (payload == null || string.IsNullOrWhiteSpace(payload.Email))
//            {
//                _logger.LogWarning("Google token payload is null or email is empty.");
//                return Result<LoginResponseModel>.Failure("Invalid Google token.");
//            }

//            var user = await _userRepository.GetAsync(x => x.Email == new Email(payload.Email));

//            if (user == null)
//            {
//                user = User.RegisterWithGoogle(
//                    payload.Name,
//                    new Email(payload.Email),
//                    payload.Picture,
//                    Gender.Other);

//                await _userRepository.AddAsync(user);
//                await _unitOfWork.SaveChangesAsync(cancellationToken);
//            }

//            if (!user.IsEmailVerified)
//            {
//                user.VerifyEmail();
//                await _userRepository.UpdateAsync(user);
//                await _unitOfWork.SaveChangesAsync(cancellationToken);
//            }

//            string token = _authService.GenerateToken(user.Id, user.Email.Value, user.Role.ToString());

//            return Result<LoginResponseModel>.Success(
//                new LoginResponseModel(token, user.IsEmailVerified),
//                "Login successful.");
//        }

//        private async Task<TokenResponse?> ExchangeAuthorizationCodeAsync(string authorizationCode, CancellationToken cancellationToken)
//        {
//            var clientId = _configuration["Google:ClientId"];
//            var clientSecret = _configuration["Google:ClientSecret"];
//            var redirectUri = _configuration["Google:RedirectUri"] ?? "postmessage"; // "postmessage" for popup mode

//            var httpClient = _httpClientFactory.CreateClient();

//            var tokenRequest = new Dictionary<string, string>
//            {
//                { "code", authorizationCode },
//                { "client_id", clientId },
//                { "client_secret", clientSecret },
//                { "redirect_uri", redirectUri },
//                { "grant_type", "authorization_code" }
//            };

//            var response = await httpClient.PostAsync(
//                "https://oauth2.googleapis.com/token",
//                new FormUrlEncodedContent(tokenRequest),
//                cancellationToken);

//            if (!response.IsSuccessStatusCode)
//            {
//                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
//                _logger.LogError("Failed to exchange authorization code. Status: {Status}, Error: {Error}",
//                    response.StatusCode, errorContent);
//                return null;
//            }

//            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken);
//            return tokenResponse;
//        }
//    }

//    // Token response model
//    public class TokenResponse
//    {
//        public string access_token { get; set; } = string.Empty;
//        public string id_token { get; set; } = string.Empty;
//        public int expires_in { get; set; }
//        public string token_type { get; set; } = string.Empty;
//        public string? refresh_token { get; set; }
//        public string scope { get; set; } = string.Empty;

//        // Mapped properties for easier access
//        public string IdToken => id_token;
//        public string AccessToken => access_token;
//    }
//}

using Application.Common.Dtos;
using Application.Features.Auth.Dtos;
using Application.Interfaces.Auth;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObject;
using Google.Apis.Auth;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Application.Features.Auth.Commands.ContinueWithGoogle
{
    public class ContinueWithGoogleCommandHandler : IRequestHandler<ContinueWithGoogleCommand, Result<LoginResponseModel>>
    {
        private readonly IAuthService _authService;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ContinueWithGoogleCommandHandler> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public ContinueWithGoogleCommandHandler(
            IAuthService authService,
            ILogger<ContinueWithGoogleCommandHandler> logger,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _authService = authService;
            _userRepository = userRepository;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Result<LoginResponseModel>> Handle(ContinueWithGoogleCommand request, CancellationToken cancellationToken)
        {
            GoogleJsonWebSignature.Payload? payload;

            try
            {
                // Exchange authorization code for tokens
                var tokenResponse = await ExchangeAuthorizationCodeAsync(request.Model.AccessToken, cancellationToken);

                if (tokenResponse == null || string.IsNullOrWhiteSpace(tokenResponse.IdToken))
                {
                    _logger.LogWarning("Failed to exchange authorization code for tokens.");
                    return Result<LoginResponseModel>.Failure("Failed to authenticate with Google.");
                }

                // Validate the ID token
                payload = await GoogleJsonWebSignature.ValidateAsync(tokenResponse.IdToken);
            }
            catch (InvalidJwtException ex)
            {
                _logger.LogError(ex, "Invalid Google JWT token.");
                return Result<LoginResponseModel>.Failure("Invalid Google token.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to authenticate with Google.");
                return Result<LoginResponseModel>.Failure("Google authentication failed.");
            }

            if (payload == null || string.IsNullOrWhiteSpace(payload.Email))
            {
                _logger.LogWarning("Google token payload is null or email is empty.");
                return Result<LoginResponseModel>.Failure("Invalid Google token.");
            }

            var user = await _userRepository.GetAsync(x => x.Email == new Email(payload.Email));

            if (user == null)
            {
                user = User.RegisterWithGoogle(
                    payload.Name,
                    new Email(payload.Email),
                    payload.Picture,
                    Gender.Other);

                await _userRepository.AddAsync(user);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            if (!user.IsEmailVerified)
            {
                user.VerifyEmail();
                await _userRepository.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            string token = _authService.GenerateToken(user.Id, user.Email.Value, user.Role.ToString());

            return Result<LoginResponseModel>.Success(
                new LoginResponseModel(token, user.IsEmailVerified),
                "Login successful.");
        }

        private async Task<TokenResponse?> ExchangeAuthorizationCodeAsync(string authorizationCode, CancellationToken cancellationToken)
        {
            var clientId = _configuration["GoogleAuth:ClientId"];
            var clientSecret = _configuration["GoogleAuth:ClientSecret"];
            var redirectUri = _configuration["GoogleAuth:RedirectUri"] ?? "postmessage";

            // Validate configuration
            if (string.IsNullOrWhiteSpace(clientId))
            {
                _logger.LogError("Google ClientId is not configured.");
                throw new InvalidOperationException("Google ClientId is missing in configuration.");
            }

            if (string.IsNullOrWhiteSpace(clientSecret))
            {
                _logger.LogError("Google ClientSecret is not configured.");
                throw new InvalidOperationException("Google ClientSecret is missing in configuration.");
            }

            var tokenRequest = new Dictionary<string, string>
            {
                { "code", authorizationCode },
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "redirect_uri", redirectUri },
                { "grant_type", "authorization_code" }
            };

            var httpClient = _httpClientFactory.CreateClient();

            var response = await httpClient.PostAsync(
                "https://oauth2.googleapis.com/token",
                new FormUrlEncodedContent(tokenRequest),
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to exchange authorization code. Status: {Status}, Error: {Error}",
                    response.StatusCode, errorContent);
                return null;
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken);
            return tokenResponse;
        }
    }

    public class TokenResponse
    {
        public string access_token { get; set; } = string.Empty;
        public string id_token { get; set; } = string.Empty;
        public int expires_in { get; set; }
        public string token_type { get; set; } = string.Empty;
        public string? refresh_token { get; set; }
        public string scope { get; set; } = string.Empty;

        public string IdToken => id_token;
        public string AccessToken => access_token;
    }
}