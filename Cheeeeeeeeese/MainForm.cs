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
using System.Diagnostics;

namespace Cheeeeeeeeese
{
    public partial class MainForm : Form
    {
        Bot bot = new Bot();

        public MainForm()
        {
            InitializeComponent();

            Win32.AllocConsole();

            Console.Title = "Bot Console";
            Console.WindowHeight = 25;
            Console.CursorVisible = false;

            ServerComboBox.Items.Add(Bot.ServerEn1);
            ServerComboBox.Items.Add(Bot.ServerEn2);
            ServerComboBox.SelectedIndex = 1;

            RoomTxt.Text = Player.DefaultRoom;
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(UsernameTxt.Text)) return;

            IPEndPoint server = new IPEndPoint(IPAddress.Parse(ServerComboBox.SelectedItem.ToString()), Bot.ServerPort);
            bot.AddPlayer(UsernameTxt.Text, PasswordTxt.Text, RoomTxt.Text, server);

            bot.StartAll();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Bot.Running = false;
        }
    }
}
