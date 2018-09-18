using Fleck;
using System;
using System.Threading.Tasks;
using System.Timers;
using System.Collections.Generic;
using GlowingPotato.CellAttack.Server.Simulator;
using GlowingPotato.CellAttack.Server.Net;
using System.Threading;
using System.Runtime;

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

        private static Action<Task> simTask;

        static void Main(string[] args)
        {
            Console.WriteLine(" ----- Cell Attack Server v1.0 ----- ");

            // create world
            World w = new World();
            // ---------- TESTING PURPOSES ONLY ----------
            w.AddChunk(new ChunkPos(0, 0));
            w.GetChunk(new ChunkPos(0, 0)).LoadThing(gliderGun, 5, 5, 37, 9, 0x01);
            //w.GetChunk(new ChunkPos(0, 0)).LoadThing(new byte[] { 1, 1, 1, 1 }, 31, 31, 2, 2, 0x07);
            // ---------- --------------------- ----------

            // create server
            List<IClientProxy> clients = new List<IClientProxy>();
            List<string> names = new List<string>(new string[] { "conflict" }); // for testing the name checker
            WebSocketServer server = new WebSocketServer("ws://0.0.0.0:8181");
            System.Threading.Timer timer = null;
            bool started = false;

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
                    Console.WriteLine("Connection closed.");
                    clients.Remove(proxy);
                };
                socket.OnMessage = message =>
                {
                    // get name packet
                    if (!names.Contains(message))
                    {
                        proxy.SendConnectPacket(0);
                        names.Add(message);
                    }
                    else
                    {
                        proxy.SendConnectPacket(128);
                        socket.Close();
                    }
                    clients.Add(proxy);

                    // if all players connected, start the game
                    if (clients.Count == PLAYER_COUNT && !started)
                    {
                        Console.WriteLine("All clients connected, starting server...");
                        foreach (IClientProxy s in clients)
                        {
                            s.SendConnectPacket(1);
                        }

                        // start simulation
                        timer.Change(0, 100);
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
            int simulations = 0;
            int timeSum = 0;
            timer = new System.Threading.Timer((a) =>
            {
                DateTime start = DateTime.Now;
                Task.WaitAll(Task.Run(() =>
                {
                    foreach (ChunkPos pos in w.GetChunks().Keys)
                    {
                        foreach (IClientProxy s in clients)
                        {
                            s.SendFieldDataPacket((int)(pos.X * Chunk.SIZE), (int)(pos.Y * Chunk.SIZE), Chunk.SIZE, Chunk.SIZE, w.GetChunks()[pos].GetOldBackingArray());
                        }
                    }

                    DateTime time1 = System.DateTime.Now;
                    w.Simulate();
                    DateTime time2 = System.DateTime.Now;

                    timeSum += (time2 - time1).Milliseconds;

                    if (simulations++ == 50)
                    {
                        DateTime gcstart = DateTime.Now;
                        GC.Collect();
                        DateTime gcend = DateTime.Now;
                        Console.WriteLine(String.Format("Simulated {0} chunks {1} times. Took on average {2}ms.", w.GetChunkCount(), simulations - 1, (double)timeSum / simulations));
                        simulations = 0;
                        timeSum = 0;
                    }
                }));
            }, null, -1, 100);

            Console.ReadKey();

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
