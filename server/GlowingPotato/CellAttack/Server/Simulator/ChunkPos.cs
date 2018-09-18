namespace GlowingPotato.CellAttack.Server.Simulator
{
    public class ChunkPos
    {

        public ChunkPos(long x, long y)
        {
            X = x;
            Y = y;
        }

        public long X { get; set; }

        public long Y { get; set; }

        public ChunkPos North()
        {
            return new ChunkPos(X, Y - 1);
        }

        public ChunkPos East()
        {
            return new ChunkPos(X + 1, Y);
        }

        public ChunkPos South()
        {
            return new ChunkPos(X, Y + 1);
        }

        public ChunkPos West()
        {
            return new ChunkPos(X - 1, Y);
        }

        public override bool Equals(object obj)
        {
            if (obj is ChunkPos)
            {
                ChunkPos p = (ChunkPos)obj;
                return p.X == X && p.Y == Y;
            }
            return false;
        }

        public override string ToString()
        {
            return "ChunkPos[x=" + X + ",y=" + Y + "]";
        }

        public override int GetHashCode()
        {
            int prime = 31;
            long hash = 1;
            hash = hash * prime + X;
            hash = hash * prime + Y;
            return (int)hash;
        }

    }
}