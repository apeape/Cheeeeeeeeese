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
using Starksoft.Net.Proxy;

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

            ProxyTypeComboBox.Items.AddRange(Enum.GetNames(typeof(ProxyType)));
            ProxyTypeComboBox.SelectedIndex = 4;

            RoomTxt.Text = Player.DefaultRoom;

            VersionTxt.Text = Bot.Version;
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(UsernameTxt.Text)) return;

            if (ProxyCheckBox.Checked && !String.IsNullOrEmpty(ProxyTxt.Text))
            {
                // try to get the proxy ip/port
                try
                {
                    string[] s = ProxyTxt.Text.Split(':');
                    IPAddress address = Dns.GetHostEntry(s[0]).AddressList[0];
                    IPEndPoint proxyAddress = new IPEndPoint(address, Int32.Parse(s[1]));

                    bot.Players.Add(new Player(UsernameTxt.Text, PasswordTxt.Text, RoomTxt.Text, VersionTxt.Text, (IPEndPoint)ServerComboBox.SelectedItem, proxyAddress, (ProxyType)ProxyType.Parse(typeof(ProxyType), ProxyTypeComboBox.Text)));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error parsing proxy: " + ex.Message);
                }
            }
            else 
                bot.Players.Add(new Player(UsernameTxt.Text, PasswordTxt.Text, RoomTxt.Text, VersionTxt.Text, (IPEndPoint)ServerComboBox.SelectedItem));

            bot.StartAll();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Bot.Running = false;
        }

        private void UsernameTxt_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsLetter(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void UsernameTxt_TextChanged(object sender, EventArgs e)
        {
            var cursorPos = UsernameTxt.SelectionStart;
            foreach (char i in UsernameTxt.Text)
            {
                if (!char.IsLetter(i))
                {
                    UsernameTxt.Text = UsernameTxt.Text.Replace(i.ToString(), "");
                }
            }
            UsernameTxt.SelectionStart = cursorPos;
        }
    }
}
