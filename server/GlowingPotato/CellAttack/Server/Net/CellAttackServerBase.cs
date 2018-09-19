using Fleck;
using GlowingPotato.CellAttack.Server.Simulator;
using GlowingPotato.CellAttack.Server.World;
using System;
using System.Collections.Generic;
using System.Text;

namespace GlowingPotato.CellAttack.Server.Net
{
    public abstract class CellAttackServerBase
    {

        private int maxPlayerCount = 1;
        private int timerDelay = -1;
        private int simulatorSpeed = 100;
        private bool started = false;

        private WebSocketServer server;
        private ClientManager clientManager = new ClientManager();
        private World.World world = null;
        private System.Threading.Timer timer = null;
        private ISimulator simulator;

        private int[] lastTime = new int[10];
        private int timeIndex = 0;

        public CellAttackServerBase(ISimulator simulator, WebSocketServer server)
        {
            this.simulator = simulator;
            this.server = server;

            timer = new System.Threading.Timer((a) =>
            {

                ChunkPos[] chunks = new ChunkPos[world.GetChunkCount()];
                world.GetChunks().Keys.CopyTo(chunks, 0);

                clientManager.ForEach((client) =>
                {
                    foreach (ChunkPos pos in chunks)
                    {
                        client.SendFieldDataPacket((int)(pos.X * Chunk.SIZE), (int)(pos.Y * Chunk.SIZE), Chunk.SIZE, Chunk.SIZE, world.GetChunks()[pos].GetOldBackingArray());
                    }
                });


                DateTime time1 = System.DateTime.Now;
                simulator.Simulate(world);
                DateTime time2 = System.DateTime.Now;

                lastTime[timeIndex] = (time2 - time1).Milliseconds;

                if (++timeIndex > lastTime.Length - 1)
                {
                    timeIndex = 0;
                }

            });
        }

        public abstract void Tick();

        public void Start()
        {
            server.Start(socket =>
            {
                ClientProxyImpl client = new ClientProxyImpl(socket);
                socket.OnOpen = () =>
                {
                    if (started)
                    {
                        Console.WriteLine("Client connected, but game has already started. Disconnecting.");
                        client.SendConnectPacket(129);
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
                    clientManager.RemoveClient(client);
                };
                socket.OnMessage = message =>
                {
                    // get name packet
                    if (!clientManager.ContainsClient(message))
                    {
                        Console.WriteLine("Client " + message + "'s connect packet recieved.");
                        client.SendConnectPacket(0);
                        clientManager.AddClient(client);

                        // if all players connected, start the game
                        if (clientManager.ClientCount == maxPlayerCount && !started)
                        {
                            Console.WriteLine("All clients connected, starting server...");
                            clientManager.ForEach((c) =>
                            {
                                c.SendConnectPacket(1);
                            });

                            // start simulation
                            Start();
                        }
                    }
                    else
                    {
                        client.SendConnectPacket(128);
                        socket.Close();
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
            timerDelay = 0;
            started = true;
            UpdateTimer();
        }

        public void Stop()
        {
            started = false;
            timerDelay = -1;
            UpdateTimer();
        }

        public bool Started
        {
            get
            {
                return started;
            }
        }

        public int PlayerCount
        {
            get
            {
                return clientManager.ClientCount;
            }
        }

        public int MaxPlayerCount
        {
            get
            {
                return maxPlayerCount;
            }
            set
            {
                maxPlayerCount = value;
            }
        }

        public int SimulatorSpeed {
            get
            {
                return simulatorSpeed;
            }
            set
            {
                simulatorSpeed = value;
                UpdateTimer();
            }
        }

        public World.World World
        {
            get
            {
                return world;
            }
            set
            {
                world = value;
            }
        }

        public string[] ClientNames
        {
            get
            {
                return clientManager.GetClientNames();
            }
        }

        protected ISimulator Simulator
        {
            get
            {
                return simulator;
            }
            set
            {
                simulator = value;
            }
        }

        private void UpdateTimer()
        {
            timer.Change(timerDelay, simulatorSpeed);
        }

    }
}
