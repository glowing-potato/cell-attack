using System;
using System.Collections.Generic;
using System.Text;

namespace GlowingPotato.CellAttack.Server.Net
{
    public sealed class ClientManager
    {

        private List<IClientProxy> clients = new List<IClientProxy>();

        public void AddClient(IClientProxy client)
        {
            clients.Add(client);
        }

        public void RemoveClient(IClientProxy client)
        {
            clients.Remove(client);
        }

        public bool ContainsClient(string name)
        {
            foreach (IClientProxy client in clients)
            {
                if (client.Name == name)
                {
                    return true;
                }
            }
            return false;
        }

        public void ForEach(Action<IClientProxy> action)
        {
            foreach (IClientProxy client in clients)
            {
                action(client);
            }
        }

        public string[] GetClientNames()
        {
            string[] names = new string[clients.Count];
            for (int i = 0; i < clients.Count; ++i)
            {
                names[i] = clients[i].Name;
            }
            return names;
        }

        public int ClientCount
        {
            get
            {
                return clients.Count;
            }
        }

    }
}
