using System.Collections.Generic;

namespace GlowingPotato.CellAttack.Server.Simulator
{

    public class World
    {

        private Dictionary<ChunkPos, Chunk> map = new Dictionary<ChunkPos, Chunk>();
        private IWorldInterface worldInterface;

        public World()
        {

        }

        public void Simulate()
        {
            foreach (ChunkPos pos in map.Keys)
            {
                // get chunk and neighbors
                Chunk c = map[pos];
                Chunk n = null;//map[pos.North()];
                Chunk ne = null;//map[pos.North().East()];
                Chunk e = null;//map[pos.East()];
                Chunk se = null;//map[pos.South().East()];
                Chunk s = null;//map[pos.South()];
                Chunk sw = null;//map[pos.South().West()];
                Chunk w = null;//map[pos.West()];
                Chunk nw = null;//map[pos.North().West()];

                // simulate chunk
                bool simulate = c.Simulate(n, ne, e, se, s, sw, w, nw, null);

                // if chunk is empty, delete it
                if (!simulate)
                {
                    map.Remove(pos);
                }
            }
            foreach (ChunkPos pos in map.Keys)
            {
                // swap buffers
                map[pos].SwapBuffers();
            }
        }

        public Chunk GetChunk(ChunkPos pos)
        {
            return map[pos];
        }

        public void AddChunk(ChunkPos pos)
        {
            map[pos] = new Chunk();
        }

    }

}