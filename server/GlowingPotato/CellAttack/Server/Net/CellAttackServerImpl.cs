using Fleck;
using GlowingPotato.CellAttack.Server.Simulator;
using System;
using System.Collections.Generic;
using System.Text;

namespace GlowingPotato.CellAttack.Server.Net
{
    public class CellAttackServerImpl : CellAttackServerBase
    {

        public CellAttackServerImpl(ISimulator simulator, WebSocketServer server) : base(simulator, server)
        {

        }

        public override void Tick()
        {
            Simulator.Simulate(World);
        }
    }
}
