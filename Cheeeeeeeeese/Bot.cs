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
        public const string Version = "0.30\0";
        public const string ServerEn1 = "91.121.157.83";
        public const string ServerEn2 = "91.121.116.13";
        public const int ServerPort = 44444;

        public List<Player> Players { get; set; }
        public bool Connected { get; set; }

        public void AddPlayer(string username, string password, IPEndPoint server)
        {
            Players.Add(new Player(username, password, server));
        }

        public void StartAll()
        {
            // start each player in its own thread
            Players.ForEach(player => new Thread(delegate() { player.Run(); }).Start());
        }
    }
}
