using MeetingAppCore.Dtos;
using MeetingAppCore.Entities;
using MeetingAppCore.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingAppCore.Interfaces
{
    public interface IRoomRepository
    {
        Task<Meeting> GetRoomById(int roomId);
        Task<Meeting> GetMeetingForConnection(string connectionId);
        void RemoveConnection(Connection connection);
        void AddRoom(Meeting room);
        Task<Meeting> DeleteRoom(int id);
        Task<Meeting> EditRoom(int id, string newName);
        Task DeleteAllRoom();
        Task<PagedList<MeetingDto>> GetAllRoomAsync(RoomParams roomParams);
        Task<MeetingDto> GetRoomDtoById(int roomId);
        Task UpdateCountMember(int roomId, int count);
    }
}
