using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Playarr.Http.Authentication
{
    public class BypassableDenyAnonymousAuthorizationRequirement : DenyAnonymousAuthorizationRequirement
    {
    }
}
