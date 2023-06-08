using AutoMapper;
using MeetingAppCore.Data;
using MeetingAppCore.Dtos;
using MeetingAppCore.Entities;
using MeetingAppCore.Helpers;
using MeetingAppCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using AutoMapper.QueryableExtensions;
using System.Linq;
using System.Threading.Tasks;
using MeetingAppCore.DebugTracker;

namespace MeetingAppCore.Repository
{
    public class RoomRepository : IRoomRepository
    {
        private readonly DbDataContext _context;
        private readonly IMapper _mapper;

        public RoomRepository(DbDataContext context, IMapper mapper)
        {
            Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("4.         Repo/Room: ctor()");
            _context = context;
            _mapper = mapper;
        }

        public async Task<Meeting> GetRoomById(int roomId)
        {
            Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("4.         Repo/Room: GetRoomById(id)");
            FunctionTracker.Instance().AddRepoFunc("Repo/Room: GetRoomById(id)");
            return await _context.Rooms.Include(x => x.Connections).FirstOrDefaultAsync(x => x.RoomId == roomId);
        }

        public async Task<MeetingDto> GetRoomDtoById(int roomId)
        {
            Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("4.         Repo/Room: GetRoomDtoById(id)");
            FunctionTracker.Instance().AddRepoFunc("Repo/Room: GetRoomDtoById(id)");
            return await _context.Rooms.Where(r => r.RoomId == roomId).ProjectTo<MeetingDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();//using Microsoft.EntityFrameworkCore;
        }

        public async Task<Meeting> GetMeetingForConnection(string connectionId)
        {
            Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("4.         Repo/Room: GetRoomForConnection(connectionId)");
            FunctionTracker.Instance().AddRepoFunc("Repo/Room: GetRoomForConnection(connectionId)");
            return await _context.Rooms.Include(x => x.Connections)
                .Where(x => x.Connections.Any(c => c.ConnectionId == connectionId))
                .FirstOrDefaultAsync();
        }

        public void RemoveConnection(Connection connection)
        {
            Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("4.         Repo/Room: RemoveConnection(Connection)");
            FunctionTracker.Instance().AddRepoFunc("Repo/Room: RemoveConnection(Connection)");
            _context.Connections.Remove(connection);
        }

        public void AddRoom(Meeting room)
        {
            Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("4.         Repo/Room: AddRoom(Room)");
            FunctionTracker.Instance().AddRepoFunc("Repo/Room: AddRoom(Room)");
            _context.Rooms.Add(room);
        }

        /// <summary>
        /// return null no action to del else delete thanh cong
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Meeting> DeleteRoom(int id)
        {
            Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("4.         Repo/Room: DeleteRoom(id)");
            FunctionTracker.Instance().AddRepoFunc("Repo/Room: DeleteRoom(id)");
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                _context.Rooms.Remove(room);
            }
            return room;
        }

        public async Task<Meeting> EditRoom(int id, string newName)
        {
            Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("4.         Repo/Room: EditRoom(id, name)");
            FunctionTracker.Instance().AddRepoFunc("Repo/Room: EditRoom(id, name)");
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                room.RoomName = newName;
            }
            return room;
        }

        public async Task DeleteAllRoom()
        {
            Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("4.         Repo/Room: DeleteAllRoom()");
            FunctionTracker.Instance().AddRepoFunc("Repo/Room: DeleteAllRoom()");
            var list = await _context.Rooms.ToListAsync();
            _context.RemoveRange(list);
        }

        public async Task<PagedList<MeetingDto>> GetAllRoomAsync(RoomParams roomParams)
        {
            Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("4.         Repo/Room: GetAllRoomAsync(RoomParams)");
            FunctionTracker.Instance().AddRepoFunc("Repo/Room: GetAllRoomAsync(RoomParams)");
            var list = _context.Rooms.AsQueryable();
            //using AutoMapper.QueryableExtensions; list.ProjectTo<RoomDto>
            return await PagedList<MeetingDto>.CreateAsync(list.ProjectTo<MeetingDto>(_mapper.ConfigurationProvider).AsNoTracking(), roomParams.PageNumber, roomParams.PageSize);
        }

        public async Task UpdateCountMember(int roomId, int count)
        {
            Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("4.         Repo/Room: UpdateCountMember(id, count)");
            FunctionTracker.Instance().AddRepoFunc("Repo/Room: UpdateCountMember(id, count)");
            var room = await _context.Rooms.FindAsync(roomId);
            if (room != null)
            {
                room.CountMember = count;
            }
        }
    }
}
