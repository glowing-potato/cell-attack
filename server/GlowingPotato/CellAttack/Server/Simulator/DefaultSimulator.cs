using System;
using System.Collections.Generic;
using System.Text;
using GlowingPotato.CellAttack.Server.World;

namespace GlowingPotato.CellAttack.Server.Simulator
{
    public class DefaultSimulator : ISimulator
    {
        public void Simulate(World.World world)
        {
            world.Simulate();
        }
    }
}
