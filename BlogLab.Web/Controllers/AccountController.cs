using System;
using System.Threading.Tasks;
using BlogLab.Models.Account;
using BlogLab.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlogLab.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly UserManager<ApplicationUserIdentity> _userManager;
        private readonly SignInManager<ApplicationUserIdentity> _signInManager;

        public AccountController
            (
            ITokenService tokenService,
            UserManager<ApplicationUserIdentity> userManager,
            SignInManager<ApplicationUserIdentity> signInManager
            )
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _signInManager = signInManager; 
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApplicationUser>> Register(ApplicationUserCreate applicationUserCreate)
        // ActionResult<ApplicationUser> either returns an ApplicationUser or the result of the action
        {
            var applicationUserIdentity = new ApplicationUserIdentity()
            {
                Username = applicationUserCreate.Username,
                Email = applicationUserCreate.Email,
                Fullname = applicationUserCreate.Fullname
            };

            var result = await _userManager.CreateAsync(applicationUserIdentity, applicationUserCreate.Password);

            if (result.Succeeded)
            {
                ApplicationUser user = new ApplicationUser()
                {
                    ApplicationUserId = applicationUserIdentity.ApplicationUserId,
                    Username = applicationUserIdentity.Username,
                    Fullname = applicationUserIdentity.Fullname,
                    Email = applicationUserIdentity.Email,
                    Token = _tokenService.CreateToken(applicationUserIdentity)
                };

                return user;
            }

            return BadRequest(result.Errors);          
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApplicationUser>> Login(ApplicationUserLogin applicationUserLogin)
        {   
            var applicationUserIdentity = await _userManager.FindByNameAsync(applicationUserLogin.Username); // creates a user based on the login data provided

            if(applicationUserIdentity != null)
            {
                var result = await _signInManager.CheckPasswordSignInAsync(
                    applicationUserIdentity, 
                    applicationUserLogin.Password,
                    false
                    );
                if (result.Succeeded)
                {
                    ApplicationUser user = new ApplicationUser()
                    {
                        ApplicationUserId = applicationUserIdentity.ApplicationUserId,
                        Username = applicationUserIdentity.Username,
                        Fullname = applicationUserIdentity.Fullname,
                        Email = applicationUserIdentity.Email,
                        Token = _tokenService.CreateToken(applicationUserIdentity)
                    };

                    return Ok(user);
                }
            }

            return BadRequest("Invalid login attempt");
        }
    }
}
