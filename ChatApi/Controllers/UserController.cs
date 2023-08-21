using ChatApi.Context;
using ChatApi.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace ChatApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ChatDbContext _authContext;
        public UserController(ChatDbContext chatDbContext)
        {
            _authContext = chatDbContext;
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] Model.User UserObj)
        {
            if (UserObj == null)
                return BadRequest();

            if (!IsValidEmail(UserObj.email))
                return BadRequest(new { Message = "Invalid email format" });

            //if (!IsValidPassword(UserObj.password))
            //    return BadRequest(new { Message = "Invalid password format" });


            var user = await _authContext.Users.FirstOrDefaultAsync(x => x.email == UserObj.email && x.password == UserObj.password);
            if (user == null)
                return NotFound(new { Message = "Login failed due to incorrect credentials" });

            var userRes = new
            {
                id = user.id,
                name = user.name,
                email = user.email,

            };


            return Ok(new
            {
                Message = " Login Success",
                UserInfo = userRes
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] Model.User UserObj)
        {
            if (UserObj == null)
                return BadRequest();

            if (!IsValidEmail(UserObj.email))
                return BadRequest(new { Message = "Invalid email format" });

            if (!IsValidPassword(UserObj.password))
                return BadRequest(new { Message = "Invalid password format" });

            if (await _authContext.Users.AnyAsync(u => u.email.ToLower() == UserObj.email.ToLower()))
                return Conflict(new { message = "Registration failed because the email is already registered" });

            UserObj.password = PasswordHasher.HashPassword(UserObj.password);
            UserObj.token = "";

            await _authContext.Users.AddAsync(UserObj);
            await _authContext.SaveChangesAsync();

            var userRes = new
            {
                id = UserObj.id,
                name = UserObj.name,
                email = UserObj.email,

            };

            return Ok(new
            {
                Message = "User Registered",
                UserInfo = userRes,
            });

        }

        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
        }

        private bool IsValidPassword(string password)
        {
            int requiredLength = 8;
            if (password.Length < requiredLength)
                return false;

            return true;
        }




    }
}
