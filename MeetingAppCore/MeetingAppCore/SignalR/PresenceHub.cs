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
    public class PresenceHub : Hub
    {
        private readonly PresenceTracker presnceTracker;
        public PresenceHub(PresenceTracker tracker)
        {
            presnceTracker = tracker;
        }
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("3.      " + new String('+', 50));
            Console.WriteLine("3.      Hub/Presence: OnConnectedAsync()");
            FunctionTracker.Instance().AddHubFunc("3.      Hub/Presence: OnConnectedAsync()");
            var isOnline = await presnceTracker.UserConnected(new UserConnectionDto(Context.User.GetUsername(), 0), Context.ConnectionId);            
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            Console.WriteLine("3.      " + new String('+', 50));
            Console.WriteLine("3.      Hub/Presence: OnDisconnectedAsync(Exception)");
            FunctionTracker.Instance().AddHubFunc("3.      Hub/Presence: OnDisconnectedAsync(Exception)");
            var isOffline = await presnceTracker.UserDisconnected(new UserConnectionDto(Context.User.GetUsername(), 0), Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
