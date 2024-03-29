﻿using MeetingAppCore.DebugTracker;
using MeetingAppCore.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingAppCore.SignalR
{
    public class ShareScreenTracker
    {
        public ShareScreenTracker()
        {
            Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("4.         Tracker/ShareScreen: ctor()");

        }
        private static readonly List<UserConnectionDto> usersShareScreen = new List<UserConnectionDto>();

        public Task<bool> UserConnectedToShareScreen(UserConnectionDto user)
        {
            Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("4.         Tracker/ShareScreen: UserConnectedToShareScreen(UserConnectionDto)");
            FunctionTracker.Instance().AddTrackerFunc("Tracker/ShareScreen: UserConnectedToShareScreen(UserConnectionDto)");
            bool isOnline = false;
            lock (usersShareScreen)
            {
                var temp = usersShareScreen.FirstOrDefault(x => x.UserName == user.UserName && x.RoomId == user.RoomId);

                if (temp == null)//chua co online
                {
                    usersShareScreen.Add(user);
                    isOnline = true;
                }
            }
            return Task.FromResult(isOnline);
        }

        public Task<bool> UserDisconnectedShareScreen(UserConnectionDto user)
        {
            Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("4.         Tracker/ShareScreen: UserDisconnectedShareScreen(UserConnectionDto)");
            FunctionTracker.Instance().AddTrackerFunc("Tracker/ShareScreen: UserDisconnectedShareScreen(UserConnectionDto)");
            bool isOffline = false;
            lock (usersShareScreen)
            {
                var temp = usersShareScreen.FirstOrDefault(x => x.UserName == user.UserName && x.RoomId == user.RoomId);
                if (temp == null)
                    return Task.FromResult(isOffline);
                else
                {
                    usersShareScreen.Remove(temp);
                    isOffline = true;
                }
            }
            return Task.FromResult(isOffline);
        }

        public Task<UserConnectionDto> GetUserIsSharing(int roomId)
        {
            Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("4.         Tracker/ShareScreen: GetUserIsSharing(roomId)");
            FunctionTracker.Instance().AddTrackerFunc("Tracker/ShareScreen: GetUserIsSharing(roomId)");
            UserConnectionDto temp = null;
            lock (usersShareScreen)
            {
                temp = usersShareScreen.FirstOrDefault(x => x.RoomId == roomId);                               
            }
            return Task.FromResult(temp);
        }

        public Task<bool> DisconnectedByUser(string username, int roomId)
        {
            Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("4.         Tracker/ShareScreen: DisconnectedByUser(username, roomId)");
            FunctionTracker.Instance().AddTrackerFunc("Tracker/ShareScreen: DisconnectedByUser(username, roomId)");
            bool isOffline = false;
            lock (usersShareScreen)
            {
                var temp = usersShareScreen.FirstOrDefault(x => x.UserName == username && x.RoomId == roomId);
                if(temp != null)
                {
                    isOffline = true;
                    usersShareScreen.Remove(temp);
                }
            }
            return Task.FromResult(isOffline);
        }
    }
}
