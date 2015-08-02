using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ExactSync.Startup))]
namespace ExactSync
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}