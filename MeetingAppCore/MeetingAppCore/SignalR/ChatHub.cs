using AutoMapper;
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
    public class ChatHub : Hub
    {
        //IMapper _mapper;
        IHubContext<PresenceHub> presenceHubContext;
        PresenceTracker presenceTracker;
        IUnitOfWork unitOfWork;
        ShareScreenTracker shareScreenTracker;

        public ChatHub(IUnitOfWork unitOfWork, ShareScreenTracker shareScreenTracker, PresenceTracker presenceTracker, IHubContext<PresenceHub> presenceHubContext)
        {
            Console.WriteLine("2.   " + new String('+', 10));
            Console.WriteLine("Hub/Chat: ctor(IUnitOfWork, UserShareScreenTracker, PresenceTracker, PresenceHub)");

            //_mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.presenceTracker = presenceTracker;
            this.presenceHubContext = presenceHubContext;
            this.shareScreenTracker = shareScreenTracker;
        }

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("2.   " + new String('+', 10));
            Console.WriteLine("Hub/Chat: OnConnectedAsync()");
            var httpContext = Context.GetHttpContext();
            var roomId = httpContext.Request.Query["roomId"].ToString();
            var roomIdInt = int.Parse(roomId);
            var username = Context.User.GetUsername();            

            await presenceTracker.UserConnected(new UserConnectionDto(username, roomIdInt), Context.ConnectionId);

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);//khi user click vao room se join vao
            await AddConnectionToGroup(roomIdInt); // luu db DbSet<Connection> de khi disconnect biet
            
            //var usersOnline = await _unitOfWork.UserRepository.GetUsersOnlineAsync(currentUsers);
            var oneUserOnline = await unitOfWork.UserRepository.GetMemberAsync(username);
            await Clients.Group(roomId).SendAsync("UserOnlineInGroup", oneUserOnline);
            
            var currentUsers = await presenceTracker.GetOnlineUsers(roomIdInt);
            await unitOfWork.RoomRepository.UpdateCountMember(roomIdInt, currentUsers.Length);
            await unitOfWork.Complete();
            
            var currentConnections = await presenceTracker.GetConnectionsForUser(new UserConnectionDto(username, roomIdInt));
            await presenceHubContext.Clients.AllExcept(currentConnections).SendAsync("CountMemberInGroup",
                   new { roomId = roomIdInt, countMember = currentUsers.Length });

            //share screen user vao sau cung
            var userIsSharing = await shareScreenTracker.GetUserIsSharing(roomIdInt);
            if(userIsSharing != null)
            {
                var currentBeginConnectionsUser = await presenceTracker.GetConnectionsForUser(userIsSharing);
                if(currentBeginConnectionsUser.Count > 0)
                    await Clients.Clients(currentBeginConnectionsUser).SendAsync("OnShareScreenLastUser", new { usernameTo = username, isShare = true });
                await Clients.Caller.SendAsync("OnUserIsSharing", userIsSharing.UserName);
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            Console.WriteLine("2.   " + new String('+', 10));
            Console.WriteLine("Hub/Chat: OnDisconnectedAsync(Exception)");
            var username = Context.User.GetUsername();
            var group = await RemoveConnectionFromGroup();
            var isOffline = await presenceTracker.UserDisconnected(new UserConnectionDto(username, group.RoomId), Context.ConnectionId);

            await shareScreenTracker.DisconnectedByUser(username, group.RoomId);
            if (isOffline)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, group.RoomId.ToString());
                var temp = await unitOfWork.UserRepository.GetMemberAsync(username);
                await Clients.Group(group.RoomId.ToString()).SendAsync("UserOfflineInGroup", temp);

                var currentUsers = await presenceTracker.GetOnlineUsers(group.RoomId);

                await unitOfWork.RoomRepository.UpdateCountMember(group.RoomId, currentUsers.Length);
                await unitOfWork.Complete();
               
                await presenceHubContext.Clients.All.SendAsync("CountMemberInGroup",
                       new { roomId = group.RoomId, countMember = currentUsers.Length });
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            Console.WriteLine("2.   " + new String('+', 10));
            Console.WriteLine("Hub/Chat: SendMessage(CreateMessageDto)");
            var userName = Context.User.GetUsername();
            var sender = await unitOfWork.UserRepository.GetUserByUsernameAsync(userName);
            
            var group = await unitOfWork.RoomRepository.GetRoomForConnection(Context.ConnectionId);

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

        public async Task MuteMicro(bool muteMicro)
        {
            Console.WriteLine("2.   " + new String('+', 10));
            Console.WriteLine("Hub/Chat: MuteMicro(bool)");
            var group = await unitOfWork.RoomRepository.GetRoomForConnection(Context.ConnectionId);
            if (group != null)
            {
                await Clients.Group(group.RoomId.ToString()).SendAsync("OnMuteMicro", new { username = Context.User.GetUsername(), mute = muteMicro });
            }
            else
            {
                throw new HubException("group == null");
            }
        }

        public async Task MuteCamera(bool muteCamera)
        {
            Console.WriteLine("2.   " + new String('+', 10));
            Console.WriteLine("Hub/Chat: MuteCamera(bool)");
            var group = await unitOfWork.RoomRepository.GetRoomForConnection(Context.ConnectionId);
            if(group != null)
            {
                await Clients.Group(group.RoomId.ToString()).SendAsync("OnMuteCamera", new { username = Context.User.GetUsername(), mute = muteCamera });
            }
            else
            {
                throw new HubException("group == null");
            } 
        }

        public async Task ShareScreen(int roomid, bool isShareScreen)
        {
            Console.WriteLine("2.   " + new String('+', 10));
            Console.WriteLine("Hub/Chat: ShareScreen(id, bool)");
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

        public async Task ShareScreenToUser(int roomid, string username, bool isShare)
        {
            Console.WriteLine("2.   " + new String('+', 10));
            Console.WriteLine("Hub/Chat: ShareScreenToUser(id, username, bool)");
            var currentBeginConnectionsUser = await presenceTracker.GetConnectionsForUser(new UserConnectionDto(username, roomid));
            if(currentBeginConnectionsUser.Count > 0)
                await Clients.Clients(currentBeginConnectionsUser).SendAsync("OnShareScreen", isShare);
        }

        private async Task<Room> RemoveConnectionFromGroup()
        {
            Console.WriteLine("2.   " + new String('+', 10));
            Console.WriteLine("Hub/Chat: RemoveConnectionFromGroup()");
            var group = await unitOfWork.RoomRepository.GetRoomForConnection(Context.ConnectionId);
            var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            unitOfWork.RoomRepository.RemoveConnection(connection);

            if (await unitOfWork.Complete()) return group;

            throw new HubException("Fail to remove connection from room");
        }

        private async Task<Room> AddConnectionToGroup(int roomId)
        {
            Console.WriteLine("2.   " + new String('+', 10));
            Console.WriteLine("Hub/Chat: AddConnectionToGroup()");
            var group = await unitOfWork.RoomRepository.GetRoomById(roomId);
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
