using System;

namespace GlowingPotato.CellAttack.Server.Simulator
{

    public interface IWorldInterface
    {

        void GenerateChunk(long xoff, long yoff);

    }
}