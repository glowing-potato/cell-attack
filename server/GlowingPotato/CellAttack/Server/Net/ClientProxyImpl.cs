using Fleck;
using System;
using System.Collections.Generic;
using System.Text;

namespace GlowingPotato.CellAttack.Server.Net
{
    public class ClientProxyImpl : IClientProxy
    {

        private IWebSocketConnection connection;
        private string name = null;
        private int screenLeftX = 0;
        private int screenTopY = 0;
        private short screenWidth = 0;
        private short screenHeight = 0;
        private int drawLeftX = 0;
        private int drawTopY = 0;
        private short drawWidth = 0;
        private short drawHeight = 0;
        private byte[] drawData = null;

        public ClientProxyImpl(IWebSocketConnection connection)
        {
            this.connection = connection;
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

        public string Name { get => name; set => name = value; }
        public int ScreenLeftX { get => screenLeftX; set => screenLeftX = value; }
        public int ScreenTopY { get => screenTopY; set => screenTopY = value; }
        public short ScreenWidth { get => screenWidth; set => screenWidth = value; }
        public short ScreenHeight { get => screenHeight; set => screenHeight = value; }
        public int DrawLeftX { get => drawLeftX; set => drawLeftX = value; }
        public int DrawTopY { get => drawTopY; set => drawTopY = value; }
        public short DrawWidth { get => drawWidth; set => drawWidth = value; }
        public short DrawHeight { get => drawHeight; set => drawHeight = value; }
        public byte[] DrawData { get => drawData; set => drawData = value; }
    }
}
