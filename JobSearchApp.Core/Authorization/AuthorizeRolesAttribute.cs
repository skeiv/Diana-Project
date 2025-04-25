using Microsoft.AspNetCore.Authorization;
using JobSearchApp.Core.Enums;

namespace JobSearchApp.Core.Authorization
{
    public class AuthorizeRolesAttribute : AuthorizeAttribute
    {
        public AuthorizeRolesAttribute(params UserRole[] roles)
        {
            Roles = string.Join(",", roles);
        }
    }
} 