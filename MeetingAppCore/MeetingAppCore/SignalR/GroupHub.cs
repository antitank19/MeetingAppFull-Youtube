using MeetingAppCore.DebugTracker;
using MeetingAppCore.Dtos;
using MeetingAppCore.Extensions;
using Microsoft.AspNetCore.Authorization;
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
        public static string CountMemberInGroupMsg => "CountMemberInGroup";
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
           
            //Test
            await Clients.Caller.SendAsync("OnConnectMeetHubSuccessfully", $"Connect meethub dc r! Fucck you! Tao vô dc r ae ơi!!!");

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
            Console.WriteLine("+++++++++++==================== " + msg + " ReceiveInvoke successfull");
            //int meetId = presenceTracker.
            Clients.Caller.SendAsync("OnTestReceiveInvoke", "invoke dc rồi ae ơi " + msg);
        }
    }
}
