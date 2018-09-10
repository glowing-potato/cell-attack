using Fleck;
using System;
using System.Threading.Tasks;
using System.Timers;
using System.Collections.Generic;
using GlowingPotato.CellAttack.Server.Simulator;
using GlowingPotato.CellAttack.Server.Net;

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
            w.GetChunk(new ChunkPos(0, 0)).LoadThing(gliderGun, 5, 5, 37, 9, 0x07);
            // ---------- --------------------- ----------

            // create server
            List<IClientProxy> clients = new List<IClientProxy>();
            List<string> names = new List<string>(new string[] { "conflict" }); // for testing the name checker
            WebSocketServer server = new WebSocketServer("ws://0.0.0.0:8181");

            server.SupportedSubProtocols = new string[] { "cell-attack-v0" };
            server.Start(socket =>
            {

                ClientProxyImpl proxy = new ClientProxyImpl(socket);

                socket.OnOpen = () =>
                {
                    Console.WriteLine("Client connected. Awaiting connect packet.");
                    bool done = false;
                    string name = null;

                    // loop until they provide a valid username
                    while (!done)
                    {
                        proxy.GetConnectPacket(out name);
                        if (!names.Contains(name))
                        {
                            done = true;
                            proxy.SendConnectPacket(0);
                        }
                        else
                        {
                            proxy.SendConnectPacket(128);
                        }
                    }

                    // connect player
                    Console.WriteLine("Player \"" + name + "\" connected. Waiting for " + (PLAYER_COUNT - clients.Count) + " more player(s).");
                    clients.Add(proxy);

                    // if all players connected, start the game
                    if (clients.Count == PLAYER_COUNT)
                    {
                        Console.WriteLine("All clients connected, starting server...");
                        foreach (IClientProxy s in clients)
                        {
                            s.SendConnectPacket(1);
                        }
                        simTask(null);
                    }
                };
                socket.OnClose = () =>
                {
                    Console.WriteLine("Connection closed.");
                    clients.Remove(proxy);
                };
                socket.OnMessage = message =>
                {

                };
                socket.OnBinary = message =>
                {

                };

            });

            // create simulation task
            System.Threading.CancellationTokenSource token = new System.Threading.CancellationTokenSource();
            simTask = (a) => Task.Run(() =>
            Task.WaitAll(
            Task.Run(() =>
            {
                SendToAll(w.GetChunk(new ChunkPos(0, 0)).GetOldBackingArray(), clients);

                DateTime time1 = System.DateTime.Now;
                w.Simulate();
                DateTime time2 = System.DateTime.Now;

                Console.WriteLine("Simulated world. Took " + (time2 - time1).Milliseconds + "ms");

            }), Task.Delay(100))).ContinueWith(simTask, token.Token);

            Task delayTask = Task.Delay(-1, token.Token).ContinueWith((a) =>
            {
                Console.WriteLine("Server terminated. Press any key to continue...");
                Console.ReadKey();
            });
            // token.Cancel();
            Task.WaitAll(delayTask);
            
        }

        static void SendToAll(byte[] message, List<IClientProxy> sockets)
        {
            Console.WriteLine("Sending packet with length " + message.Length + " to clients");
            foreach (IClientProxy s in sockets)
            {
                s.SendFieldDataPacket(0, 0, Chunk.SIZE, Chunk.SIZE, message);
            }
        }
    }
}
