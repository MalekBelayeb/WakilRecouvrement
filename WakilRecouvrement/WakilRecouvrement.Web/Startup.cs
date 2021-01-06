using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Owin;

[assembly: OwinStartup(typeof(WakilRecouvrement.Web.Startup))]

namespace WakilRecouvrement.Web
{

    public partial class Startup
    {

        public void Configuration(IAppBuilder app)
        {
             // app.MapSignalR();
            
        }
    }
}
