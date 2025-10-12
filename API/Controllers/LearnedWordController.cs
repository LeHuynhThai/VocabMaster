using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Repository.DTOs;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class LearnedWordController : ControllerBase
    { 
        public LearnedWordController()
        {
        }
        

        

        private int GetUserIdFromClaims()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier) ??
                              User.Claims.FirstOrDefault(c => c.Type == "UserId");

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }

            return 0;
        }
    }
}
