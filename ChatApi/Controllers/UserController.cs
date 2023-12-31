﻿using ChatApi.Context;
using ChatApi.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.RegularExpressions;
using System;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;

namespace ChatApi.Controllers
{
    [Route("api/")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ChatDbContext _authContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserController(ChatDbContext chatDbContext, IHttpContextAccessor httpContextAccessor)
        {
            _authContext = chatDbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Authenticate([FromBody] Model.LoginDto UserObj)
        {
            if (UserObj == null)
                return BadRequest();

            if (!IsValidEmail(UserObj.email))
                return BadRequest(new { Message = "Invalid email format" });

            //if (!IsValidPassword(UserObj.password))
            //    return BadRequest(new { Message = "Invalid password format" });


            var user = await _authContext.Users.FirstOrDefaultAsync(x => x.email == UserObj.email);
            if (user == null)
                return NotFound(new { Message = "Login failed due to incorrect credentials" });

            if (!PasswordHasher.VerifyPassword(UserObj.password, user.password))
            {
                return BadRequest(new
                {
                    Message = "Incorrect Password"
                });
            }

            var userRes = new
            {
                id = user.userId,
                name = user.name,
                email = user.email,

            };

            user.token = CreateJwt(user);

            return Ok(new
            {
                Message = " Login Success",
                UserInfo = userRes,
                token = user.token
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] Model.RegisterRequestDto UserObj)
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


            var newUser = new Model.User
            {
                name = UserObj.name,
                email = UserObj.email,
                password = UserObj.password,
                token = ""

            };

            _authContext.Users.Add(newUser);
            await _authContext.SaveChangesAsync();

            var userResponse = new
            {
                id = newUser.userId,
                name = newUser.name,
                email = newUser.email,
            };


            return Ok(new
            {
                Message = "User Registered",
                UserInfo = userResponse,
            });

        }

        [Authorize]
        [HttpGet("users")]
        public async Task<ActionResult<Model.User>> GetAllUsers()
        {


            var currentUser = GetCurrentLoggedInUser();

            if (currentUser == null)
            {
                return BadRequest(new { Message = "Unable to retrieve current user." });
            }

            var userList = await _authContext.Users
        .Where(u => u.userId != currentUser.userId)
        .Select(u => new
        {
            id = u.userId,
            name = u.name,
            email = u.email,
        })
        .ToListAsync();

            return Ok(new { users = userList });
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

        private string CreateJwt(Model.User user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("It Is A Secret Key Which Should Not Be Shared With Other Users.....");

            var claims = new List<Claim>
    {
            new Claim(ClaimTypes.Name, user.name),
            new Claim(ClaimTypes.NameIdentifier, user.userId.ToString())
    };
            var identity = new ClaimsIdentity(claims);
            

            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = credentials,
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);

            return jwtTokenHandler.WriteToken(token);
        }

        private Model.User GetCurrentLoggedInUser()
        {
            var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                var currentUser = _authContext.Users.FirstOrDefault(u => u.userId == userId);
                return currentUser;
            }

            return null;
        }





    }
}
