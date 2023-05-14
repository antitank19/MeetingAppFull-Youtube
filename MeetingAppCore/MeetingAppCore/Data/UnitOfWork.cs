using AutoMapper;
using MeetingAppCore.Interfaces;
using MeetingAppCore.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingAppCore.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        DbDataContext _context;
        IMapper _mapper;

        public UnitOfWork(DbDataContext context, IMapper mapper)
        {
            Console.WriteLine("\t\t"+new String('~', 10));
            Console.WriteLine("UnitOfWork: ctor(DbDataContext, IMapper)");
            _context = context;
            _mapper = mapper;
        }

        public IUserRepository UserRepository => new UserRepository(_context, _mapper);
        public IRoomRepository RoomRepository => new RoomRepository(_context, _mapper);

        public async Task<bool> Complete()
        {
            Console.WriteLine("\t\t" + new String('~', 10));
            Console.WriteLine("UnitOfWork:Complete()");
            return await _context.SaveChangesAsync() > 0;
        }

        public bool HasChanges()
        {
            Console.WriteLine("\t\t" + new String('~', 10));
            Console.WriteLine("UnitOfWork:HasChanges()");
            return _context.ChangeTracker.HasChanges();
        }
    }
}
