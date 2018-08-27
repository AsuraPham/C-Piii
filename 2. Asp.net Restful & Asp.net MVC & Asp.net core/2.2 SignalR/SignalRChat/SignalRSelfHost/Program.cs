using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;
using System;

namespace SignalRSelfHost
{
    class Program
    {
        static void Main(string[] args)
        {
            string url = "http://localhost:8080";
            using (WebApp.Start<StartUp>(url))
            {
                Console.WriteLine("Server" + url);
                Console.ReadLine();
            }
        }
    }
    class StartUp
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR();
        }
    }
    public class MyHub : Hub
    {
        public void Send(ChatMessage message)
        {
            Clients.All.addMessage(message);
        }
    }
}
