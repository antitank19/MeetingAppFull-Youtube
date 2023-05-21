using MeetingAppCore.DebugTracker;
using MeetingAppCore.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingAppCore.SignalR
{
    public class PresenceTracker
    {
        public PresenceTracker()
        {
            Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("4.         Tracker/Presence:ctor()");
        }
        private static readonly Dictionary<UserConnectionDto, List<string>> OnlineUsers = new Dictionary<UserConnectionDto, List<string>>();

        public Task<bool> UserConnected(UserConnectionDto user, string connectionId)
        {
            Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("4.         Tracker/Presence: UserConnected(UserConnectionDto, connectionId)");
            FunctionTracker.Instance().AddTrackerFunc("Tracker/Presence: UserConnected(UserConnectionDto, connectionId)");
            bool isOnline = false;
            lock (OnlineUsers)
            {
                var temp = OnlineUsers.FirstOrDefault(x => x.Key.UserName == user.UserName && x.Key.RoomId == user.RoomId);
                
                if(temp.Key == null)//chua co online
                {
                    OnlineUsers.Add(user, new List<string> { connectionId });
                    isOnline = true;
                }
                else if (OnlineUsers.ContainsKey(temp.Key))
                {
                    OnlineUsers[temp.Key].Add(connectionId);
                }
            }

            return Task.FromResult(isOnline);
        }

        public Task<bool> UserDisconnected(UserConnectionDto user, string connectionId)
        {
            Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("4.         Tracker/Presence: UserConnectionDto(UserConnectionDto, connectionId)");
            FunctionTracker.Instance().AddTrackerFunc("Tracker/Presence: UserConnectionDto(UserConnectionDto, connectionId)");
            bool isOffline = false;
            lock (OnlineUsers)
            {
                var temp = OnlineUsers.FirstOrDefault(x => x.Key.UserName == user.UserName && x.Key.RoomId == user.RoomId);
                if (temp.Key == null) 
                    return Task.FromResult(isOffline);

                OnlineUsers[temp.Key].Remove(connectionId);    
                if (OnlineUsers[temp.Key].Count == 0)
                {
                    OnlineUsers.Remove(temp.Key);
                    isOffline = true;
                }
            }

            return Task.FromResult(isOffline);
        }

        public Task<UserConnectionDto[]> GetOnlineUsers(int roomId)
        {
            Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("Tracker/GetOnlineUsers: GetOnlineUsers(roomId)");
            FunctionTracker.Instance().AddTrackerFunc("Tracker/Presence: GetOnlineUsers(roomId)");
            UserConnectionDto[] onlineUsers;
            lock (OnlineUsers)
            {
                onlineUsers = OnlineUsers.Where(u=>u.Key.RoomId == roomId).Select(k => k.Key).ToArray();
            }

            return Task.FromResult(onlineUsers);
        }

        public Task<List<string>> GetConnectionsForUser(UserConnectionDto user)
        {
            Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("4.         Tracker/Presence: GetConnectionsForUser(UserConnectionDto)");
            FunctionTracker.Instance().AddTrackerFunc("Tracker/Presence: GetConnectionsForUser(UserConnectionDto)");
            List<string> connectionIds = new List<string>();
            lock (OnlineUsers)
            {                
                var temp = OnlineUsers.SingleOrDefault(x => x.Key.UserName == user.UserName && x.Key.RoomId == user.RoomId);
                if(temp.Key != null)
                {
                    connectionIds = OnlineUsers.GetValueOrDefault(temp.Key);
                }       
            }
            return Task.FromResult(connectionIds);
        }
        
        public Task<List<string>> GetConnectionsForUsername(string username)
        {
            Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("4.         Tracker/Presence: GetConnectionsForUsername(username)");
            FunctionTracker.Instance().AddTrackerFunc("Tracker/Presence: GetConnectionsForUsername(username)");
            List<string> connectionIds = new List<string>();
            lock (OnlineUsers)
            {
                // 1 user co nhieu lan dang nhap
                var listTemp = OnlineUsers.Where(x => x.Key.UserName == username).ToList();
                if (listTemp.Count > 0)
                {
                    foreach(var user in listTemp)
                    {
                        connectionIds.AddRange(user.Value);
                    }
                }
            }
            return Task.FromResult(connectionIds);
        }
    }
}
