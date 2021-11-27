using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly UserManager<AppUser> userManager;
        private readonly ITokenService tokenservice;
        private readonly IMapper mapper;
        private readonly SignInManager<AppUser> signInManager;

        public AccountController(UserManager<AppUser> userManager,ITokenService tokenservice,IMapper mapper,SignInManager<AppUser> signInManager)
        {
        
            this.userManager = userManager;
            this.tokenservice = tokenservice;
            this.mapper = mapper;
            this.signInManager = signInManager;
        }


        [HttpPost("register")]
        
        public async Task<ActionResult<UserDTO>> Register(RegisterDTOs registerDto)
        {

            if (await UserExists(registerDto.UserName)) return BadRequest("UserName is Taken");
           
            var user = this.mapper.Map<AppUser>(registerDto);

            user.UserName = registerDto.UserName.ToLower();

            var result = await userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded) return BadRequest(result.Errors);

            var roleResult = await userManager.AddToRoleAsync(user, "Member");

            if(!roleResult.Succeeded) return BadRequest(result.Errors);

            return new UserDTO {
                UserName = user.UserName,
                Token = await tokenservice.CreateToken(user),
                knownAs = user.KnownAs,
                Gender = user.Gender
            };
        }

        [HttpPost("login")]
        
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
        {
            var user = await userManager.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => x.UserName == loginDTO.UserName.ToLower());

            if (user == null) { return Unauthorized("Invalid Useraname");  }

            var result = await signInManager.CheckPasswordSignInAsync(user, loginDTO.Password, false);

            if (!result.Succeeded) return Unauthorized();

            return new UserDTO
            {
                UserName = user.UserName,
                Token = await tokenservice.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                knownAs = user.KnownAs,
                Gender = user.Gender
            };
        }



        private async Task<bool> UserExists(string UserName)
        {
            return await userManager.Users.AnyAsync(x => x.UserName == UserName.ToLower()  );
        }

    }
}
