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

namespace GlowingPotato.CellAttack.Server
{
    class Program
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
        public const int PLAYER_COUNT = 1;
        private static int simSpeed = 100;

        static void Main(string[] args)
        {
            Console.WriteLine(" ----- Cell Attack Server v1.0 ----- ");

            // create world
            World w = new World();

            // ---------- TESTING PURPOSES ONLY ----------
            w.AddChunk(new ChunkPos(0, 0));
            w.GetChunk(new ChunkPos(0, 0)).LoadThing(gliderGun, 5, 5, 37, 9, 0x01);
            // ---------- --------------------- ----------

            // create server
            List<IClientProxy> clients = new List<IClientProxy>();
            List<string> names = new List<string>(new string[] { "conflict" }); // for testing the name checker
            WebSocketServer server = new WebSocketServer("ws://0.0.0.0:8181");
            System.Threading.Timer timer = null;
            bool started = false;

            Socket serverSocket = new TcpListener(IPAddress.Any, 8181).Server;
            serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            server.ListenerSocket = new SocketWrapper(serverSocket);
            server.SupportedSubProtocols = new string[] { "cell-attack-v0" };
            server.Start(socket =>
            {
                ClientProxyImpl proxy = new ClientProxyImpl(socket);
                socket.OnOpen = () =>
                {
                    if (started)
                    {
                        Console.WriteLine("Client connected, but game has already started. Disconnecting.");
                        proxy.SendConnectPacket(129);
                        socket.Close();
                    }
                    else
                    {
                        Console.WriteLine("Client connected. Awaiting connect packet.");
                    }
                };
                socket.OnClose = () =>
                {
                    Console.WriteLine("Connection to client closed.");
                    clients.Remove(proxy);
                };
                socket.OnMessage = message =>
                {
                    // get name packet
                    if (!names.Contains(message))
                    {
                        Console.WriteLine("Client connect packet recieved.");
                        proxy.SendConnectPacket(0);
                        names.Add(message);
                        clients.Add(proxy);
                    }
                    else
                    {
                        proxy.SendConnectPacket(128);
                        socket.Close();
                    }

                    // if all players connected, start the game
                    if (clients.Count == PLAYER_COUNT && !started)
                    {
                        Console.WriteLine("All clients connected, starting server...");
                        foreach (IClientProxy s in clients)
                        {
                            s.SendConnectPacket(1);
                        }

                        // start simulation
                        timer.Change(0, simSpeed);
                        started = true;
                    }
                };
                socket.OnBinary = message =>
                {
                    //if (message.Length == )
                };
                /*socket.OnError = error => 
                {

                };*/
            });

            // create timer to simulate world
            int[] lastTime = new int[10];
            int timeIndex = 0;
            timer = new System.Threading.Timer((a) =>
            {
                DateTime start = DateTime.Now;
                Task.WaitAll(Task.Run(() =>
                {
                    ChunkPos[] chunks = new ChunkPos[w.GetChunkCount()];
                    w.GetChunks().Keys.CopyTo(chunks, 0);

                    foreach (IClientProxy s in clients)
                    {
                        foreach (ChunkPos pos in chunks)
                        {
                            s.SendFieldDataPacket((int)(pos.X * Chunk.SIZE), (int)(pos.Y * Chunk.SIZE), Chunk.SIZE, Chunk.SIZE, w.GetChunks()[pos].GetOldBackingArray());
                        }
                    }


                    DateTime time1 = System.DateTime.Now;
                    w.Simulate();
                    DateTime time2 = System.DateTime.Now;

                    lastTime[timeIndex] = (time2 - time1).Milliseconds;

                    if (++timeIndex > lastTime.Length - 1)
                    {
                        timeIndex = 0;
                    }
                }));
            }, null, -1, simSpeed);

            string[] cmd = null;
            while ((cmd = Console.ReadLine().Split(" "))[0] != "/exit")
            {
                switch (cmd[0])
                {
                    case "/forcestart":
                        Console.WriteLine("Starting game forcibly.");
                        timer.Change(0, simSpeed);
                        started = true;
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
                        foreach (string name in names)
                        {
                            Console.WriteLine(name);
                        }
                        break;
                    case "/simspeed":
                        if (cmd.Length > 1)
                        {
                            try
                            {
                                simSpeed = Convert.ToInt32(cmd[1]);
                                Console.WriteLine("Setting simulation speed to " + simSpeed + "ms");
                                timer.Change(0, simSpeed);
                            }
                            catch (FormatException)
                            {
                                Console.WriteLine("Invalid parameter for the simulation speed.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Simulation speed: " + simSpeed + "ms");
                        }
                        break;
                    case "/simstats":
                        int sum = lastTime.Sum();
                        Console.WriteLine(String.Format("Last {0} simulations with {1} chunks took on average {2}ms.", lastTime.Length, w.GetChunkCount(), (double)sum / lastTime.Length));
                        break;
                    default:
                        Console.WriteLine("Invalid command. Type \"/help\" for a list of commands.");
                        break;
                }
            }

        }

        static void SendToAll(byte[] message, List<IClientProxy> sockets)
        {
            foreach (IClientProxy s in sockets)
            {
                s.SendFieldDataPacket(0, 0, Chunk.SIZE, Chunk.SIZE, message);
            }
        }
    }
}
