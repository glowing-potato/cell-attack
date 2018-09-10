using System;

namespace GlowingPotato.CellAttack.Server.Net
{

    public interface IClientProxy
    {

        void SendConnectPacket(byte code);

        void SendSpawnPacket(byte color, int centerX, int centerY);

        void SendFieldDataPacket(int leftX, int topY, short width, short height, byte[] data);

        void SendCellUpdatePacket(float cells, float score);

        void GetConnectPacket(out string name);

        void GetScreenSizePacket(out int leftX, out int topY, out short width, out short height);

        void GetClientDrawPacket(out int leftX, out int topY, out short width, out short height, out byte[] data);

    }

}
