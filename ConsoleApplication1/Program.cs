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
    // runs the server
    class Program
    {
        static void Main(string[] args)
        {
            using (WebApp.Start<Startup>("http://localhost:12345"))
            {
                Console.ReadLine();
            }
        }
    }

    // creates the pipeline  (of one)
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var api = new Api();
            app.Run(context => api.Invoke(context));
        }
    }

    public class Subscriber
    {
        private StreamWriter _writer;
        private TaskCompletionSource<bool> _tcs;
        public Subscriber(Stream body, TaskCompletionSource<bool> tcs)
        {
            this._writer = new StreamWriter(body);
            this._tcs = tcs;
        }

        public async void WriteAsync(string message)
        {
            try
            {
                _writer.Write(message);
                _writer.Flush();
            }
            catch(Exception e)
            {
                if (e.HResult == -2146232800) // non-existent connection
                    _tcs.SetResult(true);
                else
                    _tcs.SetException(e);
            }
        }
    }

    public class Api
    {
        System.Timers.Timer _timer = new System.Timers.Timer(10000);
        List<Subscriber> _subscribers = new List<Subscriber>();
        public Api()
        {
            _timer.Elapsed += _timer_Elapsed;
        }

        void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            UpdateSubscribers();
        }

        public void UpdateSubscribers()
        {
            Console.WriteLine("updating {0} subscribers", _subscribers.Count);
            var subscribersCopy = _subscribers.ToList<Subscriber>();
            var msg = String.Format("Hello async at {0}\n", DateTime.Now);
            subscribersCopy.ForEach(w => w.WriteAsync(msg));
            _timer.Start();
        }


        public Task Invoke(IOwinContext context)
        {
            SetEventHeaders(context);
            System.IO.Stream responseStream = context.Environment["owin.ResponseBody"] as Stream;
            var tcs = new TaskCompletionSource<bool>();
            var s = CreateSubscriber(responseStream, tcs);
            tcs.Task.ContinueWith(_ => _subscribers.Remove(s));
            Console.WriteLine("Add subscriber. Now have {0}", _subscribers.Count);
            s.WriteAsync("Registered\n");
            _timer.Start();
            return tcs.Task;            
        }

        private Subscriber CreateSubscriber(System.IO.Stream responseStream, TaskCompletionSource<bool> tcs)
        {
            var s = new Subscriber(responseStream, tcs);
            _subscribers.Add(s);
            return s;
        }

        private static void SetEventHeaders(IOwinContext context)
        {
            context.Response.ContentType = "text/eventstream";
            context.Response.Headers["Transfer-Encoding"] = "chunked";
            context.Response.Headers["cache-control"] = "no-cache";
        }
    }
    //  test push to git
}
