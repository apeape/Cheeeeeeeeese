using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Reflection;

namespace Cheeeeeeeeese
{
    public class Bot
    {
        public const string ServerEn1 = "91.121.157.83";
        public const string ServerEn2 = "91.121.116.13";
        public const int ServerPort = 44444;

        public IPEndPoint Server { get; set; }
        public List<Player> Players { get; set; }
        public bool Connected { get; set; }

        public Bot(IPEndPoint server)
        {
            Server = server;
        }

        public void AddPlayer(string username, string password)
        {
            Players.Add(new Player(username, password, Server));
        }

        public void StartAll()
        {
            // start each player in its own thread
            Players.ForEach(player => new Thread(delegate() { player.Run(); }).Start());
        }


    }
}
