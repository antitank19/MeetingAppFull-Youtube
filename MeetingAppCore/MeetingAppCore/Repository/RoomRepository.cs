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

namespace MeetingAppCore.Repository
{
    public class RoomRepository : IRoomRepository
    {
        private readonly DbDataContext _context;
        private readonly IMapper _mapper;

        public RoomRepository(DbDataContext context, IMapper mapper)
        {
            Console.WriteLine("\t\t\t" + new String('~', 10));
            Console.WriteLine("Repo/Room:ctor()");
            _context = context;
            _mapper = mapper;
        }

        public async Task<Room> GetRoomById(int roomId)
        {
            Console.WriteLine("\t\t\t" + new String('~', 10));
            Console.WriteLine("Repo/Room:GetRoomById(id)");
            return await _context.Rooms.Include(x => x.Connections).FirstOrDefaultAsync(x => x.RoomId == roomId);
        }

        public async Task<RoomDto> GetRoomDtoById(int roomId)
        {
            Console.WriteLine("\t\t\t" + new String('~', 10));
            Console.WriteLine("Repo/Room:GetRoomDtoById(id)");
            return await _context.Rooms.Where(r => r.RoomId == roomId).ProjectTo<RoomDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();//using Microsoft.EntityFrameworkCore;
        }

        public async Task<Room> GetRoomForConnection(string connectionId)
        {
            Console.WriteLine("\t\t\t" + new String('~', 10));
            Console.WriteLine("Repo/Room:GetRoomForConnection(connectionId)");
            return await _context.Rooms.Include(x => x.Connections)
                .Where(x => x.Connections.Any(c => c.ConnectionId == connectionId))
                .FirstOrDefaultAsync();
        }

        public void RemoveConnection(Connection connection)
        {
            Console.WriteLine("\t\t\t" + new String('~', 10));
            Console.WriteLine("Repo/Room:RemoveConnection(Connection)");
            _context.Connections.Remove(connection);
        }

        public void AddRoom(Room room)
        {
            Console.WriteLine("\t\t\t" + new String('~', 10));
            Console.WriteLine("Repo/Room:AddRoom(Room)");
            _context.Rooms.Add(room);
        }

        /// <summary>
        /// return null no action to del else delete thanh cong
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Room> DeleteRoom(int id)
        {
            Console.WriteLine("\t\t\t" + new String('~', 10));
            Console.WriteLine("Repo/Room:DeleteRoom(id)");
            var room = await _context.Rooms.FindAsync(id);
            if(room != null)
            {
                _context.Rooms.Remove(room);
            }
            return room;
        }

        public async Task<Room> EditRoom(int id, string newName)
        {
            Console.WriteLine("\t\t\t" + new String('~', 10));
            Console.WriteLine("Repo/Room:EditRoom(id, name)");
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                room.RoomName = newName;
            }
            return room;
        }

        public async Task DeleteAllRoom()
        {
            Console.WriteLine("\t\t\t" + new String('~', 10));
            Console.WriteLine("Repo/Room:DeleteAllRoom()");
            var list = await _context.Rooms.ToListAsync();
            _context.RemoveRange(list);
        }

        public async Task<PagedList<RoomDto>> GetAllRoomAsync(RoomParams roomParams)
        {
            Console.WriteLine("\t\t\t" + new String('~', 10));
            Console.WriteLine("Repo/Room:GetAllRoomAsync(RoomParams)");
            var list = _context.Rooms.AsQueryable();
            //using AutoMapper.QueryableExtensions; list.ProjectTo<RoomDto>
            return await PagedList<RoomDto>.CreateAsync(list.ProjectTo<RoomDto>(_mapper.ConfigurationProvider).AsNoTracking(), roomParams.PageNumber, roomParams.PageSize);
        }

        public async Task UpdateCountMember(int roomId, int count)
        {
            Console.WriteLine("\t\t\t" + new String('~', 10));
            Console.WriteLine("Repo/Room:UpdateCountMember(id, count)");
            var room = await _context.Rooms.FindAsync(roomId);
            if(room != null)
            {
                room.CountMember = count;
            }
        }
    }
}
