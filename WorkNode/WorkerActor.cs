using System;
using Akka.Actor;
using Akka.Routing;

namespace WorkNode
{
    public class WorkerActor : ReceiveActor
    {
        public Int64 Index { get; set; } = 0;

        public static Props Props()
        {
            return Akka.Actor.Props.Create(() => new WorkerActor()).WithRouter(FromConfig.Instance); 
        }

        protected override void PreStart()
        {
            Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1), Self, Index, Self);
            base.PreStart();
        }

        public WorkerActor()
        {
            Receive<string>(msg =>
            {
                Sender.Tell(Index++);
            });

            Receive<Int64>(msg =>
            {
                Console.WriteLine(Index);
            });
        }
    }
}
