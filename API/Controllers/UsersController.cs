﻿using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;
        private readonly IPhotoService photoService;

        public UsersController(IUserRepository userRepository , IMapper mapper,IPhotoService photoService)
        {
            this.userRepository = userRepository;
            this.mapper = mapper;
            this.photoService = photoService;
        }

       
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDTO>>> GetUsers([FromQuery]UserParams userParams)
        {
            var user = await userRepository.GetUserByUserNameAsync(User.GetUserName());
            userParams.CurrentUserName = user.UserName ;

            if (!string.IsNullOrEmpty(userParams.CurrentUserName))
                userParams.Gender = userParams.Gender == "male" ? "male" : "female";

            var users = await userRepository.GetMembersAsync(userParams);

            Response.AddPaginationHeader(users.CurrentPage , users.Pagesize,
                users.TotalCount , users.TotalPages);

            return Ok(users);

            //alternate method
            //var users = await userRepository.GetUsersAsync()
            // return Ok(users) or return users;
        }

       
        [HttpGet("{UserName}" , Name ="GetUser")]
        
        public async Task<ActionResult<MemberDTO>> GetUser(string UserName)
        {
            var user = await userRepository.GetMemberAsync(UserName);
            return this.mapper.Map<MemberDTO>(user);
        }

        [HttpPut]

        public async Task<ActionResult> UpdateUser(MemberUpdateDTO memberUpdateDTO)
        {
            var UserName = User.GetUserName();
            var user = await userRepository.GetUserByUserNameAsync(UserName);

            mapper.Map(memberUpdateDTO, user);

            userRepository.Update(user);

            if (await userRepository.SaveAllAsync())
                return NoContent();

            return BadRequest("Failed to update user");
        }

        [HttpPost("add-photo")]

        public async Task<ActionResult<PhotoDTO>> AddPhoto(IFormFile file)
        {
            var user = await userRepository.GetUserByUserNameAsync( User.GetUserName());

            var result = await photoService.AddPhotoAsync(file);

            if (result.Error!=null)
            {
                return BadRequest(result.Error.Message);
            }

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            if (user.Photos.Count == 0)
            {
                photo.IsMain = true;
            }

            user.Photos.Add(photo);

            if (await userRepository.SaveAllAsync())
            {
                return CreatedAtRoute("GetUser" , new { UserName = user.UserName}, mapper.Map<PhotoDTO>(photo));
            }
           
            
            return BadRequest("Problem Adding Photo");

        }

        [HttpPut("set-main-photo/{photoId}")]

        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await userRepository.GetUserByUserNameAsync(User.GetUserName());

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo.IsMain) return BadRequest("This is already your main photo");

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);

            if(currentMain != null)
            {
                currentMain.IsMain = false;
                photo.IsMain = true;
            }

            if (await userRepository.SaveAllAsync())
                return NoContent();


            return BadRequest("Failed to set Main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]

        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await userRepository.GetUserByUserNameAsync(User.GetUserName());

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if(photo == null)
            {
                return NotFound();
            }

            if (photo.IsMain) return BadRequest("You Cannot Delete Main Photo");

            if (photo.PublicId != null)
            {
                var result = await photoService.DeletePhotoAsync(photo.PublicId);

                if (result.Error != null) return BadRequest(result.Error.Message);

            }

            user.Photos.Remove(photo);

            if (await userRepository.SaveAllAsync()) return Ok();


            return BadRequest("Failed to set Main photo");


        }


    }
}
