using AutoMapper;
using MeetingAppCore.Interfaces;
using MeetingAppCore.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingAppCore.Data
{
    public class UnitOfWork : IRepoWrapper
    {
        DbDataContext _context;
        IMapper _mapper;

        public UnitOfWork(DbDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IUserRepository Accounts => new UserRepository(_context, _mapper);
        public IMeetingRepository Meetings => new MeetingRepository(_context, _mapper);

        public async Task<bool> Complete()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public bool HasChanges()
        {
            return _context.ChangeTracker.HasChanges();
        }
    }
}
