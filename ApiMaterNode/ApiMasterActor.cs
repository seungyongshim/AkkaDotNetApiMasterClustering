using System;
using Akka.Actor;
using Akka.Configuration;
using WorkNode;

namespace ApiMaterNode
{
    public class ApiMasterActor : ReceiveActor
    {
        public IActorRef Worker{ get; private set; }

        public Int64 Index { get; set; } = 0;

        public static Props Props(Config config)
        {
            return Akka.Actor.Props.Create(() => new ApiMasterActor());
        }

        public ApiMasterActor()
        {
            Worker = Context.ActorOf(WorkerActor.Props(), nameof(WorkerActor));
            Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1), Worker, Index++, Self);
        }

        protected override void PreRestart(Exception reason, object message)
        {
            /* Don't kill the children */
            PostStop();
        }

        protected override void PreStart()
        {
            base.PreStart();
        }
    }
}
