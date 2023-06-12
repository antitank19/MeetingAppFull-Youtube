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
        //BE SendAsync(UserOnlineInGroupMsg, MemberSignalrDto)
        public static string UserOnlineInMeetingMsg => "UserOnlineInMeeting";

        //BE SendAsync(OnMuteCameraMsg, new { username: String, mute: bool })
        public static string OnMuteCameraMsg => "OnMuteCamera";

        //SendAsync(OnMuteMicroMsg, new { username: String, mute: bool })
        public static string OnMuteMicroMsg => "OnMuteMicro";

        //Thông báo tới người đang share screen là có người mới, shareScreenHub share luôn cho người này
        //BE SendAsync(OnShareScreenLastUser, new { usernameTo: string, isShare: bool })
        public static string OnShareScreenLastUser => "OnShareScreenLastUser";

        //Thông báo người nào đang share screen
        //SendAsync(OnUserIsSharingMsg, screenSharerUsername: string);
        public static string OnUserIsSharingMsg => "OnUserIsSharing";

        //Thông báo có người rời meeting
        //BE SendAsync(UserOfflineInGroupMsg, offlineUser: MemberSignalrDto)
        public static string UserOfflineInMeetingMsg => "UserOfflineInMeeting";

        //Thông báo có Chat Message mới
        //BE SendAsync("NewMessage", MessageSignalrGetDto)
        public static string NewMessageMsg => "NewMessage";

        //BE SendAsync(OnShareScreenMsg, isShareScreen: bool)
        public static string OnShareScreenMsg => "OnShareScreen";



        //IMapper _mapper;
        IHubContext<GroupHub> groupHub;
        PresenceTracker presenceTracker;
        IRepoWrapper repos;
        ShareScreenTracker shareScreenTracker;

        public MeetingHub(IRepoWrapper repos, ShareScreenTracker shareScreenTracker, PresenceTracker presenceTracker, IHubContext<GroupHub> presenceHubContext)
        {
            //_mapper = mapper;
            this.repos = repos;
            this.presenceTracker = presenceTracker;
            this.groupHub = presenceHubContext;
            this.shareScreenTracker = shareScreenTracker;
        }

        //FE sẽ connect qua hàm này
        //FE gọi:
        //this.chatHubConnection = new HubConnectionBuilder()
        //    .withUrl(this.hubUrl + 'chathub?meetingId=' + roomId, {
        //        accessTokenFactory: () => user.token
        //    }).withAutomaticReconnect().build()
        //this.chatHubConnection.start().catch(err => console.log(err));
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("2.   " + new String('+', 50));
            Console.WriteLine("2.   Hub/Chat: OnConnectedAsync()");
            FunctionTracker.Instance().AddHubFunc("Hub/Chat: OnConnectedAsync()");
            //Step 1: Lấy meeting Id và username
            HttpContext httpContext = Context.GetHttpContext();
            string meetingIdString = httpContext.Request.Query["meetingId"].ToString();
            int meetingIdInt = int.Parse(meetingIdString);
            string username = Context.User.GetUsername();

            //Step 2: Add ContextConnection vào MeetingHub.Group(meetingId) và add (user, meeting) vào presenceTracker
            await presenceTracker.UserConnected(new UserConnectionSignalrDto(username, meetingIdInt), Context.ConnectionId);
            await Groups.AddToGroupAsync(Context.ConnectionId, meetingIdString);//khi user click vao room se join vao
            //await AddConnectionToGroup(meetingIdInt); // luu db DbSet<Connection> de khi disconnect biet
            
            //Step 3: Tạo Connect để lưu vào DB, ConnectionId
            #region lưu Db Connection
            Meeting meeting = await repos.Meetings.GetMeetingByIdSignalr(meetingIdInt);
            Connection connection = new Connection(Context.ConnectionId, Context.User.GetUsername());
            if (meeting != null)
            {
                meeting.Connections.Add(connection);
            }

            if (await repos.Complete()) { }
            else
            {
                throw new HubException("Failed to add connection to room");
            }
            #endregion

            //var usersOnline = await _unitOfWork.UserRepository.GetUsersOnlineAsync(currentUsers);
            //Step 4: Thông báo với meetHub.Group(meetingId) là mày đã online  SendAsync(UserOnlineInGroupMsg, MemberSignalrDto)
            MemberSignalrDto currentUserDto = await repos.Accounts.GetMemberSignalrAsync(username);
            await Clients.Group(meetingIdString).SendAsync(UserOnlineInMeetingMsg, currentUserDto);
            Console.WriteLine("2.1     " + new String('+', 50));
            Console.WriteLine("2.1     Hub/ChatSend: UserOnlineInGroupMsg, MemberSignalrDto");
            FunctionTracker.Instance().AddHubFunc("Hub/ChatSend: UserOnlineInGroupMsg, MemberSignalrDto");

            //Step 5: Update số người trong meeting lên db
            UserConnectionSignalrDto[] currentUsersInMeeting = await presenceTracker.GetOnlineUsersInMeet(meetingIdInt);
            await repos.Meetings.UpdateCountMemberSignalr(meetingIdInt, currentUsersInMeeting.Length);
            await repos.Complete();

            //Test
            await Clients.Caller.SendAsync("OnConnectMeetHubSuccessfully", $"Connect meethub dc r! Fucck you! {username} vô dc r ae ơi!!!");

            // Step 6: Thông báo với groupHub.Group(groupId) số người ở trong phòng  
            List<string> currentUserConnectionIds = await presenceTracker.GetConnectionIdsForUser(new UserConnectionSignalrDto(username, meetingIdInt));
            Console.WriteLine("2.1     " + new String('+', 50));
            Console.WriteLine("2.1     Hub/PresenceSend: CountMemberInGroupMsg, { meetingId, countMember }");
            FunctionTracker.Instance().AddHubFunc("Hub/PresenceSend: CountMemberInGroupMsg, { meetingId, countMember }");
            //await groupHub.Clients.AllExcept(currentUserConnectionIds).SendAsync(GroupHub.CountMemberInGroupMsg,
            //       new { meetingId = meetingIdInt, countMember = currentUsersInMeeting.Length });
            await groupHub.Clients.Group("1").SendAsync(GroupHub.CountMemberInMeetingMsg,
                  new { meetingId = meetingIdInt, countMember = currentUsersInMeeting.Length });

            //share screen cho user vao sau cung
            //step 7: Thông báo shareScreen cho user vào cuối 
            UserConnectionSignalrDto userIsSharing = await shareScreenTracker.GetUserIsSharingScreenForMeeting(meetingIdInt);
            if(userIsSharing != null)
            {
                List<string> sharingUserConnectionIds = await presenceTracker.GetConnectionIdsForUser(userIsSharing);
                if(sharingUserConnectionIds.Count > 0)
                {
                    await Clients.Clients(sharingUserConnectionIds).SendAsync(OnShareScreenLastUser, new { usernameTo = username, isShare = true });
                }

                await Clients.Caller.SendAsync(OnUserIsSharingMsg, userIsSharing.UserName);
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
            bool isOffline = await presenceTracker.UserDisconnected(new UserConnectionSignalrDto(username, meeting.RoomId), Context.ConnectionId);

            //step 4: Remove khỏi shareScreenTracker nếu có
            await shareScreenTracker.RemoveUserShareScreen(username, meeting.RoomId);

            //step 5: Remove ContextConnectionId khỏi meetingHub.Group(meetingId)   chắc move ra khỏi if
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, meeting.RoomId.ToString());
            if (isOffline)
            {
                ////step 5: Nếu ko còn connect nào nữa thì remove ContextConnectionId khỏi meetingHub.Group(meetingId)   chắc move ra khỏi if
                //await Groups.RemoveFromGroupAsync(Context.ConnectionId, meeting.RoomId.ToString());

                //step 6: Nếu ko còn connect nào nữa thì Thông báo với meetingHub.Group(meetingId)
                MemberSignalrDto offLineUser = await repos.Accounts.GetMemberSignalrAsync(username);
                await Clients.Group(meeting.RoomId.ToString()).SendAsync(UserOfflineInMeetingMsg, offLineUser);

                //step 7: Update lại số người trong phòng
                UserConnectionSignalrDto[] currentUsersInRoom = await presenceTracker.GetOnlineUsersInMeet(meeting.RoomId);
                await repos.Meetings.UpdateCountMemberSignalr(meeting.RoomId, currentUsersInRoom.Length);
                await repos.Complete();

                //await presenceHub.Clients.All.SendAsync("CountMemberInGroup",
                //       new { meetingId = group.RoomId, countMember = currentUsers.Length });

                //step 8: Thông báo với groupHub.Group(groupId) số người ở trong phòng
                //await groupHub.Clients.All.SendAsync(GroupHub.CountMemberInGroupMsg,
                //       new { meetingId = meeting.RoomId, countMember = currentUsersInRoom.Length });
                groupHub.Clients.Group("1").SendAsync(GroupHub.CountMemberInMeetingMsg,
                       new { meetingId = meeting.RoomId, countMember = currentUsersInRoom.Length });
            }
            //step 9: Disconnect khỏi meetHub
            await base.OnDisconnectedAsync(exception);
        }

        //FE gọi chatHubConnection.invoke('SendMessage', { content: string })
        public async Task SendMessage(MessageSignalrCreateDto createMessageDto)
        {
            Console.WriteLine("2.   " + new String('+', 50));
            Console.WriteLine("2.   Hub/Chat: SendMessage(CreateMessageDto)");
            FunctionTracker.Instance().AddHubFunc("Hub/Chat: SendMessage(CreateMessageDto)");
            string userName = Context.User.GetUsername();
            AppUser sender = await repos.Accounts.GetUserByUsernameSignalrAsync(userName);

            Meeting meeting = await repos.Meetings.GetMeetingForConnectionSignalr(Context.ConnectionId);

            if(meeting != null)
            {
                var message = new MessageSignalrGetDto
                {
                    SenderUsername = userName,
                    SenderDisplayName = sender.DisplayName,
                    Content = createMessageDto.Content,
                    MessageSent = DateTime.Now
                };
                //Luu message vao db
                //code here
                //send meaasge to group
                await Clients.Group(meeting.RoomId.ToString()).SendAsync(NewMessageMsg, message);
            }
        }
        //FE gọi chatHubConnection.invoke('MuteMicro', mute)
        public async Task MuteMicro(bool muteMicro)
        {
            Console.WriteLine("2.   " + new String('+', 50));
            Console.WriteLine("2.   Hub/Chat: MuteMicro(bool)");
            FunctionTracker.Instance().AddHubFunc("Hub/Chat: MuteMicro(bool)");
            var group = await repos.Meetings.GetMeetingForConnectionSignalr(Context.ConnectionId);
            if (group != null)
            {
                await Clients.Group(group.RoomId.ToString()).SendAsync(OnMuteMicroMsg, new { username = Context.User.GetUsername(), mute = muteMicro });
            }
            else
            {
                throw new HubException("group == null");
            }
        }

        //sẽ dc gọi khi FE gọi chatHubConnection.invoke('MuteCamera', mute)
        public async Task MuteCamera(bool muteCamera)
        {
            Console.WriteLine("2.   " + new String('+', 50));
            Console.WriteLine("2.   Hub/Chat: MuteCamera(bool)");
            FunctionTracker.Instance().AddHubFunc("Hub/Chat: MuteCamera(bool)");
            var group = await repos.Meetings.GetMeetingForConnectionSignalr(Context.ConnectionId);
            if(group != null)
            {
                await Clients.Group(group.RoomId.ToString()).SendAsync(OnMuteCameraMsg, new { username = Context.User.GetUsername(), mute = muteCamera });
            }
            else
            {
                throw new HubException("group == null");
            } 
        }

        //sẽ dc gọi khi FE gọi chatHubConnection.invoke('ShareScreen', meetingId, isShareScreen)
        public async Task ShareScreen(int meetingId, bool isShareScreen)
        {
            Console.WriteLine("2.   " + new String('+', 50));
            Console.WriteLine("2.   Hub/Chat: ShareScreen(id, bool)");
            FunctionTracker.Instance().AddHubFunc("Hub/Chat: ShareScreen(id, bool)");
            if (isShareScreen)//true is doing share
            {
                await shareScreenTracker.AddUserSharingScreen(new UserConnectionSignalrDto(Context.User.GetUsername(), meetingId));
                await Clients.Group(meetingId.ToString()).SendAsync(OnUserIsSharingMsg, Context.User.GetUsername());
            }
            else
            {
                await shareScreenTracker.RemoveUserShareScreen(new UserConnectionSignalrDto(Context.User.GetUsername(), meetingId));
            }
            await Clients.Group(meetingId.ToString()).SendAsync(OnShareScreenMsg, isShareScreen);
            //var group = await _unitOfWork.RoomRepository.GetRoomForConnection(Context.ConnectionId);
        }
        //sẽ dc gọi khi FE gọi chatHubConnection.invoke('ShareScreenToUser', meetingId, username, isShareScreen)
        public async Task ShareScreenToUser(int meetingId, string username, bool isShare)
        {
            Console.WriteLine("2.   " + new String('+', 50));
            Console.WriteLine("2.   Hub/Chat: ShareScreenToUser(id, username, bool)");
            FunctionTracker.Instance().AddHubFunc("Hub/Chat: ShareScreenToUser(id, username, bool)");
            var currentBeginConnectionsUser = await presenceTracker.GetConnectionIdsForUser(new UserConnectionSignalrDto(username, meetingId));
            if(currentBeginConnectionsUser.Count > 0)
                await Clients.Clients(currentBeginConnectionsUser).SendAsync(OnShareScreenMsg, isShare);
        }

        private async Task<Meeting> RemoveConnectionFromMeeting()
        {
            Console.WriteLine("2.   " + new String('+', 50));
            Console.WriteLine("2.   Hub/Chat: RemoveConnectionFromMeeting()");
            FunctionTracker.Instance().AddHubFunc("Hub/Chat: RemoveConnectionFromMeeting()");
            var meeting = await repos.Meetings.GetMeetingForConnectionSignalr(Context.ConnectionId);
            var connection = meeting.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            repos.Meetings.EndConnectionSignalr(connection);

            if (await repos.Complete()) return meeting;

            throw new HubException("Fail to remove connection from room");
        }

        //private async Task<Meeting> AddConnectionToGroup(int meetingId)
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


        //TestOnly
        public async Task TestReceiveInvoke(string msg)
        {
            Console.WriteLine("+++++++++++==================== " + msg + " ReceiveInvoke successfull");
            //int meetId = presenceTracker.
            Clients.Caller.SendAsync("OnTestReceiveInvoke", "invoke dc rồi ae ơi "+msg);
        }
    }
}
