using Azure.Core;
using ChatApi.Context;
using ChatApi.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public async Task<IActionResult> SendMessage([FromBody] RequestMessageDto request)
        {

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

            if (!await _context.Users.AnyAsync(u => u.id == receiverId))
                return Conflict(new { message = "User does not exist" });



            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

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
        public async Task<IActionResult> EditMessage(int messageId, [FromBody] EditMessageRequestDto request)
        {
            var authenticatedUserId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (request == null)
            {
                return NotFound("Message Not Found");

            }

            var message = await _context.Messages.FirstOrDefaultAsync(m => m.id == messageId);

            if (message == null)
            {
                return NotFound("Message Not Found");
            }

            if (message.senderId != Convert.ToInt32(authenticatedUserId))
            {
                return Unauthorized();
            }
            message.content = request.content;
            await _context.SaveChangesAsync();
            return Ok("Message edited successfully");
        }

        [HttpDelete]
        [Route("api/messages/{messageId}")]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            var authenticatedUserId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (messageId == null)
            {
                return NotFound("Message Not Found");

            }

            var message = await _context.Messages.FirstOrDefaultAsync(m => m.id == messageId);

            if (message.senderId != Convert.ToInt32(authenticatedUserId))
            {
                return Unauthorized();
            }

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            return Ok("Message Deleted Successfully");
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetConversationHisotry(int userId, DateTime before, int count = 20, string sort = "asc")
        {

            var authenticatedUserId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var conversation = _context.Messages.Where(m => (m.senderId == Convert.ToInt32(authenticatedUserId) && m.receiverId == userId) ||
                                                                  (m.senderId == userId && m.receiverId == Convert.ToInt32(authenticatedUserId)));

            

            if (!await _context.Users.AnyAsync(u => u.id == userId))
                return Conflict(new { message = "User does not exist" });


            if (before != default(DateTime))
            {
                conversation = conversation.Where(m => m.timestamp < before);
            }

            if (before > DateTime.Now)
            {
                return BadRequest("Invalid Before Parameter");
            }

            if (sort != "asc" && sort != "desc")
            {
                return BadRequest("Invalid Request Parameter");
            }

            if (count <= 0)
            {
                return BadRequest("Invalid Request Parameter : Chat Count cannot be zero or negative");
            }
            conversation = sort == "desc" ? conversation.OrderByDescending(m => m.timestamp) : conversation.OrderBy(m => m.timestamp);

            

            var chat = await conversation
                        .Take(count)
                        .ToListAsync();

            if (chat.Count == null)
            {
                return NotFound("Conversation does not exist");
            }











            return Ok(chat);

        }


    }
}
