using Fleck;
using System;
using System.Collections.Generic;
using System.Text;

namespace GlowingPotato.CellAttack.Server.Net
{
    class ClientProxyImpl : IClientProxy
    {

        private IWebSocketConnection connection;
        private string name;
        private int screenLeftX;
        private int screenTopY;
        private short screenWidth;
        private short screenHeight;
        private int drawLeftX;
        private int drawTopY;
        private short drawWidth;
        private short drawHeight;
        private byte[] drawData;

        public ClientProxyImpl(IWebSocketConnection connection)
        {
            this.connection = connection;
        }

        public void GetConnectPacket(out string name)
        {
            name = this.name;
        }

        public void GetClientDrawPacket(out int leftX, out int topY, out short width, out short height, out byte[] data)
        {
            leftX = drawLeftX;
            topY = drawTopY;
            width = drawWidth;
            height = drawHeight;
            data = drawData;
        }

        public void GetScreenSizePacket(out int leftX, out int topY, out short width, out short height)
        {
            leftX = screenLeftX;
            topY = screenTopY;
            width = screenWidth;
            height = screenHeight;
        }

        public void SendCellUpdatePacket(float cells, float score)
        {
            byte[] packet = new byte[8];
            BitConverter.GetBytes(cells).CopyTo(packet, 0);
            BitConverter.GetBytes(score).CopyTo(packet, 4);
            connection.Send(packet);
        }

        public void SendConnectPacket(byte code)
        {
            connection.Send(new byte[] { code });
        }

        public void SendFieldDataPacket(int leftX, int topY, short width, short height, byte[] data)
        {
            byte[] packet = new byte[14 + data.Length];
            BitConverter.GetBytes(leftX).CopyTo(packet, 0);
            BitConverter.GetBytes(topY).CopyTo(packet, 4);
            BitConverter.GetBytes(width).CopyTo(packet, 8);
            BitConverter.GetBytes(height).CopyTo(packet, 10);
            data.CopyTo(packet, 12);
            connection.Send(packet);
        }

        public void SendSpawnPacket(byte color, int centerX, int centerY)
        {
            byte[] packet = new byte[8];
            BitConverter.GetBytes(color).CopyTo(packet, 0);
            BitConverter.GetBytes(centerX).CopyTo(packet, 1);
            BitConverter.GetBytes(centerY).CopyTo(packet, 5);
            connection.Send(packet);
        }
    }
}
