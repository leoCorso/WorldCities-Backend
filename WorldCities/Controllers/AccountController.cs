using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorldCities.Data;
using WorldCities.Data.Models;

namespace WorldCities.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly JwtHandler _jwtHandler;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(ApplicationDbContext dbContext, JwtHandler jwtHandler, UserManager<ApplicationUser> userManager) { 
            _dbContext = dbContext;
            _jwtHandler = jwtHandler;
            _userManager = userManager;
        }

        [HttpPost("Login")]
        public async Task<ActionResult<ApiLoginResult>> Login(ApiLoginRequest loginRequest)
        {
            var user = await _userManager.FindByNameAsync(loginRequest.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, loginRequest.Password))
            {
                return Unauthorized(new ApiLoginResult
                {
                    Success = false,
                    Message = "Invalid Credentials Provided"
                });
            }
            var secToken = await _jwtHandler.GetTokenAsync(user);
            var jwt = new JwtSecurityTokenHandler().WriteToken(secToken);
            return Ok(new ApiLoginResult
            {
                Success = true,
                Message = "Login Successful",
                Token = jwt
            });
        }

    }
}
