using AutoMapper;
using AutoMapper.QueryableExtensions;
using MeetingAppCore.Data;
using MeetingAppCore.DebugTracker;
using MeetingAppCore.Dtos;
using MeetingAppCore.Entities;
using MeetingAppCore.Helpers;
using MeetingAppCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingAppCore.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly DbDataContext _context;
        private readonly IMapper _mapper;
        public UserRepository(DbDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<AppUser> GetUserByIdAsync(Guid id)
        {
            //Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("4.         Repo/User: GetUserByIdAsync(id)");
            FunctionTracker.Instance().AddRepoFunc("Repo/Room: GetRoomById(id)");
            return await _context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            //Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("4.         Repo/User: GetUserByUsernameAsync(username)");
            FunctionTracker.Instance().AddRepoFunc("Repo/User: GetUserByUsernameAsync(username)");
            return await _context.Users.SingleOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<MemberSignalrDto> GetMemberAsync(string username)
        {
            //Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("4.         Repo/User: GetMemberAsync(username)");
            FunctionTracker.Instance().AddRepoFunc("Repo/User: GetMemberAsync(username)");
            return await _context.Users.Where(x => x.UserName == username)
                .ProjectTo<MemberSignalrDto>(_mapper.ConfigurationProvider)//add CreateMap<AppUser, MemberDto>(); in AutoMapperProfiles
                .SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<MemberSignalrDto>> GetUsersOnlineAsync(UserConnectionDto[] userOnlines)
        {
            //Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("4.         Repo/User: GetUsersOnlineAsync(UserConnectionDto[])");
            FunctionTracker.Instance().AddRepoFunc("Repo/User: GetUsersOnlineAsync(UserConnectionDto[])");
            List<MemberSignalrDto> listUserOnline = new List<MemberSignalrDto>();
            foreach (var u in userOnlines)
            {
                MemberSignalrDto user = await _context.Users.Where(x => x.UserName == u.UserName)
                .ProjectTo<MemberSignalrDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();

                listUserOnline.Add(user);
            }
            //return await Task.Run(() => listUserOnline.ToList());
            return await Task.FromResult(listUserOnline.ToList());
        }

        public async Task<PagedList<MemberSignalrDto>> GetMembersAsync(UserParams userParams)
        {
            //Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("4.         Repo/User: GetMembersAsync(UserParams)");
            FunctionTracker.Instance().AddRepoFunc("Repo/User: GetMembersAsync(UserParams)");
            var query = _context.Users.AsQueryable();
            query = query.Where(u => u.UserName != userParams.CurrentUsername).OrderByDescending(u => u.LastActive);

            return await PagedList<MemberSignalrDto>.CreateAsync(query.ProjectTo<MemberSignalrDto>(_mapper.ConfigurationProvider).AsNoTracking(), userParams.PageNumber, userParams.PageSize);
        }

        public async Task<IEnumerable<MemberSignalrDto>> SearchMemberAsync(string displayname)
        {
            //Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("4.         Repo/User: SearchMemberAsync(name)");
            FunctionTracker.Instance().AddRepoFunc("Repo/User: SearchMemberAsync(name)");
            return await _context.Users.Where(u => u.DisplayName.ToLower().Contains(displayname.ToLower()))
                .ProjectTo<MemberSignalrDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<AppUser> UpdateLocked(string username)
        {
            //Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("4.         Repo/User: UpdateLocked(username)");
            FunctionTracker.Instance().AddRepoFunc("Repo/User: UpdateLocked(username)");
            var user = await _context.Users.SingleOrDefaultAsync(x => x.UserName == username);
            if(user != null)
            {
                user.Locked = !user.Locked;
            }
            return user;
        }
    }
}
