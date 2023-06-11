using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingAppCore.Interfaces
{
    public interface IRepoWrapper
    {
        IUserRepository Accounts { get; }
        IMeetingRepository Meetings { get; }
        Task<bool> Complete();
        bool HasChanges();
    }
}
