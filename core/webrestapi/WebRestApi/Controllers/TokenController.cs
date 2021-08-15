using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using WebRestApi.Models;
using WebRestApi.Service;
using WebRestApi.Service.Models;

namespace WebRestApi.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly IDataService _dataService;
        private readonly ILogger<TokenController> _logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="dataService"></param>
        public TokenController(ILogger<TokenController> logger, IDataService dataService)
        {
            _logger = logger;
            _dataService = dataService;
        }

        /// <summary>
        /// Just for testing purposes
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Ping()
        {
            _logger.LogInformation(LoggingEvents.AuthenticationPingRequest, "Ping request");
            return Ok("pong");
        }

        /// <summary>
        /// Receive JWT token by login and password
        /// </summary>
        /// <param name="credentials">User login and password</param>
        /// <returns>JWT token</returns>
        /// <response code="200">If operation has been completed without any exception</response>
        /// <response code="401">If user input invalid login and/or password</response>
        /// <response code="500">If something wrong had happen during token generation</response>
        [HttpPost]
        public async Task<IActionResult> Token([FromBody] Credentials credentials)
        {
            var isValid = ModelState.IsValid;
            JsonResult result = null;

            try
            {
                _logger.LogInformation(LoggingEvents.AuthenticationGetToken, $"Try get token for {credentials.Login}");

                var identity = await GetIdentity(credentials.Login, credentials.Password);

                if (identity == null)
                {
                    _logger.LogInformation(LoggingEvents.AuthenticationGetToken, $"Was not found identity for {credentials.Login}");
                    return StatusCode(StatusCodes.Status401Unauthorized, new ErrorResponse { Message = "Invalid user name or password" });
                }

                var now = DateTime.UtcNow;

                var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    notBefore: now,
                    claims: identity.Claims,
                    expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
                );

                var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

                var response = new
                {
                    access_token = encodedJwt,
                    username = identity.Name
                };

                result = new JsonResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogException(LoggingEvents.ErrorOnGetToken, ex);
                
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Message = "Something wen wrong" });
            }

            return result;
        }

        private async Task<ClaimsIdentity> GetIdentity(string login, string password)
        {
            var user = await _dataService.GetUserByLoginPasswordAsync(login, password);

            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, user.Email),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role.Name),
                    new Claim(ClaimTypes.Sid, user.Id.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
                return claimsIdentity;
            }

            return null;
        }
    }
}