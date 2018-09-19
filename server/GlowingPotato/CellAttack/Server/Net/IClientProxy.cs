using System;

namespace GlowingPotato.CellAttack.Server.Net
{

    public interface IClientProxy
    {

        void SendConnectPacket(byte code);

        void SendSpawnPacket(byte color, int centerX, int centerY);

        void SendFieldDataPacket(int leftX, int topY, short width, short height, byte[] data);

        void SendCellUpdatePacket(float cells, float score);

        string Name { get; set; }
        int ScreenLeftX { get; set; }
        int ScreenTopY { get; set; }
        short ScreenWidth { get; set; }
        short ScreenHeight { get; set; }
        int DrawLeftX { get; set; }
        int DrawTopY { get; set; }
        short DrawWidth { get; set; }
        short DrawHeight { get; set; }
        byte[] DrawData { get; set; }

    }

}
