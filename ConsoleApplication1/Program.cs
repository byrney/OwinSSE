using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Hosting;
using Owin;
using System.IO;
using System.Collections.Generic;

namespace OwinConsole
{
    class Program
    {
        static void Main(string[] args)
        {

            StartOptions options = new StartOptions();
            using (WebApp.Start<Startup>("http://localhost:12345"))
            {
                Console.ReadLine();
            }
        }
    }

    public class Api
    {
        System.Timers.Timer _timer = new System.Timers.Timer(25000);
        List<StreamWriter> _writers = new List<StreamWriter>();
        public Api()
        {
            _timer.Elapsed += _timer_Elapsed;
         //   _timer.Start();
        }

        void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            UpdateWriters();
        }

        public void UpdateWriters()
        {

            _writers.ForEach(w => w.WriteLineAsync("Hello async"));
            _timer.Start();

        }

        public async Task Invoke(IOwinContext context)
        {
            context.Response.ContentType = "text/eventstream";
            context.Response.Headers["Content-Encoding"] = "chunked";
            context.Response.Headers["cache-control"] = "no-cache";
            context.Response.ContentLength = null;
            System.IO.Stream responseStream = context.Environment["owin.ResponseBody"] as Stream;
            var w = new StreamWriter(responseStream);
            _writers.Add(w);
            await w.WriteLineAsync("Registered");
            context.Response.Body.Flush();
            _timer.Start();
         //  w.Close();
          //  await context.Response.WriteAsync("Hello from Api");
        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var api = new Api();
            app.Run(context => api.Invoke(context));
        }
    }

}
