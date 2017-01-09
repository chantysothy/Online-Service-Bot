using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(AgentLoginWeb.Startup))]
namespace AgentLoginWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
