using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlowingPotato.CellAttack.Server.Simulator
{

    public class World
    {

        private Dictionary<ChunkPos, Chunk> map = new Dictionary<ChunkPos, Chunk>();
        private Dictionary<ChunkPos, bool> deleteMap = new Dictionary<ChunkPos, bool>();
        private IWorldInterface worldInterface;

        public World()
        {

        }

        public void Simulate()
        {
            bool nc = false, nec = false, ec = false, sec = false, sc = false, swc = false, wc = false, nwc = false;
            ChunkPos[] keys = new ChunkPos[map.Count];
            map.Keys.CopyTo(keys, 0);
            foreach (ChunkPos pos in keys)
            {
                // check if any chunks need to be created
                map[pos].CheckEdges(out nc, out nec, out ec, out sec, out sc, out swc, out wc, out nwc);

                // create chunks
                if (nc && (!map.ContainsKey(pos.North()) || map[pos.North()] == null))
                {
                    map[pos.North()] = new Chunk();
                }
                if (nec && (!map.ContainsKey(pos.North().East()) || map[pos.North().East()] == null))
                {
                    map[pos.North().East()] = new Chunk();
                }
                if (ec && (!map.ContainsKey(pos.East()) || map[pos.East()] == null))
                {
                    map[pos.East()] = new Chunk();
                }
                if (sec && (!map.ContainsKey(pos.South().East()) || map[pos.South().East()] == null))
                {
                    map[pos.South().East()] = new Chunk();
                }
                if (sc && (!map.ContainsKey(pos.South()) || map[pos.South()] == null))
                {
                    map[pos.South()] = new Chunk();
                }
                if (swc && (!map.ContainsKey(pos.South().West()) || map[pos.South().West()] == null))
                {
                    map[pos.South().West()] = new Chunk();
                }
                if (wc && (!map.ContainsKey(pos.West()) || map[pos.West()] == null))
                {
                    map[pos.West()] = new Chunk();
                }
                if (nwc && (!map.ContainsKey(pos.North().West()) || map[pos.North().West()] == null))
                {
                    map[pos.North().West()] = new Chunk();
                }
            }

            keys = new ChunkPos[map.Count];
            map.Keys.CopyTo(keys, 0);

            Task.WaitAll(map.Select((KeyValuePair<ChunkPos, Chunk> pair) =>
            {
                ChunkPos pos = pair.Key;

                return Task.Run(() =>
                {

                    // get chunk and neighbors
                    Chunk c = map[pos];
                    Chunk n = null, ne = null, e = null, se = null, s = null, sw = null, w = null, nw = null;
                    if (map.ContainsKey(pos.North()))
                    {
                        n = map[pos.North()];
                    }
                    if (map.ContainsKey(pos.North().East()))
                    {
                        ne = map[pos.North().East()];
                    }
                    if (map.ContainsKey(pos.East()))
                    {
                        e = map[pos.East()];
                    }
                    if (map.ContainsKey(pos.South().East()))
                    {
                        se = map[pos.South().East()];
                    }
                    if (map.ContainsKey(pos.South()))
                    {
                        s = map[pos.South()];
                    }
                    if (map.ContainsKey(pos.South().West()))
                    {
                        sw = map[pos.South().West()];
                    }
                    if (map.ContainsKey(pos.West()))
                    {
                        w = map[pos.West()];
                    }
                    if (map.ContainsKey(pos.North().West()))
                    {
                        nw = map[pos.North().West()];
                    }

                    // simulate chunk
                    deleteMap[pos] = !c.Simulate(n, ne, e, se, s, sw, w, nw, null);
                });
            }).ToArray());
            
            keys = new ChunkPos[map.Count];
            map.Keys.CopyTo(keys, 0);
            foreach (ChunkPos pos in keys)
            {
                // check if the chunk needs to be deleted
                if (deleteMap[pos])
                {
                    Console.WriteLine(pos + " deleted");
                    map.Remove(pos);
                    deleteMap.Remove(pos);
                } else
                {
                    // swap buffers
                    map[pos].SwapBuffers();
                }
            }
        }

        public Chunk GetChunk(ChunkPos pos)
        {
            return map.ContainsKey(pos) ? map[pos] : null;
        }

        public void AddChunk(ChunkPos pos)
        {
            map[pos] = new Chunk();
        }

        public int GetChunkCount()
        {
            return map.Count;
        }

        public Dictionary<ChunkPos, Chunk> GetChunks()
        {
            return map;
        }

    }

}