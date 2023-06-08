using AutoMapper;
using MeetingAppCore.DebugTracker;
using MeetingAppCore.Dtos;
using MeetingAppCore.Entities;
using MeetingAppCore.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace MeetingAppCore.Controllers
{

    public class AccountController : BaseApiController
    {
        private readonly ITokenService tokenService;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService, IMapper mapper)
        {
            this.tokenService = tokenService;
            _mapper = mapper;
            this.userManager = userManager;
            this.signInManager = signInManager;

        }

        [HttpPost("register")]
        //api/account/register?username=Test&password=hoainam10th with Register(string username, string password)
        public async Task<ActionResult<UserDto>> Register(RegisterDto register)
        {
            Console.WriteLine("1."+new String('=', 50));
            Console.WriteLine("1.Api/Acount: Register(RegisterDto)");
            FunctionTracker.Instance().AddApiFunc("Api/Acount: Api/Acount: Register(RegisterDto)");

            if (await UserExists(register.UserName))
                return BadRequest("Username is taken");

            var user = _mapper.Map<AppUser>(register);

            user.UserName = register.UserName.ToLower();

            var result = await userManager.CreateAsync(user, register.Password);

            if (!result.Succeeded) return BadRequest(result.Errors);

            var roleResult = await userManager.AddToRoleAsync(user, "Guest");
            if (!roleResult.Succeeded) return BadRequest(roleResult.Errors);

            var userDto = new UserDto
            {
                UserName = user.UserName,
                DisplayName = user.DisplayName,
                LastActive = user.LastActive,
                Token = await tokenService.CreateTokenAsync(user),
                PhotoUrl = null
            };

            return Ok(userDto);
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            Console.WriteLine("1."+new String('=', 50));
            Console.WriteLine("1.Api/Acount: Login(LoginDto)");
            FunctionTracker.Instance().AddApiFunc("Api/Acount: Login(LoginDto)");
            var user = await userManager.Users
                //.Include(p => p.Photos)
                .SingleOrDefaultAsync(x => x.UserName == loginDto.UserName.ToLower());

            if (user == null)
                return Unauthorized("Invalid Username");

            if (user.Locked)//true = locked
                return BadRequest("This account is loked by admin");

            var result = await signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (!result.Succeeded) return Unauthorized("Invalid password");

            var userDto = new UserDto
            {
                UserName = user.UserName,
                DisplayName = user.DisplayName,
                LastActive = user.LastActive,
                Token = await tokenService.CreateTokenAsync(user),
                PhotoUrl = user.PhotoUrl
            };
            return Ok(userDto);
        }

        [HttpPost("login-social")]
        public async Task<ActionResult<UserDto>> LoginSocial(LoginSocialDto loginDto)
        {
            Console.WriteLine("1."+new String('=', 50));
            Console.WriteLine("1.Api/Acount: LoginSocial(LoginSocial)");
            FunctionTracker.Instance().AddApiFunc("Api/Acount: LoginSocial(LoginSocial)");
            var user = await userManager.Users
                .SingleOrDefaultAsync(x => x.UserName == loginDto.Email);
            // email = username
            if (user != null)//có rồi thì đăng nhập bình thường
            {
                if (user.Locked)//true = locked
                    return BadRequest("This account is loked by admin");

                var result = await signInManager.CheckPasswordSignInAsync(user, loginDto.Email, false);

                if (!result.Succeeded) return Unauthorized("Invalid password");

                var userDto = new UserDto
                {
                    UserName = user.UserName,
                    DisplayName = user.DisplayName,
                    LastActive = user.LastActive,
                    Token = await tokenService.CreateTokenAsync(user),
                    PhotoUrl = user.PhotoUrl
                };
                return Ok(userDto);
            }
            else//Chưa có thì tạo mới user
            {
                var appUser = new AppUser
                {
                    UserName = loginDto.Email,
                    Email = loginDto.Email,
                    DisplayName = loginDto.Name,
                    PhotoUrl = loginDto.PhotoUrl
                };

                var result = await userManager.CreateAsync(appUser, loginDto.Email);//password là email

                if (!result.Succeeded) return BadRequest(result.Errors);

                var roleResult = await userManager.AddToRoleAsync(appUser, "Guest");
                if (!roleResult.Succeeded) return BadRequest(roleResult.Errors);

                var userDto = new UserDto
                {
                    UserName = appUser.UserName,
                    DisplayName = appUser.DisplayName,
                    LastActive = appUser.LastActive,
                    Token = await tokenService.CreateTokenAsync(appUser),
                    PhotoUrl = loginDto.PhotoUrl
                };

                return Ok(userDto);
            }
        }

        private async Task<bool> UserExists(string username)
        {
            Console.WriteLine("1." + new String('=', 50));
            Console.WriteLine("1.Api/Acount: UserExists(username");
            return await userManager.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}
