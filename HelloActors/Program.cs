using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloActors
{
    class Program
    {
        static void Main(string[] args)
        {
            var actorSystem = ActorSystem.Create("mysystem");
            IActorRef actorRef = actorSystem.ActorOf(Props.Create<EchoActor>());
            actorRef.Ask(new Initialize(){UserName = "tamir"}).Wait();
            var res=actorRef.Ask("Hello World").Result;
            //Console.WriteLine(res);
            Console.ReadLine();
        }
    }

    public class EchoActor:ReceiveActor
    {
        public string UserName { get; set; }


        public EchoActor()
        {
            Become(Uninitialized);
            
        }

        private void Uninitialized()
        {
            Receive<Initialize>(s =>
            {
                UserName = s.UserName;
                Become(Initialized);
                Context.Sender.Tell(s.UserName);
            });
        }


        private void Initialized()
        {
            Receive<string>(msg =>
            {
                Console.WriteLine($"{UserName} - {msg}");
                Context.Sender.Tell(msg);
            });
        }
    }

    class Initialize
    {
        public string UserName { get; set; }
    }
}
