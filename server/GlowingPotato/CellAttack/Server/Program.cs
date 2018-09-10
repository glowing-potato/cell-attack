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

        public const int PLAYER_COUNT = 1;

        static void Main(string[] args)
        {
            Console.WriteLine(" ----- Cell Attack Server v1.0 ----- ");

            World w = new World();
            List<IClientProxy> clients = new List<IClientProxy>();
            WebSocketServer server = new WebSocketServer("ws://0.0.0.0:8181");

            // testing purposes only
            w.AddChunk(new ChunkPos(0, 0));
            Array.Fill(w.GetChunk(new ChunkPos(0, 0)).GetOldBackingArray(), (byte) Chunk.TIMER_MASK);
            Array.Fill(w.GetChunk(new ChunkPos(0, 0)).GetNewBackingArray(), (byte)Chunk.TIMER_MASK);
            w.GetChunk(new ChunkPos(0, 0)).GetOldBackingArray()[Chunk.SIZE * 2 + 2] = 0x07;
            w.GetChunk(new ChunkPos(0, 0)).GetOldBackingArray()[Chunk.SIZE * 2 + 3] = 0x07;
            w.GetChunk(new ChunkPos(0, 0)).GetOldBackingArray()[Chunk.SIZE * 2 + 4] = 0x07;

            int ctr = 0;
            Timer t = new Timer(500);
            t.Elapsed += (sender, eventArgs) =>
            {
                SendToAll(w.GetChunk(new ChunkPos(0, 0)).GetOldBackingArray(), clients);

                DateTime time1 = System.DateTime.Now;
                w.Simulate();
                DateTime time2 = System.DateTime.Now;
                Console.WriteLine("Simulated world. Took " + (time2 - time1).Milliseconds + "ms");

                ctr++;
               // if (ctr == 1)
                //{
                   // t.Stop();
                //}
                
            };

            server.SupportedSubProtocols = new string[] { "cell-attack-v0" };
            server.Start(socket =>
            {

                ClientProxyImpl proxy = new ClientProxyImpl(socket);

                socket.OnOpen = () =>
                {
                    Console.WriteLine("Client connected. Awaiting connect packet.");

                    socket.Send(new byte[] { 0 });

                    clients.Add(proxy);

                    if (clients.Count == PLAYER_COUNT)
                    {
                        Console.WriteLine("All clients connected, starting server...");

                        foreach (IClientProxy s in clients)
                        {
                            s.SendConnectPacket(1);
                        }

                        t.Start();
                        //t.Stop();
                        //w.Simulate();
                        //w.Simulate();
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
            Console.ReadKey();
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
