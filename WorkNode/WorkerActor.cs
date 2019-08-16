using System;
using Akka.Actor;
using Akka.Routing;

namespace WorkNode
{
    public class WorkerActor : ReceiveActor
    {
        public static Props Props()
        {
            return Akka.Actor.Props.Create(() => new WorkerActor()).WithRouter(FromConfig.Instance); 
        }

        public WorkerActor()
        {
            Receive<Int64>(msg => Console.WriteLine(msg));
        }
    }
}
