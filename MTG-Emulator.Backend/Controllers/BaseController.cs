using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MTG_Emulator.Backend.DB.Models;

namespace MTG_Emulator.Backend.Controllers
{
    public class BaseController : ControllerBase
    {
        protected bool IsOwnerOrAdmin(string resourceApiUserId)
        {
            var callerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return resourceApiUserId == callerId || User.IsInRole(Roles.Admin);
        }
    }
}