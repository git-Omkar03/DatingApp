using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.SignalR
{
    [Authorize]
    public class PresenceHub : Hub
    {
        private readonly PresenceTracker presenceTracker;

        public PresenceHub(PresenceTracker presenceTracker)
        {
            this.presenceTracker = presenceTracker;
        }

        public override async Task OnConnectedAsync()
        {
            await presenceTracker.UserConnected(Context.User.GetUserName(), Context.ConnectionId);
            await Clients.Others.SendAsync("UserIsOnline",Context.User.GetUserName());

            var currentUsers = await presenceTracker.GetOnlineUsers();
            await Clients.All.SendAsync("GetOnlineUsers", currentUsers);
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            await presenceTracker.UserDisconnected(Context.User.GetUserName(), Context.ConnectionId);

            await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUserName());

            await base.OnDisconnectedAsync(ex);

            var currentUsers = await presenceTracker.GetOnlineUsers();
            await Clients.All.SendAsync("GetOnlineUsers", currentUsers);
        }
    }
}
