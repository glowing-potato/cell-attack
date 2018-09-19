using System;
using System.Collections.Generic;
using System.Text;

namespace GlowingPotato.CellAttack.Server.Simulator
{
    public interface ISimulator
    {

        void Simulate(World.World world);

    }
}
