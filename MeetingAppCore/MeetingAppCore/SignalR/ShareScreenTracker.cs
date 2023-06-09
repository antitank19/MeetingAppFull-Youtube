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
        private static readonly List<UserConnectionDto> usersSharingScreen = new List<UserConnectionDto>();

        //Add user ở meeting nào đang shareScreen
        public Task<bool> AddUserSharingScreen(UserConnectionDto user)
        {
            //Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("4.         Tracker/ShareScreen: UserConnectedToShareScreen(UserConnectionDto)");
            FunctionTracker.Instance().AddTrackerFunc("Tracker/ShareScreen: UserConnectedToShareScreen(UserConnectionDto)");
            bool isOnline = false;
            lock (usersSharingScreen)
            {
                UserConnectionDto temp = usersSharingScreen.FirstOrDefault(x => x.UserName == user.UserName && x.RoomId == user.RoomId);

                if (temp == null)//chua co online
                {
                    usersSharingScreen.Add(user);
                    isOnline = true;
                }
            }
            return Task.FromResult(isOnline);
        }

        public Task<bool> RemoveUserShareScreen(UserConnectionDto user)
        {
            //Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("4.         Tracker/ShareScreen: UserDisconnectedShareScreen(UserConnectionDto)");
            FunctionTracker.Instance().AddTrackerFunc("Tracker/ShareScreen: UserDisconnectedShareScreen(UserConnectionDto)");
            bool isOffline = false;
            lock (usersSharingScreen)
            {
                var temp = usersSharingScreen.FirstOrDefault(x => x.UserName == user.UserName && x.RoomId == user.RoomId);
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

        public Task<bool> DisconnectedByUser(string username, int roomId)
        {
            //Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("4.         Tracker/ShareScreen: DisconnectedByUser(username, roomId)");
            FunctionTracker.Instance().AddTrackerFunc("Tracker/ShareScreen: DisconnectedByUser(username, roomId)");
            bool isOffline = false;
            lock (usersSharingScreen)
            {
                var temp = usersSharingScreen.FirstOrDefault(x => x.UserName == username && x.RoomId == roomId);
                if(temp != null)
                {
                    isOffline = true;
                    usersSharingScreen.Remove(temp);
                }
            }
            return Task.FromResult(isOffline);
        }

        public Task<UserConnectionDto> GetUserIsSharingScreenForMeeting(int roomId)
        {
            //Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("4.         Tracker/ShareScreen: GetUserIsSharing(roomId)");
            FunctionTracker.Instance().AddTrackerFunc("Tracker/ShareScreen: GetUserIsSharing(roomId)");
            UserConnectionDto temp = null;
            lock (usersSharingScreen)
            {
                temp = usersSharingScreen.FirstOrDefault(x => x.RoomId == roomId);                               
            }
            return Task.FromResult(temp);
        }
    }
}
