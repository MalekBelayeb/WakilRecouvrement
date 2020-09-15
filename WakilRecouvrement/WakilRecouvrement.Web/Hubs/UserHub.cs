using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace WakilRecouvrement.Web.Hubs
{
    [HubName("userHub")]

    public class UserHub : Hub
    {
 

        public static void Show()
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<UserHub>();
            context.Clients.All.displayNotifs();
        }
    }
}