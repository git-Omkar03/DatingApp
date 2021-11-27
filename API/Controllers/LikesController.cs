using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Authorize]
    public class LikesController : BaseApiController
    {
        private readonly IUserRepository userRepository;
        private readonly ILikesRepository likesRepository;

        public LikesController(IUserRepository userRepository , ILikesRepository likesRepository)
        {
            this.userRepository = userRepository;
            this.likesRepository = likesRepository;
        }


        [HttpPost("{UserName}")]
        public async Task<ActionResult> AddLike(string UserName)
        {
            var sourceUserId = User.GetUserId();
            var likedUser = await userRepository.GetUserByUserNameAsync(UserName);
            var sourceuser = await likesRepository.GetUserWithLikes(sourceUserId);

            if (likedUser == null) return NotFound();

            if (sourceuser.UserName == UserName) return BadRequest("You cant like Yourself");

            var userLike = await likesRepository.GetUserLike(sourceUserId, likedUser.Id);

            if (userLike != null)  return BadRequest("You already liked this user"); 

            userLike = new Entities.UserLike
            {
                SourceUserID = sourceUserId,
                LikedUserID = likedUser.Id
            };

            sourceuser.LikedUsers.Add(userLike);

            if (await userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Like didnt worked");
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();
            var users = await likesRepository.GetUserLikes(likesParams);

            Response.AddPaginationHeader(users.CurrentPage, users.Pagesize
                , users.TotalCount, users.TotalPages);

            return Ok(users);
        }
    }
}
