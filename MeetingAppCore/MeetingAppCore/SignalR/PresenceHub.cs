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
        private readonly PresenceTracker _tracker;
        public PresenceHub(PresenceTracker tracker)
        {
            _tracker = tracker;
        }
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("3.      " + new String('+', 10));
            Console.WriteLine("Hub/Presence: OnConnectedAsync()");
            var isOnline = await _tracker.UserConnected(new UserConnectionDto(Context.User.GetUsername(), 0), Context.ConnectionId);            
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            Console.WriteLine("3.      " + new String('+', 10));
            Console.WriteLine("Hub/Presence: OnDisconnectedAsync()");
            var isOffline = await _tracker.UserDisconnected(new UserConnectionDto(Context.User.GetUsername(), 0), Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
