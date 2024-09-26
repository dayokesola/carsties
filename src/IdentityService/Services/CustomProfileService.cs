using System;
using System.Security.Claims;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IdentityModel;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Services;

public class CustomProfileService : IProfileService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public CustomProfileService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var user = await _userManager.GetUserAsync(context.Subject);
        if (user != null)
        {
            var exClaims = await _userManager.GetClaimsAsync(user);
            if (!string.IsNullOrEmpty(user.UserName))
            {
                var claims = new List<Claim> { new Claim("username", user.UserName) };
                context.IssuedClaims.AddRange(claims);
                var uname = exClaims.FirstOrDefault(x => x.Type == JwtClaimTypes.Name);
                if (uname != null)
                {
                    context.IssuedClaims.Add(uname);
                }
            }
        }
    }

    public Task IsActiveAsync(IsActiveContext context)
    {
        return Task.CompletedTask;
    }
}
