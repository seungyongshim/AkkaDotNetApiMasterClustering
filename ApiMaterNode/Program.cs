using System;
using Akka;
using Akka.Actor;
using Akka.Cluster.Tools.Singleton;
using Akka.Configuration;
using Petabridge.Cmd.Cluster;
using Petabridge.Cmd.Host;
using Petabridge.Cmd.Remote;

namespace ApiMaterNode
{
    class Program
    {
        static void Main(string[] args)
        {
            var port = Convert.ToInt32(args[0]);

            //Override the configuration of the port
            var config =
                ConfigurationFactory
                    .ParseString("petabridge.cmd.port=" + (10000 + port) + "\nakka.remote.dot-netty.tcp.port=" + port)
                    .WithFallback(ConfigurationFactory.ParseString(@"
petabridge.cmd
{
  host = ""0.0.0.0""
  log-palettes-on-startup = on
}

akka {
  log-config-on-start = on
  stdout-loglevel = DEBUG

  actor {
    provider = ""Akka.Cluster.ClusterActorRefProvider, Akka.Cluster""
    debug
    {
      receive = on         # Default off
      autoreceive = on     # Default off
      lifecycle = on       # Default off
      event-stream = on    # Default off
      unhandled = on       # Default off
    }

    deployment 
    {
		/ApiMasterActor 
		{
		
		}

        ""/ApiMasterActor/WorkerActor""
        {
          
		  router = round-robin-pool # routing strategy
		  nr-of-instances = 100 # max number of total routees
		  
		  cluster # Cluster 안에 node 당 최대 instances 개수 정의
          {
		  	enabled = on
            max-nr-of-instances-per-node = 2
            allow-local-routees = off
            use-role = work
          }
        }
    }
  }

  remote {
    log-remote-lifecycle-events = DEBUG
    dot-netty.tcp {
        hostname = ""0.0.0.0""
        port = 0
    }
    
    log-sent-messages = on
    log-received-messages = on
    log-remote-lifecycle-events = DEBUG
  }

  cluster {

    roles = [api]
    log-info = on
    allow-weakly-up-members = on

    debug {
      verbose-heartbeat-logging = on
	  verbose-receive-gossip-logging = on
    }

    seed-nodes = [
    ""akka.tcp://ClusterSystem@192.168.100.76:4053"",
    ""akka.tcp://ClusterSystem@192.168.100.76:4054""
    ]

    
    #auto-down-unreachable-after = 30s
  }
}"));

            //create an Akka system
            var system = ActorSystem.Create("ClusterSystem", config);

            var cmd = PetabridgeCmd.Get(system);
            cmd.RegisterCommandPalette(RemoteCommands.Instance);
            cmd.RegisterCommandPalette(ClusterCommands.Instance);
            cmd.Start();

            //create an actor that handles cluster domain events
            system.ActorOf(Props.Create(() => new ApiMasterActor()), "ApiMasterActor");

            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
        }
    }
}
