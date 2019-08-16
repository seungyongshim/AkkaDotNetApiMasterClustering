using System;
using Akka;
using Akka.Actor;
using Akka.Configuration;
using Petabridge.Cmd.Cluster;
using Petabridge.Cmd.Host;
using Petabridge.Cmd.Remote;

namespace WorkNode
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
    log-info = on

    debug {
      verbose-heartbeat-logging = on
	  verbose-receive-gossip-logging = on
    }

    roles = [work]

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

            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
        }
    }
}
