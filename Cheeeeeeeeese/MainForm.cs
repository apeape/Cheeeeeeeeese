using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Threading;
using Cheeeeeeeeese.Util;

namespace Cheeeeeeeeese
{
    public partial class MainForm : Form
    {
        Bot bot;

        public MainForm()
        {
            InitializeComponent();

            Win32.AllocConsole();

            Console.Title = "Bot Console";
            Console.WindowHeight = 25;
            Console.CursorVisible = false;

            IPEndPoint en2 = new IPEndPoint(IPAddress.Parse(Bot.ServerEn2), Bot.ServerPort);
            bot.AddPlayer("user", "pass", en2);

            bot.StartAll();
        }
    }
}
