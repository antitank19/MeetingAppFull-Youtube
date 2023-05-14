using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingAppCore.Dtos
{
    public class UserConnectionDto
    {
        public UserConnectionDto() { }
        public UserConnectionDto(string userName, int roomId)
        {
            UserName = userName;
            RoomId = roomId;
        }
        public string UserName { get; set; }
        public int RoomId { get; set; }
    }
}
