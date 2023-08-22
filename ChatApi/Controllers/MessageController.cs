using Azure.Core;
using ChatApi.Context;
using ChatApi.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        public readonly ChatDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public MessageController(ChatDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost]
        public IActionResult SendMessage([FromBody] RequestMessageDto request) {
            
            if (request == null)
            {
                return BadRequest(new { Message = "Message cannot be blank" });
            }

            

            var messageId = Guid.NewGuid();
            var senderId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var receiverId = request.receiverId;
            var content = request.content;
            var timestamp = DateTime.Now;

            var message = new Message
            {
                
                senderId = Convert.ToInt32(senderId),
                receiverId = receiverId,
                content = content,
                timestamp = timestamp,
            };

            if (_context.Users.Any(u => u.id != receiverId))
                return Conflict(new { message = "User does not exist" });

            _context.Messages.Add(message);
            _context.SaveChanges();

            var response = new
            {
                messageId = messageId,
                senderId = senderId,
                receiverId = receiverId,
                content = content,
                timestamp = timestamp
            };

            return Ok(response);
        }

        [HttpPut]
        [Route("api/messages/{messageId}")]
        public IActionResult EditMessage(int messageId, [FromBody] EditMessageRequestDto request)
        {
            var authenticatedUserId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if(request == null)
            {
                return NotFound("Message Not Found");

            }

            var message = _context.Messages.FirstOrDefault(m => m.id == messageId);

            if(message == null)
            {
                return NotFound("Message Not Found");
            }

            if (message.senderId != Convert.ToInt32(authenticatedUserId))
            {
                return Unauthorized();
            }
            message.content = request.content;
            _context.SaveChanges();
            return Ok("Message edited successfully");
        }

        [HttpDelete]
        [Route("api/messages/{messageId}")]
        public IActionResult DeleteMessage(int messageId)
        {
            var authenticatedUserId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (messageId == null)
            {
                return NotFound("Message Not Found");

            }

            var message = _context.Messages.FirstOrDefault(m => m.id == messageId);
            
            if (message.senderId != Convert.ToInt32(authenticatedUserId))
            {
                return Unauthorized();
            }

            _context.Messages.Remove(message);
            _context.SaveChanges();

            return Ok("Message Deleted Successfully");
        }

    }
}
