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

        public static bool Running { get; set; }

        public Bot()
        {
            Players = new List<Player>();
        }

        public void AddPlayer(string username, string password, string room, string version, IPEndPoint server)
        {
            Players.Add(new Player(username, password, room, version, server));
        }

        public void StartAll()
        {
            Running = true;
            // start each player in its own thread
            List<Player> unstarted = Players.Where(player => !player.Started && !player.Connected).ToList();
            unstarted.ForEach(player => new Thread(delegate() { player.Run(); }).Start());
        }
    }
}
