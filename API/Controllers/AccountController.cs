using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
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
        private readonly DataContext _context;
        private readonly ITokenService tokenservice;
        private readonly IMapper mapper;

        public AccountController(DataContext context,ITokenService tokenservice,IMapper mapper)
        {
            _context = context;
            this.tokenservice = tokenservice;
            this.mapper = mapper;
        }


        [HttpPost("register")]
        
        public async Task<ActionResult<UserDTO>> Register(RegisterDTOs registerDto)
        {
            using var hmac = new HMACSHA512();

            if (await UserExists(registerDto.UserName)) return BadRequest("Username is Taken");
           
            var user = this.mapper.Map<AppUser>(registerDto);

            user.Username = registerDto.UserName.ToLower();
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
            user.PasswordSalt = hmac.Key;
      

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            return new UserDTO {
                Username = user.Username,
                Token = tokenservice.CreateToken(user),
                knownAs = user.KnownAs,
                Gender = user.Gender
            };
        }

        [HttpPost("login")]
        
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
        {
            var user = await _context.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => x.Username == loginDTO.UserName.ToLower());

            if (user == null) { return Unauthorized("Invalid Useraname");  }

            using var hmac = new HMACSHA512(user.PasswordSalt);

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));

            for (int i= 0 ; i<computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i])
                {
                    return Unauthorized("Invalid Password");
                }
            }

            return new UserDTO
            {
                Username = user.Username,
                Token = tokenservice.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                knownAs = user.KnownAs,
                Gender = user.Gender
            };
        }



        private async Task<bool> UserExists(string username)
        {
            return await _context.Users.AnyAsync(x => x.Username == username.ToLower()  );
        }

    }
}
