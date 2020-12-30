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
 
/*
        public static void Show()
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<UserHub>();
            context.Clients.All.displayNotifs();
        }

        public static void ShowByAdminRole()
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<UserHub>();
            context.Clients.All.displayAdminNotifs();
         }

        public static void ShowByAgentRole()
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<UserHub>();
            context.Clients.All.displayAgentNotifs();
        }

        public static void ShowByUser()
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<UserHub>();
            context.Clients.All.displayUserNotifs();
        }
*/
    }
}