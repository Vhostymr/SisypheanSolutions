using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SisypheanSolutions.Startup))]
namespace SisypheanSolutions
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
        }
    }
}
