using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingAppCore.Dtos
{
    public class UserConnectionSignalrDto
    {
        public UserConnectionSignalrDto() { }
        public UserConnectionSignalrDto(string userName, int roomId)
        {
            UserName = userName;
            RoomId = roomId;
        }
        public string UserName { get; set; }
        public int RoomId { get; set; }
    }
}
