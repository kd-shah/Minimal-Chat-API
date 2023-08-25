using Azure.Core;
using ChatApi.Context;
using ChatApi.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace ChatApi.Controllers
{
    [Authorize]
    [Route("api/messages")]
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
        public async Task<IActionResult> SendMessage(int receiverId, [FromBody] SendMessageDto request)
        {

            if (request == null)
            {
                return BadRequest(new { Message = "Message cannot be blank" });
            }



            //var messageId = Guid.NewGuid();
            var senderId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var sender = await _context.Users.FindAsync(Convert.ToInt32(senderId));
            if (sender == null)
            {
                return NotFound("Sender not found");
            }


            //var receiverId = request.receiverId;
            var receiver = await _context.Users.FindAsync(receiverId);
            if (receiver == null)
            {
                return NotFound("Receiver not found");
            }

            var content = request.content;
            var timestamp = DateTime.Now;

            var message = new Message
            {

                //senderId = Convert.ToInt32(senderId),
                sender = sender,

                //receiverId = receiverId,
                receiver = receiver,

                content = content,
                timestamp = timestamp,
            };

            if (!await _context.Users.AnyAsync(u => u.userId == receiverId))
                return Conflict(new { message = "User does not exist" });



            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var response = new
            {
                messageId = message.messageId,
                senderId = sender.userId,
                receiverId = receiver.userId,
                content = content,
                timestamp = timestamp
            };

            return Ok(response);
        }

        [HttpPut ("{messageId}")]
        
        public async Task<IActionResult> EditMessage(int messageId, [FromBody] EditMessageRequestDto request)
        {
            var authenticatedUserId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (request == null)
            {
                return NotFound("Message Not Found");

            }

            var message = await _context.Messages.FirstOrDefaultAsync(m => m.messageId == messageId);

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

        [HttpDelete("{messageId}")]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            var authenticatedUserId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (messageId == null)
            {
                return NotFound("Message Not Found");

            }

            var message = await _context.Messages.FirstOrDefaultAsync(m => m.messageId == messageId);

            if (message.senderId != Convert.ToInt32(authenticatedUserId))
            {
                return Unauthorized();
            }

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            return Ok("Message Deleted Successfully");
        }

        [HttpGet]
        public async Task<IActionResult> GetConversationHisotry(int userId, DateTime before, int count = 20, string sort = "asc")
        {

            var authenticatedUserId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var conversation = _context.Messages.Include(m => m.sender).Include(m => m.receiver).Where(m => (m.senderId == Convert.ToInt32(authenticatedUserId) && m.receiverId == userId) ||
                                                                  (m.senderId == userId && m.receiverId == Convert.ToInt32(authenticatedUserId)));



            if (!await _context.Users.AnyAsync(u => u.userId == userId))
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


            var chat = await conversation.Select(m => new
            {
                id = m.messageId,
                senderId = m.senderId,
                receiverId = m.receiverId,
                content = m.content,
                timestamp = m.timestamp,
            }).Take(count).ToListAsync();

            if (chat.Count == 0)
            {
                return NotFound("Conversation does not exist");
            }

            return Ok(chat);


        }


    }
}
