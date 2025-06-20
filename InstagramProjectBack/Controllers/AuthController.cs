﻿using InstagramProjectBack.Models;
using InstagramProjectBack.Models.Dto;
using InstagramProjectBack.Repositories;
using InstagramProjectBack.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace InstagramProjectBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly TokenService _tokenService;

        public AuthController(IAuthService authService,TokenService tokenService)
        {
            _authService = authService;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
        {
            try
            {
                var response = await _authService.Register(dto);

                if (!response.Success)
                {
                    if (response.Message.Contains("already"))
                        return Conflict(new { response.Message });
                    return BadRequest(new { response.Message });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Server error", Details = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            try
            {
                var response = await _authService.Login(dto);

                if (!response.Success)
                    return Unauthorized(new { response.Message });

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Server error", Details = ex.Message });
            }
        }

        [HttpPost("send-verification")]
        public async Task<IActionResult> SendVerification([FromQuery] string email)
        {
            try
            {
                var response = await _authService.ResendVerificationTokenAsync(email);

                if (!response.Success)
                {
                    if (response.Message.Contains("not found"))
                        return NotFound(new { response.Message });
                    return BadRequest(new { response.Message });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Server error", Details = ex.Message });
            }
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyRequestDto dto)
        {
            try
            {
                bool isVerified = await _authService.VerifyUserAsync(dto.token);

                if (!isVerified)
                    return BadRequest(new { Message = "User verification failed." });

                return Ok(new { Message = "User verified successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Server error", Details = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("GetUser")]
        public async Task<IActionResult> GetUser()
        {
            try
            {
                int userId = _tokenService.GetUserIdFromHttpContext(HttpContext);
                var result = await _authService.GetUserAsync(userId);
                if (result.Success == false)
                {
                    return BadRequest(new { Message = "invalid user id." });
                }

                return Ok(new { User = result.Data });
            }
            catch (Exception ex)
            {

                return StatusCode(500, new { Message = "Server error", Details = ex.Message });
            }

            
        }
        
        
        [HttpGet("ping")]
        public IActionResult Ping()
        {
           return Ok("Alive");
        }
    }
}
