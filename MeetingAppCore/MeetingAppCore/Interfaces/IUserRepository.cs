using MeetingAppCore.Dtos;
using MeetingAppCore.Entities;
using MeetingAppCore.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingAppCore.Interfaces
{
    public interface IUserRepository
    {
        Task<AppUser> GetUserByIdAsync(Guid id);
        Task<AppUser> GetUserByUsernameSignalrAsync(string username);
        Task<MemberSignalrDto> GetMemberSignalrAsync(string username);
        Task<PagedList<MemberSignalrDto>> GetMembersAsync(UserParams userParams);
        Task<IEnumerable<MemberSignalrDto>> SearchMemberAsync(string displayname);
        Task<IEnumerable<MemberSignalrDto>> GetUsersOnlineAsync(UserConnectionSignalrDto[] userOnlines);
        Task<AppUser> UpdateLocked(string username);
    }
}
