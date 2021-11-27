﻿using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext context;
        private readonly IMapper mapper;

        public UserRepository(DataContext context ,IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

      
        public async Task<MemberDTO> GetMemberAsync(string UserName)
        {
            return await context.Users
               .Where(x => x.UserName == UserName)
               .ProjectTo<MemberDTO>(mapper.ConfigurationProvider)
               .SingleOrDefaultAsync();
        }


        public async Task<PagedList<MemberDTO>> GetMembersAsync(UserParams userParams)
        {
            var query =  context.Users.AsNoTracking().AsQueryable() ;

            query = query.Where(u => u.UserName != userParams.CurrentUserName);
            query = query.Where(u => u.Gender == userParams.Gender);

            var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
            var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

            query = query.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);

            query = userParams.OrderBy switch
            {
                "created" => query.OrderByDescending(u => u.Created),
                _ => query.OrderByDescending(u => u.LastActive)
            };

            return await PagedList<MemberDTO>.CreateAsync(query.ProjectTo<MemberDTO>
                (mapper.ConfigurationProvider).AsNoTracking(),
                userParams.PageNumber,userParams.PageSize);
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            await context.Users.Include(p => p.Photos).ToListAsync();
            return await context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUserNameAsync(string UserName)
        {
            await context.Users.Include(p => p.Photos).ToListAsync();
            return await context.Users.SingleOrDefaultAsync(x => x.UserName == UserName);
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            await context.Users.Include(p => p.Photos).ToListAsync();
            return await context.Users.ToListAsync();
        }

        public async Task<bool> SaveAllAsync()
        {
            return await context.SaveChangesAsync() > 0;
        }

        public void Update(AppUser user)
        {
            context.Entry(user).State = EntityState.Modified;
        }

        
    }
}
