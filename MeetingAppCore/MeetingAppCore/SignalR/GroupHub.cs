using MeetingAppCore.DebugTracker;
using MeetingAppCore.Dtos;
using MeetingAppCore.Extensions;
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
    public class GroupHub : Hub
    {
        //BE: SendAsync(GroupHub.CountMemberInGroupMsg, new { meetingId: int, countMember: int })
        public static string CountMemberInMeetingMsg => "CountMemberInMeeting";
        public static string OnLockedUserMsg => "OnLockedUser";

        private readonly PresenceTracker presnceTracker;
        public GroupHub(PresenceTracker presnceTracker)
        {
            this.presnceTracker = presnceTracker;
        }
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("3.      " + new String('+', 50));
            Console.WriteLine("3.      Hub/Presence: OnConnectedAsync()");
            FunctionTracker.Instance().AddHubFunc("3.      Hub/Presence: OnConnectedAsync()");

            HttpContext httpContext = Context.GetHttpContext();
            string groupIdString = httpContext.Request.Query["groupId"].ToString();
            int groupIdInt = int.Parse(groupIdString);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupIdString);//khi user click vao room se join vao

            //Test
            await Clients.Caller.SendAsync("OnConnectMeetHubSuccessfully", $"Connect GroupHub dc r! Fucck you! Tao vô dc r ae ơi!!!");

            var isOnline = await presnceTracker.UserConnected(new UserConnectionSignalrDto(Context.User.GetUsername(), 0), Context.ConnectionId);            
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            Console.WriteLine("3.      " + new String('+', 50));
            Console.WriteLine("3.      Hub/Presence: OnDisconnectedAsync(Exception)");
            FunctionTracker.Instance().AddHubFunc("3.      Hub/Presence: OnDisconnectedAsync(Exception)");
            var isOffline = await presnceTracker.UserDisconnected(new UserConnectionSignalrDto(Context.User.GetUsername(), 0), Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        //TestOnly
        public async Task TestReceiveInvoke(string msg)
        {
            Console.WriteLine("+++++++++++==================== " + msg + " group ReceiveInvoke successfull");
            //int meetId = presenceTracker.
            Clients.Group("1").SendAsync("OnTestReceiveInvoke", Context.User.GetUsername()+" group invoke dc rồi ae ơi " + msg);
        }
    }
}
