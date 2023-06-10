using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingAppCore.Dtos
{
    public class MessageSignalrGetDto
    {
        public string SenderDisplayName { get; set; }
        public string SenderUsername { get; set; }
        public string Content { get; set; }
        public DateTime MessageSent { get; set; }
    }
}
