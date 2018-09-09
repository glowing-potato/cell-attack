using Fleck;
using System;
using System.Threading.Tasks;
using System.Timers;
using System.Collections.Generic;

namespace server
{
    class Program
    {

        public const int PLAYER_COUNT = 1;

        static void Main(string[] args)
        {
            Console.WriteLine(" ----- Cell Attack Server v1.0 ----- ");

            World w = new World();
            List<IWebSocketConnection> sockets = new List<IWebSocketConnection>();
            WebSocketServer server = new WebSocketServer("ws://0.0.0.0:8181");

            // testing purposes only
            w.AddChunk(new ChunkPos(0, 0));
            w.GetChunk(new ChunkPos(0, 0)).GetOldBackingArray()[Chunk.SIZE * 2 + 2] = 0x07;
            w.GetChunk(new ChunkPos(0, 0)).GetOldBackingArray()[Chunk.SIZE * 2 + 3] = 0x07;
            w.GetChunk(new ChunkPos(0, 0)).GetOldBackingArray()[Chunk.SIZE * 2 + 4] = 0x07;

            Timer t = new Timer(100);
            t.Elapsed += (sender, eventArgs) =>
            {
                DateTime time1 = System.DateTime.Now;
                w.Simulate();
                DateTime time2 = System.DateTime.Now;
                Console.WriteLine("Simulated world. Took " + (time2 - time1).Milliseconds + "ms");

                byte[] packet = new byte[Chunk.SIZE * Chunk.SIZE + 12];
                BitConverter.GetBytes(0L).CopyTo(packet, 0);
                BitConverter.GetBytes(0L).CopyTo(packet, 4);
                BitConverter.GetBytes((short) Chunk.SIZE).CopyTo(packet, 8);
                BitConverter.GetBytes((short) Chunk.SIZE).CopyTo(packet, 10);
                w.GetChunk(new ChunkPos(0, 0)).GetOldBackingArray().CopyTo(packet, 12);

                SendToAll(packet, sockets);
            };

            server.SupportedSubProtocols = new string[] { "cell-attack-v0" };
            server.Start(socket =>
            {

                string name = null;

                socket.OnOpen = () =>
                {
                    Console.WriteLine("Client connected. Awaiting connect packet.");

                    socket.Send(new byte[] { 0 });

                    sockets.Add(socket);

                    if (sockets.Count == PLAYER_COUNT)
                    {
                        Console.WriteLine("All clients connected, starting server...");
                        SendToAll(new byte[] { 1 }, sockets);
                        t.Start();
                        //t.Stop();
                        //w.Simulate();
                        //w.Simulate();
                    }
                };
                socket.OnClose = () =>
                {
                    Console.WriteLine("Connection closed.");
                    sockets.Remove(socket);
                };
                socket.OnMessage = message =>
                {
                    name = message;
                };
                socket.OnBinary = message =>
                {
                    
                };
                
            });
            Console.ReadKey();
        }

        static void SendToAll(byte[] message, List<IWebSocketConnection> sockets)
        {
            Console.WriteLine("Sending packet with length " + message.Length + " to clients");
            foreach (IWebSocketConnection s in sockets)
            {
                s.Send(message);
            }
        }
    }
}
