using MeetingAppCore.Dtos;
using MeetingAppCore.Entities;
using MeetingAppCore.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingAppCore.Interfaces
{
    public interface IMeetingRepository
    {
        Task<Meeting> GetMeetingByIdSignalr(int roomId);
        Task<Meeting> GetMeetingForConnectionSignalr(string connectionId);
        void EndConnectionSignalr(Connection connection);
        void AddMeeting(Meeting room);
        Task<Meeting> DeleteRoom(int id);
        Task<Meeting> EditRoom(int id, string newName);
        Task DeleteAllRoom();
        Task<PagedList<MeetingDto>> GetAllRoomAsync(RoomParams roomParams);
        Task<MeetingDto> GetRoomDtoById(int roomId);
        Task UpdateCountMemberSignalr(int roomId, int count);
    }
}
