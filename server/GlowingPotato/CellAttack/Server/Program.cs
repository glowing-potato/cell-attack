using Fleck;
using System;
using System.Threading.Tasks;
using System.Timers;
using System.Collections.Generic;
using GlowingPotato.CellAttack.Server.Simulator;
using GlowingPotato.CellAttack.Server.Net;
using System.Threading;
using System.Runtime;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using GlowingPotato.CellAttack.Server.World;

namespace GlowingPotato.CellAttack.Server
{
    public class Program
    {

        public static readonly byte[] gliderGun = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                                               0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                                               0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1,
                                                               0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1,
                                                               0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                                               0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 1, 1, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                                               0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                                               0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                                               0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
                                                             };
        

        static void Main(string[] args)
        {
            Console.WriteLine(" ----- Cell Attack Server v1.0 ----- ");

            // create world
            World.World w = new World.World();

            // ---------- TESTING PURPOSES ONLY ----------
            w.AddChunk(new ChunkPos(0, 0));
            w.GetChunk(new ChunkPos(0, 0)).LoadThing(gliderGun, 5, 5, 37, 9, 0x01);
            // ---------- --------------------- ----------

            // create simulator
            ISimulator simulator = new DefaultSimulator();

            // create server
            WebSocketServer webSocket = new WebSocketServer("ws://0.0.0.0:8181");
            Socket serverSocket = new TcpListener(IPAddress.Any, 8181).Server;
            serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            webSocket.ListenerSocket = new SocketWrapper(serverSocket);
            webSocket.SupportedSubProtocols = new string[] { "cell-attack-v0" };
            CellAttackServerImpl server = new CellAttackServerImpl(simulator, webSocket);

            server.World = w;
            server.Open();

            string[] cmd = null;
            while ((cmd = Console.ReadLine().Split(" "))[0] != "/exit")
            {
                switch (cmd[0])
                {
                    case "/forcestart":
                        Console.WriteLine("Starting game forcibly.");
                        server.Start();
                        break;
                    case "/help":
                        Console.WriteLine("Commands: /exit, /forcestart, /help, /loaddemo, /players, /simspeed, /simstats");
                        break;
                    case "/loaddemo":
                        w.GetChunks().Clear();
                        w.AddChunk(new ChunkPos(0, 0));
                        w.GetChunk(new ChunkPos(0, 0)).LoadThing(gliderGun, 5, 5, 37, 9, 0x01);
                        Console.WriteLine("Loaded demo simulation.");
                        break;
                    case "/players":
                        Console.WriteLine("Currently connected players:");
                        foreach (string name in server.ClientNames)
                        {
                            Console.WriteLine(name);
                        }
                        break;
                    case "/simspeed":
                        if (cmd.Length > 1)
                        {
                            try
                            {
                                int simulatorSpeed = Convert.ToInt32(cmd[1]);
                                Console.WriteLine("Setting simulation speed to " + simulatorSpeed + "ms");
                                server.SimulatorSpeed = simulatorSpeed;
                            }
                            catch (FormatException)
                            {
                                Console.WriteLine("Invalid parameter for the simulation speed.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Simulation speed: " + server.SimulatorSpeed + "ms");
                        }
                        break;
                    case "/simstats":
                        /*int sum = lastTime.Sum();
                        Console.WriteLine(String.Format("Last {0} simulations with {1} chunks took on average {2}ms.", lastTime.Length, w.GetChunkCount(), (double)sum / lastTime.Length));*/
                        break;
                    default:
                        Console.WriteLine("Invalid command. Type \"/help\" for a list of commands.");
                        break;
                }
            }

            

        }
    }
}
