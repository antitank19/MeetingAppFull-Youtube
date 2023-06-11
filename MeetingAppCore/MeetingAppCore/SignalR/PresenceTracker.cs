using MeetingAppCore.DebugTracker;
using MeetingAppCore.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingAppCore.SignalR
{
    //
    public class PresenceTracker
    {
        //Key dạng UserConnectionDto chứa Username và MeetingId
        //Value chứa list các MeetingHub và GroupHub ContextConnectionId
        private static readonly Dictionary<UserConnectionSignalrDto, List<string>> OnlineUsers = new Dictionary<UserConnectionSignalrDto, List<string>>();

        /// <summary>
        /// Thêm connection cho người và meeting    <br/>
        /// Dc gọi bởi GroupHub và MeetHub OnConnnectedAsync    <br/>
        /// Nếu ko có key UserConnectionDto (chưa track dc Username và MeetingId) thì tạo Key mới và thêm ContextConnectionId   <br/>
        /// Nếu đã track rồi thì thêm ContextConnectionId    <br/>
        /// </summary>
        /// <param name="user"></param>
        /// <param name="connectionId"></param>
        /// <returns>(ko quan trọng) true nếu người đang connect vào meeting</returns>
        public Task<bool> UserConnected(UserConnectionSignalrDto user, string connectionId)
        {
            //Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("4.         Tracker/Presence: UserConnected(UserConnectionDto, connectionId)");
            FunctionTracker.Instance().AddTrackerFunc("Tracker/Presence: UserConnected(UserConnectionDto, connectionId)");
            bool isOnline = false;
            lock (OnlineUsers)
            {
                KeyValuePair<UserConnectionSignalrDto, List<string>> temp = OnlineUsers.FirstOrDefault(x => x.Key.UserName == user.UserName && x.Key.RoomId == user.RoomId);
                
                if(temp.Key == null)//chua co online
                {
                    OnlineUsers.Add(user, new List<string> { connectionId });
                    isOnline = true;
                }
                else if (OnlineUsers.ContainsKey(temp.Key))
                {
                    OnlineUsers[temp.Key].Add(connectionId);
                    isOnline = true;
                }
            }

            return Task.FromResult(isOnline);
        }
        /// <summary>
        /// Xóa connection cho người và check xem người đó còn on ko (còn connect vô phòng ko    <br/>
        /// Dc gọi bởi GroupHub và MeetHub OnDisconnectedAsync   <br/>
        /// Nếu ko có key cho username và meetingId thì trả là false (isOnline)?  (ngược lại mới đúng?)  <br/>
        /// Nếu có key thì tiếp     <br/>
        ///   Bỏ cái ContextConnectionId ra khỏi key     <br/>
        ///   Nếu key cho username và meetingId đã hết ContextConnectionId thì bỏ luôn Key và return true (isOffLine)  <br/>
        ///   Nếu key còn ContextConnectionId thì return false (isOnline)      <br/>
        /// </summary>
        /// <param name="user"></param>
        /// <param name="connectionId"></param>
        /// <returns>True nếu không còn HubConnection nào cho (username và meetingId) </returns>
        public Task<bool> UserDisconnected(UserConnectionSignalrDto user, string connectionId)
        {
            //Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("4.         Tracker/Presence: UserDisconnected(UserConnectionDto, connectionId)");
            FunctionTracker.Instance().AddTrackerFunc("Tracker/Presence: UserDisconnected(UserConnectionDto, connectionId)");
            bool isOffline = false;
            lock (OnlineUsers)
            {
                KeyValuePair<UserConnectionSignalrDto, List<string>> userMeetingValue = OnlineUsers.FirstOrDefault(x => x.Key.UserName == user.UserName && x.Key.RoomId == user.RoomId);
                if (userMeetingValue.Key == null)
                {
                    return Task.FromResult(isOffline);
                    //return Task.FromResult(!isOffline); //Nên là cái này mới đúng
                }

                OnlineUsers[userMeetingValue.Key].Remove(connectionId);    
                if (OnlineUsers[userMeetingValue.Key].Count == 0)
                {
                    OnlineUsers.Remove(userMeetingValue.Key);
                    isOffline = true;
                }
            }

            return Task.FromResult(isOffline);
        }

        /// <summary>
        /// Lấy danh sách UserConnectionDto những người trong meeting
        /// </summary>
        /// <param name="meetingId"></param>
        /// <returns></returns>
        public Task<UserConnectionSignalrDto[]> GetOnlineUsersInMeet(int meetingId)
        {
            //Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("Tracker/GetOnlineUsers: GetOnlineUsersInRoom(roomId)");
            FunctionTracker.Instance().AddTrackerFunc("Tracker/Presence: GetOnlineUsersInRoom(roomId)");
            UserConnectionSignalrDto[] userInRoom;
            lock (OnlineUsers)
            {
                userInRoom = OnlineUsers.Where(u=>u.Key.RoomId == meetingId).Select(k => k.Key).ToArray();
            }

            return Task.FromResult(userInRoom);
        }

        /// <summary>
        /// Lấy hết ContextConnection
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<List<string>> GetConnectionIdsForUser(UserConnectionSignalrDto user)
        {
            //Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("4.         Tracker/Presence: GetConnectionIdsForUser(UserConnectionDto)");
            FunctionTracker.Instance().AddTrackerFunc("Tracker/Presence: GetConnectionIdsForUser(UserConnectionDto)");
            List<string> connectionIds = new List<string>();
            lock (OnlineUsers)
            {                
                var valuePair = OnlineUsers.SingleOrDefault(x => x.Key.UserName == user.UserName && x.Key.RoomId == user.RoomId);
                if(valuePair.Key != null)
                {
                    connectionIds = OnlineUsers.GetValueOrDefault(valuePair.Key);
                }       
            }
            return Task.FromResult(connectionIds);
        }

        /// <summary>
        /// Không quan trọng  <br/>
        /// Được gọi bởi api lock user
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public Task<List<string>> GetConnectionIdsForUsername(string username)
        {
            //Console.WriteLine("4.         " + new String('~', 50));
            Console.WriteLine("4.         Tracker/Presence: GetConnectionIdsForUsername(username)");
            FunctionTracker.Instance().AddTrackerFunc("Tracker/Presence: GetConnectionIdsForUsername(username)");
            List<string> connectionIds = new List<string>();
            lock (OnlineUsers)
            {
                // 1 user co nhieu lan kết nối vào hub
                var listTemp = OnlineUsers.Where(x => x.Key.UserName == username).ToList();
                if (listTemp.Count > 0)
                {
                    foreach(KeyValuePair<UserConnectionSignalrDto, List<string>> userConnections in listTemp)
                    {
                        connectionIds.AddRange(userConnections.Value);
                    }
                }
            }
            return Task.FromResult(connectionIds);
        }
    }
}
