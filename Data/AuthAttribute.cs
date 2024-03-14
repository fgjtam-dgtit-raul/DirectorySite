using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DirectorySite.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace DirectorySite.Data
{
    public class AuthAttribute : Attribute, IAuthorizationFilter 
    {  
        
        private readonly JwtSettings _jwtSettings;

        public AuthAttribute()
        {
            // TODO: Inject this settings
            _jwtSettings =  new JwtSettings(){
                Issuer="https://directoryAPI.fgjtam.gob.mx",
                Audience="https://fgjtam.gob.mx",
                Key="e4b412d9dca447f5a651e0e729c5eeb9"
            };
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if(context != null){
                
                // Validate session
                var token = context.HttpContext.Session.GetString("JWTToken");
                if(token == null){
                    context.HttpContext.Response.Redirect("/authentication");
                    return;
                }

                // Validate token
                try {
                    context.HttpContext.User = TokenValidator.ValidateToken( token, _jwtSettings );
                }
                catch (Exception ) {
                    context.HttpContext.Response.Redirect("/authentication");
                    return;
                }
            }
        }
    }  
}