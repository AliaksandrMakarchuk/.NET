using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebRestApi.Service;
using WebRestApi.Service.Models;
using WebRestApi.Service.Models.Client;

namespace WebRestApi.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/[controller]")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize]
    [ApiController]
    public class UserController : ControllerBase
    {
        private ILogger _logger;
        private IDataService _dataService;

        ///
        public UserController(ILogger<UserController> logger, IDataService dataService)
        {
            _logger = logger;
            _dataService = dataService;
        }

        /// <summary>
        /// Get list of users
        /// </summary>
        /// <remarks></remarks>
        /// <returns>Collection of existing users</returns>
        /// <response code="200">If operation has been completed without any exception</response>
        /// <response code="401">If User is not authorized</response>
        /// <response code="403">If the access is forbidden</response>
        /// <response code="500">If something wrong had happen during getting users</response>
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            _logger.LogInformation(LoggingEvents.GetAllUsers, "Try get all existing users");

            try
            {
                var users = await _dataService.GetAllUsers();

                return Ok(users.Select(u => u.ToClientUser()));
            }
            catch (Exception ex)
            {
                _logger.LogException(LoggingEvents.ErrorOnGetAllUsers, ex);

                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Message = "Something went wrong" });
            }
        }

        /// <summary>
        /// Get User by Id
        /// </summary>
        /// <remarks></remarks>
        /// <param name="id"></param>
        /// <returns>User by Id</returns>
        /// <response code="200">If operation has been completed without any exception</response>
        /// <response code="500">If something wrong had happen during gettting the user</response>
        [Authorize(Roles = "user, admin")]
        [HttpGet("{id}", Name = "GetUserById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            _logger.LogInformation(LoggingEvents.GetUserById, $"Get User by Id {id}");

            try
            {
                if (this.User.FindFirst(x => x.Type == ClaimsIdentity.DefaultRoleClaimType).Value == UserRole.ADMIN.RoleName ||
                    this.User.FindFirst(x => x.Type == ClaimTypes.Sid).Value == id.ToString())
                {
                    var user = await _dataService.GetUserByIdAsync(id);
                    return Ok(user);
                }

                _logger.LogInformation(LoggingEvents.GetUserById, "Request forbidden for current user");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogException(LoggingEvents.ErrorOnGetUserById, ex);

                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Message = "Something went wrong" });
            }
        }

        /// <summary>
        /// Create new user
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="user">User</param>
        /// <returns>New created User</returns>
        /// <response code="201">If new User has been successfully created</response>
        /// <response code="400">If the First or Last name was not specified</response>
        /// <response code="500">If something wrong had happen during the User creation</response>
        [Authorize(Roles = "admin")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post([FromBody] IdentityUser user)
        {
            _logger.LogInformation(LoggingEvents.CreateUser, "Try create user");

            if (string.IsNullOrWhiteSpace(user.FirstName) || string.IsNullOrWhiteSpace(user.LastName))
            {
                _logger.LogError(LoggingEvents.ErrorOnCreateUser, $"Can't create new user: {nameof(user.FirstName)} or {nameof(user.LastName)} is not specified or contains white-space characters");
                return BadRequest(new ErrorResponse { Message = "User first name and last name should be filled in" });
            }

            try
            {
                var newUser = await _dataService.CreateNewUserAsync(user);
                return Created("here", newUser);
            }
            catch (Exception ex)
            {
                _logger.LogException(LoggingEvents.ErrorOnCreateUser, "Error on creating new User", ex);

                var error = new ErrorResponse { Message = "Error has accured on saving changes" };
                return StatusCode(StatusCodes.Status500InternalServerError, error);
            }
        }

        /// <summary>
        /// Update user
        /// </summary>
        /// <remarks></remarks>
        /// <param name="user">User with new properties</param>
        /// <returns>User updated parameters</returns>
        /// <response code="200">If the User successfully updated</response>
        /// <response code="400">If could not find User by User.Id</response>
        /// <response code="500">If something wrong had happend during user update</response>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "user, admin")]
        public async Task<IActionResult> Put([FromBody] ClientUser user)
        {
            _logger.LogInformation(LoggingEvents.UpdateUserName, $"Update user name for User with Id {user.Id}");

            if (this.User.FindFirst(x => x.Type == ClaimsIdentity.DefaultRoleClaimType).Value == UserRole.USER.RoleName &&
                this.User.FindFirst(x => x.Type == ClaimTypes.Sid).Value != user.Id.ToString())
            {
                _logger.LogInformation(LoggingEvents.GetUserById, "Request forbidden for current user");
                return Forbid();
            }

            try
            {
                var updatedUser = await _dataService.UpdateUserAsync(user);

                if (updatedUser == null)
                {
                    _logger.LogInformation(LoggingEvents.ErrorOnUpdateUserName, $"Could not found User with Id {user.Id}");
                    return BadRequest(new ErrorResponse { Message = "User with specified identifier could not be found" });
                }

                return Ok(updatedUser);
            }
            catch (Exception ex)
            {
                _logger.LogException(LoggingEvents.ErrorOnUpdateUserName, ex);
                
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Message = "Error has accured on saving changes" });
            }
        }

        /// <summary>
        /// Delete user
        /// </summary>
        /// <param name="id">User Id</param>
        /// <remarks></remarks>
        /// <response code="200">If User has heen successfully deleted</response>
        /// <response code="400">If could not find User by id</response>
        /// <response code="401">If User does not have rights for deleting user</response>
        /// <response code="500">If something went wrong during removing the User</response>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete([FromBody] int id)
        {
            _logger.LogInformation(LoggingEvents.DeleteUserById, $"Try delete User with Id: {id}");

            var userRole = this.User.FindFirst(x => x.Type == ClaimsIdentity.DefaultRoleClaimType).Value;
            var userId = this.User.FindFirst(x => x.Type == ClaimTypes.Sid).Value;

            if (userRole == UserRole.ADMIN.RoleName && userId == id.ToString() ||
                userRole == UserRole.USER.RoleName && userId != id.ToString())
            {
                _logger.LogInformation(LoggingEvents.DeleteUserById, "Request forbidden for current user");
                return Forbid();
            }

            try
            {
                var user = await _dataService.DeleteUserAsync(id);
                if (user == null)
                {
                    _logger.LogInformation(LoggingEvents.ErrorOnDeleteUserById, $"Could not found User with Id {id}");
                    return BadRequest(new ErrorResponse { Message = $"Could not find a user by id: {id}" });
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogException(LoggingEvents.ErrorOnDeleteUserById, ex);

                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Message = "Error on removing specified User" });
            }
        }
    }
}