using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChatApi.Context;
using ChatApi.Model;
using System;

namespace ChatApi.Controllers
{
    [Authorize]
    [Route("api/")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private readonly ChatDbContext _context;
        public LogController(ChatDbContext context)
        {
            _context = context;
        }

        [HttpGet("log")]
        public async Task<IActionResult> GetLogs([FromQuery] string startTime = null, [FromQuery] string endTime = null)
        {
            DateTime? parsedStartTime = ParseDateTime(startTime);
            DateTime? parsedEndTime = ParseDateTime(endTime);
            if (parsedStartTime == null)
                parsedStartTime = DateTime.Now.AddMinutes(-5);
            if (parsedEndTime == null)
                parsedEndTime = DateTime.Now;
            var logs = await _context.Logs
                .Where(log => log.timeStamp >= parsedStartTime && log.timeStamp <= parsedEndTime)
                .ToListAsync();
            if (logs.Count == 0)
                return NotFound();
            return Ok(logs);
        }
        private DateTime? ParseDateTime(string dateTimeString)
        {
            if (string.IsNullOrEmpty(dateTimeString))
                return null;
            if (DateTime.TryParse(dateTimeString, out DateTime parsedDateTime))
                return parsedDateTime;
            return null;
        }
    }
}







