using Castle.MicroKernel.Registration;
using Castle.Windsor;
using CommonServiceLocator.WindsorAdapter.Unofficial;
using Fulcrum.Common.Web;
using log4net.Config;
using Merit.Runtime;
using Microsoft.Owin;
using Owin;
using System.Web.Mvc;

[assembly: OwinStartup(typeof(Merit.Invoicing.Startup))]
namespace Merit.Invoicing
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            XmlConfigurator.Configure();

            var container = new WindsorContainer();

            CommonAppSetup.ConfigureContainerWeb(container);

            // set up web api di
            /*GlobalConfiguration.Configuration.DependencyResolver = new WindsorDependencyResolver(container.Kernel);
            GlobalConfiguration.Configuration.Services.Replace(
                typeof(IHttpControllerActivator),
                new WindsorControllerActivator(container));*/

            // set up mvc di
            container.Register(Classes.FromThisAssembly()
                                       .BasedOn<IController>()
                                       .LifestylePerWebRequest());

            var controllerFactory = new WindsorControllerFactory(container.Kernel);

            ControllerBuilder.Current.SetControllerFactory(controllerFactory);

            DependencyResolver.SetResolver(new WindsorServiceLocator(container));

            CommonAppSetup.ConfigureEventPipeline(container);
        }
    }
}
