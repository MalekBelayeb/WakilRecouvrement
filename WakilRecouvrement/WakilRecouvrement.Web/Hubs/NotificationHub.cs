using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WakilRecouvrement.Web.Hubs
{
    public class NotificationHub : Hub
    {
        public void Send(string message)
        {
            Clients.All.addNewNotification(message);
        }
    }
}