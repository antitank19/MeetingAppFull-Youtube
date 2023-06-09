using AutoMapper;
using MeetingAppCore.DebugTracker;
using MeetingAppCore.Dtos;
using MeetingAppCore.Entities;
using MeetingAppCore.Extensions;
using MeetingAppCore.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingAppCore.SignalR
{
    [Authorize]
    public class MeetingHub : Hub
    {
        //Thông báo có người mới vào meeting
        // SendAsync(UserOnlineInGroupMsg, MemberSignalrDto)
        public static string UserOnlineInGroupMsg => "UserOnlineInGroup";
        public static string OnMuteCameraMsg = "OnMuteCamera";


        //IMapper _mapper;
        IHubContext<GroupHub> groupHub;
        PresenceTracker presenceTracker;
        IUnitOfWork unitOfWork;
        ShareScreenTracker shareScreenTracker;

        public MeetingHub(IUnitOfWork unitOfWork, ShareScreenTracker shareScreenTracker, PresenceTracker presenceTracker, IHubContext<GroupHub> presenceHubContext)
        {
            //_mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.presenceTracker = presenceTracker;
            this.groupHub = presenceHubContext;
            this.shareScreenTracker = shareScreenTracker;
        }


        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("2.   " + new String('+', 50));
            Console.WriteLine("2.   Hub/Chat: OnConnectedAsync()");
            FunctionTracker.Instance().AddHubFunc("Hub/Chat: OnConnectedAsync()");
            //Step 1: Lấy meeting Id và username
            HttpContext httpContext = Context.GetHttpContext();
            string meetingIdString = httpContext.Request.Query["roomId"].ToString();
            int meetingIdInt = int.Parse(meetingIdString);
            string username = Context.User.GetUsername();
            //Step 2: Add ContextConnection vào MeetingHub.Group(meetingId) và add (user, meeting) vào presenceTracker
            await presenceTracker.UserConnected(new UserConnectionDto(username, meetingIdInt), Context.ConnectionId);

            await Groups.AddToGroupAsync(Context.ConnectionId, meetingIdString);//khi user click vao room se join vao
            //await AddConnectionToGroup(meetingIdInt); // luu db DbSet<Connection> de khi disconnect biet
            //Step 3: Tạo Connect để lưu vào DB, ConnectionId
            #region lưu Db Connection
            Meeting meeting = await unitOfWork.MeetingRepository.GetRoomById(meetingIdInt);
            Connection connection = new Connection(Context.ConnectionId, Context.User.GetUsername());
            if (meeting != null)
            {
                meeting.Connections.Add(connection);
            }

            if (await unitOfWork.Complete()) { }
            else
            {
                throw new HubException("Failed to add connection to room");
            }
            #endregion

            //var usersOnline = await _unitOfWork.UserRepository.GetUsersOnlineAsync(currentUsers);
            //Step 4: Thông báo với meetHub.Group(meetingId) là mày đã online  SendAsync(UserOnlineInGroupMsg, MemberSignalrDto)
            MemberSignalrDto currentUserDto = await unitOfWork.UserRepository.GetMemberAsync(username);
            await Clients.Group(meetingIdString).SendAsync(UserOnlineInGroupMsg, currentUserDto);
            Console.WriteLine("2.1     " + new String('+', 50));
            Console.WriteLine("2.1     Hub/ChatSend: UserOnlineInGroupMsg, MemberSignalrDto");
            FunctionTracker.Instance().AddHubFunc("Hub/ChatSend: UserOnlineInGroupMsg, MemberSignalrDto");

            //Step 5: Update số người trong meeting lên db
            UserConnectionDto[] currentUsersInMeeting = await presenceTracker.GetOnlineUsersInRoom(meetingIdInt);
            await unitOfWork.MeetingRepository.UpdateCountMember(meetingIdInt, currentUsersInMeeting.Length);
            await unitOfWork.Complete();

            // Step 6: Thông báo với groupHub.Group(groupId) số người ở trong phòng  
            List<string> currentConnectionIds = await presenceTracker.GetConnectionIdsForUser(new UserConnectionDto(username, meetingIdInt));
            Console.WriteLine("2.1     " + new String('+', 50));
            Console.WriteLine("2.1     Hub/PresenceSend: CountMemberInGroupMsg, { meetingId, countMember }");
            FunctionTracker.Instance().AddHubFunc("Hub/PresenceSend: CountMemberInGroupMsg, { meetingId, countMember }");
            await groupHub.Clients.AllExcept(currentConnectionIds).SendAsync(GroupHub.CountMemberInGroupMsg,
                   new { meetingId = meetingIdInt, countMember = currentUsersInMeeting.Length });
           //come back
            //share screen cho user vao sau cung

            //step 7: Thông báo shareScreen cho user vào cuối 
            UserConnectionDto userIsSharing = await shareScreenTracker.GetUserIsSharingScreenForMeeting(meetingIdInt);
            if(userIsSharing != null)
            {
                List<string> currentBeginConnectionsUser = await presenceTracker.GetConnectionIdsForUser(userIsSharing);
                if(currentBeginConnectionsUser.Count > 0)
                    await Clients.Clients(currentBeginConnectionsUser).SendAsync("OnShareScreenLastUser", new { usernameTo = username, isShare = true });
                await Clients.Caller.SendAsync("OnUserIsSharing", userIsSharing.UserName);
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            Console.WriteLine("2.   " + new String('+', 50));
            Console.WriteLine("2.   Hub/Chat: OnDisconnectedAsync(Exception)");
            FunctionTracker.Instance().AddHubFunc("Hub/Chat: OnDisconnectedAsync(Exception)");
            //step 1: Lấy username 
            string username = Context.User.GetUsername();
            //step 2: Xóa connection trong db và lấy meeting
            Meeting meeting = await RemoveConnectionFromMeeting();
            //step 3: Xóa ContextConnectionId khỏi presenceTracker và check xem user còn connect nào khác với meeting ko
            bool isOffline = await presenceTracker.UserDisconnected(new UserConnectionDto(username, meeting.RoomId), Context.ConnectionId);

            //step 4: Remove khỏi shareScreenTracker nếu có
            await shareScreenTracker.DisconnectedByUser(username, meeting.RoomId);

            //step 5: Remove ContextConnectionId khỏi meetingHub.Group(meetingId)   chắc move ra khỏi if
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, meeting.RoomId.ToString());
            if (isOffline)
            {
                ////step 5: Nếu ko còn connect nào nữa thì remove ContextConnectionId khỏi meetingHub.Group(meetingId)   chắc move ra khỏi if
                //await Groups.RemoveFromGroupAsync(Context.ConnectionId, meeting.RoomId.ToString());

                //step 6: Nếu ko còn connect nào nữa thì Thông báo với meetingHub.Group(meetingId)
                MemberSignalrDto temp = await unitOfWork.UserRepository.GetMemberAsync(username);
                await Clients.Group(meeting.RoomId.ToString()).SendAsync("UserOfflineInGroup", temp);

                //step 7: Update lại số người trong phòng
                UserConnectionDto[] currentUsers = await presenceTracker.GetOnlineUsersInRoom(meeting.RoomId);
                await unitOfWork.MeetingRepository.UpdateCountMember(meeting.RoomId, currentUsers.Length);
                await unitOfWork.Complete();

                //await presenceHub.Clients.All.SendAsync("CountMemberInGroup",
                //       new { roomId = group.RoomId, countMember = currentUsers.Length });
                
                //step 8: Thông báo với groupHub.Group(groupId) số người ở trong phòng
                //comeback
                await groupHub.Clients.All.SendAsync(GroupHub.CountMemberInGroupMsg,
                       new { meetingId = meeting.RoomId, countMember = currentUsers.Length });
            }
            //step 9: Disconnect khỏi meetHub
            await base.OnDisconnectedAsync(exception);
        }
        //
        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            Console.WriteLine("2.   " + new String('+', 50));
            Console.WriteLine("2.   Hub/Chat: SendMessage(CreateMessageDto)");
            FunctionTracker.Instance().AddHubFunc("Hub/Chat: SendMessage(CreateMessageDto)");
            string userName = Context.User.GetUsername();
            AppUser sender = await unitOfWork.UserRepository.GetUserByUsernameAsync(userName);

            Meeting group = await unitOfWork.MeetingRepository.GetMeetingForConnection(Context.ConnectionId);

            if(group != null)
            {
                var message = new MessageDto
                {
                    SenderUsername = userName,
                    SenderDisplayName = sender.DisplayName,
                    Content = createMessageDto.Content,
                    MessageSent = DateTime.Now
                };
                //Luu message vao db
                //code here
                //send meaasge to group
                await Clients.Group(group.RoomId.ToString()).SendAsync("NewMessage", message);
            }
        }
        // chatHubConnection.invoke('MuteMicro', mute)
        public async Task MuteMicro(bool muteMicro)
        {
            Console.WriteLine("2.   " + new String('+', 50));
            Console.WriteLine("2.   Hub/Chat: MuteMicro(bool)");
            FunctionTracker.Instance().AddHubFunc("Hub/Chat: MuteMicro(bool)");
            var group = await unitOfWork.MeetingRepository.GetMeetingForConnection(Context.ConnectionId);
            if (group != null)
            {
                await Clients.Group(group.RoomId.ToString()).SendAsync("OnMuteMicro", new { username = Context.User.GetUsername(), mute = muteMicro });
            }
            else
            {
                throw new HubException("group == null");
            }
        }

        //chatHubConnection.invoke('MuteCamera', mute)
        public async Task MuteCamera(bool muteCamera)
        {
            Console.WriteLine("2.   " + new String('+', 50));
            Console.WriteLine("2.   Hub/Chat: MuteCamera(bool)");
            FunctionTracker.Instance().AddHubFunc("Hub/Chat: MuteCamera(bool)");
            var group = await unitOfWork.MeetingRepository.GetMeetingForConnection(Context.ConnectionId);
            if(group != null)
            {
                await Clients.Group(group.RoomId.ToString()).SendAsync(OnMuteCameraMsg, new { username = Context.User.GetUsername(), mute = muteCamera });
            }
            else
            {
                throw new HubException("group == null");
            } 
        }

        //sẽ dc gọi khi FE gọi chatHubConnection.invoke('ShareScreen', roomId, isShareScreen)
        public async Task ShareScreen(int roomid, bool isShareScreen)
        {
            Console.WriteLine("2.   " + new String('+', 50));
            Console.WriteLine("2.   Hub/Chat: ShareScreen(id, bool)");
            FunctionTracker.Instance().AddHubFunc("Hub/Chat: ShareScreen(id, bool)");
            if (isShareScreen)//true is doing share
            {
                await shareScreenTracker.AddUserSharingScreen(new UserConnectionDto(Context.User.GetUsername(), roomid));
                await Clients.Group(roomid.ToString()).SendAsync("OnUserIsSharing", Context.User.GetUsername());
            }
            else
            {
                await shareScreenTracker.RemoveUserShareScreen(new UserConnectionDto(Context.User.GetUsername(), roomid));
            }
            await Clients.Group(roomid.ToString()).SendAsync("OnShareScreen", isShareScreen);
            //var group = await _unitOfWork.RoomRepository.GetRoomForConnection(Context.ConnectionId);
        }
        //sẽ dc gọi khi FE gọi chatHubConnection.invoke('ShareScreenToUser', roomId, username, isShareScreen)
        public async Task ShareScreenToUser(int roomid, string username, bool isShare)
        {
            Console.WriteLine("2.   " + new String('+', 50));
            Console.WriteLine("2.   Hub/Chat: ShareScreenToUser(id, username, bool)");
            FunctionTracker.Instance().AddHubFunc("Hub/Chat: ShareScreenToUser(id, username, bool)");
            var currentBeginConnectionsUser = await presenceTracker.GetConnectionIdsForUser(new UserConnectionDto(username, roomid));
            if(currentBeginConnectionsUser.Count > 0)
                await Clients.Clients(currentBeginConnectionsUser).SendAsync("OnShareScreen", isShare);
        }

        private async Task<Meeting> RemoveConnectionFromMeeting()
        {
            Console.WriteLine("2.   " + new String('+', 50));
            Console.WriteLine("2.   Hub/Chat: RemoveConnectionFromMeeting()");
            FunctionTracker.Instance().AddHubFunc("Hub/Chat: RemoveConnectionFromMeeting()");
            var meeting = await unitOfWork.MeetingRepository.GetMeetingForConnection(Context.ConnectionId);
            var connection = meeting.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            unitOfWork.MeetingRepository.RemoveConnection(connection);

            if (await unitOfWork.Complete()) return meeting;

            throw new HubException("Fail to remove connection from room");
        }

        //private async Task<Meeting> AddConnectionToGroup(int roomId)
        //{
        //    Console.WriteLine("2.   " + new String('+', 50));
        //    Console.WriteLine("2.   Hub/Chat: AddConnectionToGroup(roomId)");
        //    FunctionTracker.Instance().AddHubFunc("Hub/Chat: AddConnectionToGroup(roomId)");
        //    Meeting meeting = await unitOfWork.MeetingRepository.GetRoomById(roomId);
        //    Connection connection = new Connection(Context.ConnectionId, Context.User.GetUsername());
        //    if (meeting != null)
        //    {
        //        meeting.Connections.Add(connection);
        //    }

        //    if (await unitOfWork.Complete()) return meeting;

        //    throw new HubException("Failed to add connection to room");
        //}
    }
}
