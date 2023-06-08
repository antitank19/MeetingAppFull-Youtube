using AutoMapper;
using MeetingAppCore.DebugTracker;
using MeetingAppCore.Dtos;
using MeetingAppCore.Entities;
using MeetingAppCore.Extensions;
using MeetingAppCore.Interfaces;
using Microsoft.AspNetCore.Authorization;
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
        public static string userOnlineInGroupMsg = "UserOnlineInGroup";
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
            var httpContext = Context.GetHttpContext();
            var meetingIdString = httpContext.Request.Query["roomId"].ToString();
            var meetingIdInt = int.Parse(meetingIdString);
            var username = Context.User.GetUsername();            

            await presenceTracker.UserConnected(new UserConnectionDto(username, meetingIdInt), Context.ConnectionId);

            await Groups.AddToGroupAsync(Context.ConnectionId, meetingIdString);//khi user click vao room se join vao
            await AddConnectionToGroup(meetingIdInt); // luu db DbSet<Connection> de khi disconnect biet

            //var usersOnline = await _unitOfWork.UserRepository.GetUsersOnlineAsync(currentUsers);
            MemberDto currentUserDto = await unitOfWork.UserRepository.GetMemberAsync(username);
            await Clients.Group(meetingIdString).SendAsync(userOnlineInGroupMsg, currentUserDto);
            Console.WriteLine("2.1   " + new String('+', 50));
            Console.WriteLine("2.1   Hub/Chat: OnConnectedAsync()");
            FunctionTracker.Instance().AddHubFunc("Hub/Presence: Clients.AllExcept(currentConnections).SendAsync(\"CountMemberInGroup\"");

            UserConnectionDto[] currentUsers = await presenceTracker.GetOnlineUsersInRoom(meetingIdInt);
            await unitOfWork.MeetingRepository.UpdateCountMember(meetingIdInt, currentUsers.Length);
            await unitOfWork.Complete();

            List<string> currentConnectionIds = await presenceTracker.GetConnectionIdsForUser(new UserConnectionDto(username, meetingIdInt));
            Console.WriteLine("2.1   " + new String('+', 50));
            Console.WriteLine("2.1   Hub/Chat: Clients.AllExcept(currentConnections).SendAsync(\"CountMemberInGroup\")");
            FunctionTracker.Instance().AddHubFunc("Hub/Presence: Clients.AllExcept(currentConnections).SendAsync(\"CountMemberInGroup\")");
            await groupHub.Clients.AllExcept(currentConnectionIds).SendAsync(GroupHub.CountMemberInGroupMsg,
                   new { roomId = meetingIdInt, countMember = currentUsers.Length });

            //share screen cho user vao sau cung
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
            var username = Context.User.GetUsername();
            var meeting = await RemoveConnectionFromGroup();
            var isOffline = await presenceTracker.UserDisconnected(new UserConnectionDto(username, meeting.RoomId), Context.ConnectionId);

            await shareScreenTracker.DisconnectedByUser(username, meeting.RoomId);
            if (isOffline)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, meeting.RoomId.ToString());
                MemberDto temp = await unitOfWork.UserRepository.GetMemberAsync(username);
                await Clients.Group(meeting.RoomId.ToString()).SendAsync("UserOfflineInGroup", temp);

                UserConnectionDto[] currentUsers = await presenceTracker.GetOnlineUsersInRoom(meeting.RoomId);

                await unitOfWork.MeetingRepository.UpdateCountMember(meeting.RoomId, currentUsers.Length);
                await unitOfWork.Complete();
               
                //await presenceHub.Clients.All.SendAsync("CountMemberInGroup",
                //       new { roomId = group.RoomId, countMember = currentUsers.Length });
                await groupHub.Clients.All.SendAsync(GroupHub.CountMemberInGroupMsg,
                       new { roomId = meeting.RoomId, countMember = currentUsers.Length });
            }
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
                await shareScreenTracker.UserConnectedToShareScreen(new UserConnectionDto(Context.User.GetUsername(), roomid));
                await Clients.Group(roomid.ToString()).SendAsync("OnUserIsSharing", Context.User.GetUsername());
            }
            else
            {
                await shareScreenTracker.UserDisconnectedShareScreen(new UserConnectionDto(Context.User.GetUsername(), roomid));
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

        private async Task<Meeting> RemoveConnectionFromGroup()
        {
            Console.WriteLine("2.   " + new String('+', 50));
            Console.WriteLine("2.   Hub/Chat: RemoveConnectionFromGroup()");
            FunctionTracker.Instance().AddHubFunc("Hub/Chat: RemoveConnectionFromGroup()");
            var group = await unitOfWork.MeetingRepository.GetMeetingForConnection(Context.ConnectionId);
            var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            unitOfWork.MeetingRepository.RemoveConnection(connection);

            if (await unitOfWork.Complete()) return group;

            throw new HubException("Fail to remove connection from room");
        }

        private async Task<Meeting> AddConnectionToGroup(int roomId)
        {
            Console.WriteLine("2.   " + new String('+', 50));
            Console.WriteLine("2.   Hub/Chat: AddConnectionToGroup(roomId)");
            FunctionTracker.Instance().AddHubFunc("Hub/Chat: AddConnectionToGroup(roomId)");
            var group = await unitOfWork.MeetingRepository.GetRoomById(roomId);
            var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());
            if (group != null)
            {
                group.Connections.Add(connection);
            }

            if (await unitOfWork.Complete()) return group;

            throw new HubException("Failed to add connection to room");
        }
    }
}
