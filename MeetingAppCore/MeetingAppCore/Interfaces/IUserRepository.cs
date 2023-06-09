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
        Task<AppUser> GetUserByUsernameAsync(string username);
        Task<MemberSignalrDto> GetMemberAsync(string username);
        Task<PagedList<MemberSignalrDto>> GetMembersAsync(UserParams userParams);
        Task<IEnumerable<MemberSignalrDto>> SearchMemberAsync(string displayname);
        Task<IEnumerable<MemberSignalrDto>> GetUsersOnlineAsync(UserConnectionDto[] userOnlines);
        Task<AppUser> UpdateLocked(string username);
    }
}
