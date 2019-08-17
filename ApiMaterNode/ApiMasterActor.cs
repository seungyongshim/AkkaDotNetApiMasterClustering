using System;
using Akka.Actor;
using Akka.Configuration;
using WorkNode;

namespace ApiMaterNode
{
    public class ApiMasterActor : ReceiveActor
    {
        public IActorRef Worker{ get; private set; }

        public static Props Props(Config config)
        {
            return Akka.Actor.Props.Create(() => new ApiMasterActor());
        }

        public ApiMasterActor()
        {
            if (Context.Child(nameof(WorkerActor)).Equals(ActorRefs.Nobody))
            {
                Worker = Context.ActorOf(WorkerActor.Props(), nameof(WorkerActor));
            }
            else
                Worker = Context.Child(nameof(WorkerActor));

            Receive<Int64>(msg => Console.WriteLine(msg));
        }

        protected override void PreRestart(Exception reason, object message)
        {
            /* Don't kill the children */
            PostStop();
        }

        protected override void PreStart()
        {
            Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1), Worker, "Hello", Self);
            base.PreStart();
        }
    }
}
