using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IDataService _dataService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="dataService"></param>
        public MessageController(ILogger<MessageController> logger, IDataService dataService)
        {
            _logger = logger;
            _dataService = dataService;
        }

        /// <summary>
        /// Get all messages from db
        /// </summary>
        /// <returns>All existing messages</returns>
        /// <response code="200">If operation has been completed without any exception</response>
        /// <response code="500">If something wrong had happen during getting users</response>
        [Authorize(Roles = "admin, user")]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            _logger.LogInformation(LoggingEvents.GetMessagesByUser, $"Try get all messages for user {this.User.FindFirst(x => x.Type == ClaimTypes.Sid).Value}");
            try
            {
                var messages = await _dataService.GetAllMessagesAsync(this.User);
                return Ok(messages.Select(m => new ClientMessage
                {
                    Id = m.Id,
                    From = new ClientUser
                    {
                        Id = m.Sender.Id,
                        FirstName = m.Sender.FirstName,
                        LastName = m.Sender.LastName,
                        RoleName = m.Sender.Role.Name
                    },
                    To = new ClientUser
                    {
                        Id = m.Receiver.Id,
                        FirstName = m.Receiver.FirstName,
                        LastName = m.Receiver.LastName,
                        RoleName = m.Receiver.Role.Name
                    },
                    Message = m.Text
                }));
            }
            catch (Exception ex)
            {
                _logger.LogException(LoggingEvents.ErrorOnGetAllMessageByUser, ex);

                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Message = "Something went wrong" });
            }
        }

        /// <summary>
        /// Get Message by Id
        /// </summary>
        /// <remarks></remarks>
        /// <param name="id">Message Id</param>
        /// <returns>Message</returns>
        /// <response code="200">If operation has been completed without any exception</response>
        /// <response code="500">If something wrong had happen during gettting the user</response>
        [Authorize(Roles = "user, admin")]
        [HttpGet("{id}", Name = "GetMessageById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            _logger.LogInformation(LoggingEvents.GetMessageById, $"Try get Message by Id: {id}");

            try
            {
                var message = await _dataService.GetMessageById(id);
                var currentUserId = int.Parse(this.User.FindFirst(x => x.Type == ClaimTypes.Sid).Value);
                var isAdmin = this.User.FindFirst(x => x.Type == ClaimsIdentity.DefaultRoleClaimType).Value == UserRole.ADMIN.RoleName;

                if (!isAdmin && currentUserId != message.SenderId)
                {
                    _logger.LogInformation(LoggingEvents.GetMessageById, "Request forbidden for current user");
                    return Forbid();
                }

                return Ok(new ClientMessage
                {
                    Id = message.Id,
                    From = new ClientUser
                    {
                        Id = message.Sender.Id,
                        FirstName = message.Sender.FirstName,
                        LastName = message.Sender.LastName,
                        RoleName = message.Sender.Role.Name
                    },
                    To = new ClientUser
                    {
                        Id = message.Receiver.Id,
                        FirstName = message.Receiver.FirstName,
                        LastName = message.Receiver.LastName,
                        RoleName = message.Receiver.Role.Name
                    },
                    Message = message.Text
                });
            }
            catch (Exception ex)
            {
                _logger.LogException(LoggingEvents.ErrorOnGetMessageById, ex);

                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Message = "Something went wrong" });
            }
        }

        /// <summary>
        /// Send a message
        /// </summary>
        /// <param name="message"></param>
        /// <response code="200">If operation has been completed without any exception</response>
        /// <response code="400">If a sender or receiver could not be found</response>
        /// <response code="500">If something wrong had happen during getting users</response>
        [Authorize(Roles = "user, admin")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post([FromBody] ClientMessage message)
        {
            _logger.LogInformation(LoggingEvents.CreateMessage, "Try send message");

            var currentUserId = int.Parse(this.User.FindFirst(x => x.Type == ClaimTypes.Sid).Value);

            if (message.From?.Id != null &&
                this.User.FindFirst(x => x.Type == ClaimsIdentity.DefaultRoleClaimType).Value == UserRole.ADMIN.RoleName &&
                message.From.Id.Value != currentUserId)
            {
                _logger.LogInformation(LoggingEvents.GetMessageById, "Request forbidden for current user");
                return Forbid();
            }

            var userFrom = await _dataService.GetUserByIdAsync(message.From?.Id ?? currentUserId);
            var userTo = await _dataService.GetUserByIdAsync(message.To.Id.Value);

            if (userFrom == null || userTo == null)
            {
                _logger.LogError(LoggingEvents.ErrorOnCreateMessage, $"Can't send message - UserFrom is NULL: {userFrom == null}, UserTo is NULL: {userTo == null}");
                return BadRequest(new ErrorResponse { Message = "Should specify correct id for both sender and receiver" });
            }

            try
            {
                var result = await _dataService.SendMessageAsync(new ClientMessage
                {
                    From = userFrom.ToClientUser(),
                    To = userTo.ToClientUser(),
                    Message = message.Message
                });

                if (!result)
                {
                    _logger.LogError(LoggingEvents.ErrorOnCreateMessage, $"Message could not be sent");
                    return BadRequest(new ErrorResponse { Message = "Message could not be sent" });
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogException(LoggingEvents.ErrorOnCreateMessage, $"Error on sending new message", ex);

                var error = new ErrorResponse { Message = "Error has accured on sending message" };
                return StatusCode(StatusCodes.Status500InternalServerError, error);
            }
        }

        /// <summary>
        /// Update message
        /// </summary>
        /// <param name="message">Message</param>
        /// <response code="200">Message has been successfully removed</response>
        /// <response code="400">Message with the specified Id could not be found</response>
        /// <response code="500">Something went wrong</response>
        [Authorize(Roles = "user, admin")]
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] ClientMessage message)
        {
            _logger.LogInformation(LoggingEvents.UpdateMessage, $"Update Message with Id {message.Id}");

            var msg = await _dataService.GetMessageById(message.Id);

            var currentUserId = int.Parse(this.User.FindFirst(x => x.Type == ClaimTypes.Sid).Value);

            if (this.User.FindFirst(x => x.Type == ClaimsIdentity.DefaultRoleClaimType).Value == UserRole.USER.RoleName &&
                msg.SenderId != currentUserId)
            {
                _logger.LogInformation(LoggingEvents.UpdateMessage, "Request forbidden for current user");
                return Forbid();
            }

            try
            {
                var msg2 = await _dataService.UpdateMessageAsync(message);

                if (msg2 == null)
                {
                    _logger.LogInformation(LoggingEvents.ErrorOnUpdateMessage, $"Could not found Message with Id {message.Id}");
                    return BadRequest(new ErrorResponse { Message = $"Message with with the Id = {message.Id} could not be found" });
                }

                return Ok(msg2);
            }
            catch (Exception ex)
            {
                _logger.LogException(LoggingEvents.ErrorOnUpdateMessage, $"Error on updating Message with Id {message.Id}", ex);

                var error = new ErrorResponse { Message = "Error has accured on removing message" };
                return StatusCode(StatusCodes.Status500InternalServerError, error);
            }
        }

        /// <summary>
        /// Remove a message by Id
        /// </summary>
        /// <param name="id">Message Id</param>
        /// <response code="200">Message has been successfully removed</response>
        /// <response code="400">Message with the specified Id could not be found</response>
        /// <response code="500">Something went wrong</response>
        [Authorize(Roles = "user, admin")]
        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] int id)
        {
            _logger.LogInformation(LoggingEvents.DeleteMessageById, $"Delete Message with Id {id}");

            var msg = await _dataService.GetMessageById(id);

            var currentUserId = int.Parse(this.User.FindFirst(x => x.Type == ClaimTypes.Sid).Value);

            if (this.User.FindFirst(x => x.Type == ClaimsIdentity.DefaultRoleClaimType).Value == UserRole.USER.RoleName &&
                msg.SenderId != currentUserId)
            {
                _logger.LogInformation(LoggingEvents.DeleteMessageById, "Request forbidden for current user");
                return Forbid();
            }

            try
            {
                var message = await _dataService.DeleteMessageAsync(id);
                if (message == null)
                {
                    _logger.LogInformation(LoggingEvents.ErrorOnDeleteMessageById, $"Could not found Message with Id {message.Id}");
                    return BadRequest(new ErrorResponse { Message = $"Message with with the Id = {id} could not be found" });
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogException(LoggingEvents.ErrorOnDeleteMessageById, $"Error on deleting Message with Id {id}", ex);

                var error = new ErrorResponse { Message = "Error has accured on removing message" };
                return StatusCode(StatusCodes.Status500InternalServerError, error);
            }
        }

        // /// <summary>
        // /// Remove a message by Id
        // /// </summary>
        // /// <param name="id">Message Id</param>
        // /// <response code="200">Message has been successfully removed</response>
        // /// <response code="400">Message with the specified Id could not be found</response>
        // /// <response code="500">Something went wrong</response>
        // [Authorize(Roles = "user")]
        // [HttpDelete("user", Name = "DeleteByUserId")]
        // public async Task<IActionResult> DeleteByUser([FromBody] int id)
        // {
        //     _logger.LogInformation(LoggingEvents.DeleteMessageByUser, $"Delete Message for User with Id {id}");

        //     try
        //     {
        //         var message = await _dataService.DeleteMessageAsync(id);
        //         if (message == null)
        //         {
        //             _logger.LogInformation(LoggingEvents.ErrorOnDeleteMessageByUser, $"Could not found Message with Id {message.Id}");
        //             return BadRequest(new ErrorResponse { Message = $"Message with with the Id = {id} could not be found" });
        //         }

        //         return Ok();
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogException(LoggingEvents.ErrorOnDeleteUser, $"Error on deleting Message with Id {id}", ex);

        //         var error = new ErrorResponse { Message = "Error has accured on removing message" };
        //         return StatusCode(StatusCodes.Status500InternalServerError, error);
        //     }
        // }
    }
}