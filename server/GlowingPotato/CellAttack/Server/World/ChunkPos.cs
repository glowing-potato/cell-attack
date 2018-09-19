namespace GlowingPotato.CellAttack.Server.World
{
    public class ChunkPos
    {

        private long x;
        private long y;

        public ChunkPos(long x, long y)
        {
            this.x = x;
            this.y = y;
        }

        public long X { get => x; set => x = value; }

        public long Y { get => y; set => y = value; }

        public ChunkPos North()
        {
            return new ChunkPos(x, y - 1);
        }

        public ChunkPos East()
        {
            return new ChunkPos(x + 1, y);
        }

        public ChunkPos South()
        {
            return new ChunkPos(x, y + 1);
        }

        public ChunkPos West()
        {
            return new ChunkPos(x - 1, y);
        }

        public override bool Equals(object obj)
        {
            if (obj is ChunkPos)
            {
                ChunkPos p = (ChunkPos)obj;
                return p.x == x && p.y == y;
            }
            return false;
        }

        public override string ToString()
        {
            return "ChunkPos[x=" + x + ",y=" + y + "]";
        }

        public override int GetHashCode()
        {
            int prime = 31;
            long hash = 1;
            hash = hash * prime + x;
            hash = hash * prime + y;
            return (int)hash;
        }

    }
}