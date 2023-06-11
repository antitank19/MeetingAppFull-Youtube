using MeetingAppCore.DebugTracker;
using MeetingAppCore.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingAppCore.SignalR
{
    public class ShareScreenTracker
    {
        // chứa xem user ở meeting nào đang shareScreen
        private static readonly List<UserConnectionSignalrDto> usersSharingScreen = new List<UserConnectionSignalrDto>();

        //Add user ở meeting nào đang shareScreen
        public Task<bool> AddUserSharingScreen(UserConnectionSignalrDto userMeetConnection)
        {
            //Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("4.         Tracker/ShareScreen: UserConnectedToShareScreen(UserConnectionDto)");
            FunctionTracker.Instance().AddTrackerFunc("Tracker/ShareScreen: UserConnectedToShareScreen(UserConnectionDto)");
            bool isOnline = false;
            lock (usersSharingScreen)
            {
                UserConnectionSignalrDto exsited = usersSharingScreen.FirstOrDefault(x => x.UserName == userMeetConnection.UserName && x.RoomId == userMeetConnection.RoomId);

                if (exsited == null)//chua co online
                {
                    usersSharingScreen.Add(userMeetConnection);
                    isOnline = true;
                }
            }
            return Task.FromResult(isOnline);
        }

        public Task<bool> RemoveUserShareScreen(UserConnectionSignalrDto userMeetConnection)
        {
            //Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("4.         Tracker/ShareScreen: UserDisconnectedShareScreen(UserConnectionDto)");
            FunctionTracker.Instance().AddTrackerFunc("Tracker/ShareScreen: UserDisconnectedShareScreen(UserConnectionDto)");
            bool isOffline = false;
            lock (usersSharingScreen)
            {
                var temp = usersSharingScreen.FirstOrDefault(x => x.UserName == userMeetConnection.UserName && x.RoomId == userMeetConnection.RoomId);
                if (temp == null)
                    return Task.FromResult(isOffline);
                else
                {
                    usersSharingScreen.Remove(temp);
                    isOffline = true;
                }
            }
            return Task.FromResult(isOffline);
        }

        public Task<bool> RemoveUserShareScreen(string username, int meetingId)
        {
            //Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("4.         Tracker/ShareScreen: DisconnectedByUser(username, roomId)");
            FunctionTracker.Instance().AddTrackerFunc("Tracker/ShareScreen: DisconnectedByUser(username, roomId)");
            bool isOffline = false;
            lock (usersSharingScreen)
            {
                var temp = usersSharingScreen.FirstOrDefault(x => x.UserName == username && x.RoomId == meetingId);
                if(temp != null)
                {
                    isOffline = true;
                    usersSharingScreen.Remove(temp);
                }
            }
            return Task.FromResult(isOffline);
        }

        public Task<UserConnectionSignalrDto> GetUserIsSharingScreenForMeeting(int meetingId)
        {
            //Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("4.         Tracker/ShareScreen: GetUserIsSharing(roomId)");
            FunctionTracker.Instance().AddTrackerFunc("Tracker/ShareScreen: GetUserIsSharing(roomId)");
            UserConnectionSignalrDto temp = null;
            lock (usersSharingScreen)
            {
                temp = usersSharingScreen.FirstOrDefault(x => x.RoomId == meetingId);                               
            }
            return Task.FromResult(temp);
        }
    }
}
