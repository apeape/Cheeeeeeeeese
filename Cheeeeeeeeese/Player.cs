using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Cheeeeeeeeese.Util;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Cheeeeeeeeese
{
    public class Player
    {
        #region Constants/Variables

        public const int DefaultDelay = 500;
        public const int Timeout = 10000;
        public const int BufferSize = 65536;
        public const string DefaultRoom = "flowerbed";

        public SortedList<IncomingMessage.Type, MethodInfo> MessageHandlers;
        private TcpClient TcpClient;
        private NetworkStream NetStream;
        private byte[] RecvBuf = new byte[BufferSize];

        public bool Started { get; set; }
        public bool Connected { get; set; }
        public IPEndPoint Server { get; set; }
        public string Version { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int UserId { get; set; }
        public int UserLevel { get; set; }
        public string CurrentRoom { get; set; }
        public List<string> CurrentPlayers { get; set; }
        public List<string> CurrentShamans { get; set; }
        public ulong Cheese { get; set; }

        public DateTime SendPing { get; set; }
        public DateTime SendWin { get; set; }
        public DateTime LastPing { get; set; }

        private Random rand = new Random();
        #endregion

        public Player(string username, string password, string room, string version, IPEndPoint server)
        {
            Started = false;
            Connected = false;
            this.Username = username;
            this.Password = password;
            this.CurrentRoom = room;
            this.Server = server;
            this.Version = version;

            if (MessageHandlers == null) SetupMessageHandlers();

            Thread.CurrentThread.IsBackground = true;
        }

        public void Run()
        {
            Run(DefaultDelay);
        }

        public void Run(int Delay)
        {
            Started = true;

            DateTime startTime = DateTime.Now;

            Connect();

            while (Bot.Running)
            {
                if (!Connected)
                {
                    if ((DateTime.Now - startTime).TotalMilliseconds >= Player.Timeout)
                    {
                        Console.WriteLine(Username + ": Timed out while trying to connect");
                        break;
                    }
                    Thread.Sleep(Delay);
                }

                else
                {
                    Tick();
                    Thread.Sleep(Delay);
                }
            }

            Console.WriteLine(Username + ": Got exit signal");
            Connected = false;
        }

        public bool Connect()
        {
            try
            {
                TcpClient = Retry.RetryAction<TcpClient>(() =>
                {
                    return new TcpClient(Server.Address.ToString(), Server.Port);
                }, 10, 1000);
            }
            catch (Exception e)
            {
                Console.WriteLine(Username + ": Failed to connect to " + Server.Address + ":" + Server.Port);
                return false;
            }

            Console.WriteLine(Username + ": connected to " + Server.Address + ":" + Server.Port);

            NetStream = TcpClient.GetStream();
            NetStream.ReadTimeout = Timeout;
            NetStream.WriteTimeout = Timeout;

            SendVersion();
            SendPing = DateTime.MinValue;
            LastPing = DateTime.MinValue;
            SendWin = DateTime.MinValue;

            NetStream.BeginRead(RecvBuf, 0, RecvBuf.Length, new AsyncCallback(Receive), this);

            return true;
        }

        public void Tick()
        {
            var now = DateTime.Now;
            if (LastPing == DateTime.MinValue) LastPing = now;

            var elapsed = (now - LastPing).TotalSeconds;
            if (elapsed >= 10)
            {
                Send(OutgoingMessage.Type.CheckInventory);
                Send(OutgoingMessage.Type.PingTime, (elapsed * 1000).ToString().ToByteArray());
                LastPing = now;
            }

            if (SendPing != DateTime.MinValue && (now - SendPing).TotalSeconds >= 10)
            {
                Send(OutgoingMessage.Type.Ping);
                SendPing = DateTime.MinValue;
            }

            if (SendWin != DateTime.MinValue && (now - SendWin).TotalSeconds >= 1)
            {
                SendWin = DateTime.MinValue;
                Send(OutgoingMessage.Type.Win, new byte[] { 0 });
            }
        }

        #region Send/Receive

        public void Receive(IAsyncResult ar)
        {
            // Read data from the remote device.
            int bytesRead = NetStream.EndRead(ar);

            if (bytesRead > 2)
            {
                var packets = RecvBuf.SplitBytes(0, bytesRead);
                foreach (byte[] packet in packets)
                {
                    // read packet
                    short type = BitConverter.ToInt16(packet, 0);
                    var splitPacket = RecvBuf.Skip(3).SplitStrings(Message.Delimiter, bytesRead - 3);

                    // find the message handler method for this message type
                    if (MessageHandlers.ContainsKey((IncomingMessage.Type)type))
                    {
                        // found a handler, invoke it
                        var handler = MessageHandlers[(IncomingMessage.Type)type];
                        
                        //Console.WriteLine(Username + ": received " + Enum.GetName(typeof(IncomingMessage.Type), type));
                        handler.Invoke(this, new object[] { splitPacket });
                    }
                    // ignored message type
                    else if (IgnoredMessage.Type.IsDefined(typeof(IgnoredMessage.Type), type))
                    {
                        // do nothing for now
                        //Console.WriteLine(Username + ": received ignored packet type " + type.ToString("{0:x2}"));
                    }
                    else // no handler
                    {
                        Console.WriteLine(Username + ": received unknown packet type " + RecvBuf[0] + ", " + RecvBuf[1]);
                        // invoke default handler
                        MessageHandlers[IncomingMessage.Type.Default].Invoke(this, new object[] { splitPacket });
                    }
                }
            }

            try
            {
                // continue reading
                NetStream.BeginRead(RecvBuf, 0, RecvBuf.Length, new AsyncCallback(Receive), ar);
            }
            catch (Exception e)
            {
                Console.WriteLine(Username + ": Got disconnected");
                Connected = false;
                Connect();
            }
        }

        public void Send(OutgoingMessage.Type type)
        {
            Send(BitConverter.GetBytes((short)type));
        }

        public void Send(OutgoingMessage.Type type, params object[] args)
        {
            //Console.WriteLine(Username + ": Sending " + Enum.GetName(typeof(OutgoingMessage.Type), type));

            // this code is baby shit
            List<byte[]> args2 = new List<byte[]>();
            args2.Add(BitConverter.GetBytes((short)type));
            foreach (byte[] arg in args)
                args2.Add(arg);

            Send(args2.ToArray());
        }

        public void Send(params object[] args)
        {
            List<byte> data = new List<byte>();

            if (args.Count() == 1)
                data.AddRange((byte[])args[0]); // don't add delimiter
            else
            {
                for (int i = 0; i < args.Length; i++)
                {
                    data.AddRange((byte[])args[i]);
                    if (i < args.Length - 1)
                        data.Add(Message.Delimiter);
                }
            }
            data.Add(Message.End);

            NetStream.Write(data.ToArray(), 0, data.Count());
        }
        #endregion

        #region Message Handlers
        [BotMessageHandler(IncomingMessage.Type.OnInventory)]
        public void OnInventory(List<string> data)
        {
            try
            {
                ulong oldcheese = Cheese;
                Cheese = ulong.Parse(data[0]);
                if (Cheese > oldcheese)
                    Console.WriteLine(Username + ": Current Cheese: " + Cheese);
            }
            catch (Exception)
            {
                Console.WriteLine(Username + ": Error reading cheese value");
            }
        }

        [BotMessageHandler(IncomingMessage.Type.On420)]
        public void On420(List<string> data)
        {
            Send(OutgoingMessage.Type.Four20);
        }

        [BotMessageHandler(IncomingMessage.Type.OnRoomStart)]
        public void OnRoomStart(List<string> data)
        {
            //CleanNames(data);
            //Console.WriteLine(Username + ": start " + String.Join(", ", data.ToArray()));
            Console.WriteLine(Username + ": New round starting");
        }

        [BotMessageHandler(IncomingMessage.Type.OnRoomNoWin)]
        public void OnRoomNoWin(List<string> data)
        {
            Console.WriteLine(Username + ": Can't win yet");
            SendWin = DateTime.Now;
        }

        [BotMessageHandler(IncomingMessage.Type.OnRoomGotCheese)]
        public void OnRoomGotCheese(List<string> data)
        {
            int id = Int32.Parse(data[0]);
            if (id == UserId)
            {
                Console.WriteLine(Username + ": I GOT DA CHEEZ!");
                Send(OutgoingMessage.Type.EnterHole, "0".ToByteArray());
            }
        }

        [BotMessageHandler(IncomingMessage.Type.OnRoomJoin)]
        public void OnRoomJoin(List<string> data)
        {
            CurrentRoom = data[0];
            Console.WriteLine(Username + ": joined room " + CurrentRoom);
        }

        [BotMessageHandler(IncomingMessage.Type.OnRoomPlayers)]
        public void OnRoomPlayers(List<string> data)
        {
            CurrentPlayers = data;
            var cleaned = CleanNames(data.Where(p => p.Contains("#")).ToList());
            Console.WriteLine(Username + ": got player list: " + String.Join(", ", cleaned.ToArray()));
        }

        [BotMessageHandler(IncomingMessage.Type.OnRoomTransform)]
        public void OnRoomTransform(List<string> data)
        {
            // this is totally not working, wtf
            /*
            Console.WriteLine(Username + ": Transform: " + String.Join(", ", data.ToArray()));
            try
            {
                if (data.Count == 1) // single shaman
                {
                    Console.WriteLine(Username + ": Shaman: " + GetPlayerName(data[0]));
                }
                else
                {
                    Console.WriteLine(Username + ": Shamans: " + GetPlayerName(data[0]) + ", " + GetPlayerName(data[1]));
                }
            }
            catch (Exception)
            {
                Console.WriteLine(Username + ": Error reading shamans");
            }
            */
        }

        [BotMessageHandler(IncomingMessage.Type.OnRoomSync)]
        public void OnRoomSync(List<string> data)
        {
            if (CurrentPlayers.Count > 1)
            {
                //Thread.Sleep(rand.Next(3000, 7000));
                Send(OutgoingMessage.Type.GrabCheese);
            }
            else
                Console.WriteLine(Username + ": All alone, can't get any cheese =(");
        }

        [BotMessageHandler(IncomingMessage.Type.OnUserLogin)]
        public void OnUserLogin(List<string> data)
        {
            UserId = Int32.Parse(data[1]);
            UserLevel = Int32.Parse(data[2]);
            Console.WriteLine(data[0] + " logged in, " + UserId + ", " + UserLevel);
            Connected = true;
        }

        [BotMessageHandler(IncomingMessage.Type.OnPing)]
        public void OnPing(List<string> data)
        {
            SendPing = DateTime.Now;
        }

        [BotMessageHandler(IncomingMessage.Type.OnVersion)]
        public void OnVersion(List<string> data)
        {
            Console.WriteLine(Username + ": " + data[0] + " players currently online");
            SendLogin();
        }

        [BotMessageHandler(IncomingMessage.Type.OnChat)]
        public void OnChat(List<string> data)
        {
            Console.WriteLine(Username + ": Incoming Chat Msg: " + data[0]);
        }

        [BotMessageHandler(IncomingMessage.Type.OnLoginError)]
        public void OnLoginError(List<string> data)
        {
            if (data.Count == 0)
            {
                Console.WriteLine(Username + " failed to log in: Nickname already taken (???)");
            }
            else if (data.Count == 1)
            {
                Console.WriteLine(Username + " failed to log in: Invalid Password " + String.Join(", ", data.ToArray()));
            }
            else if (data.Count == 2)
            {
                Console.WriteLine(Username + " failed to log in: Already logged in " + String.Join(", ", data.ToArray()));
            }
        }

        [BotMessageHandler(IncomingMessage.Type.OnBanned)]
        public void OnBanned(List<string> data)
        {
            try
            {
                var duration = Math.Floor(Decimal.Parse(data[0]) / 3600000);
                if (duration == 0)
                {
                    Console.WriteLine(Username + ": Banned for the following reason: " + data[1]);
                }
                else
                {
                    Console.WriteLine(Username + ": Banned for " + duration + " hours for the following reason: " + data[1]);
                }
            }
            catch (Exception)
            {
                Console.WriteLine(Username + ": Error parsing ban packet");
            }
            //var duration = Math.Floor((Number(_local3[1]) / 3600000));
        }

        [BotMessageHandler(IncomingMessage.Type.OnPermaBanned)]
        public void OnPermaBanned(List<string> data)
        {
            try
            {
                if (data.Count == 0)
                    Console.WriteLine(Username + ": Perma-banned =(");
                else if (data.Count == 1)
                    Console.WriteLine(Username + ": Already banned for " + data[1] + " hours, if you are ever banned for >24 hours total, your account/mouse will be deleted automatically (OH NOES)");
                else
                {
                    var duration = Math.Floor(Decimal.Parse(data[0]) / 3600000);
                    Console.WriteLine(Username + ": Banned for " + duration + " hours for the following reason: " + data[1]);
                }
            }
            catch (Exception)
            {
                Console.WriteLine(Username + ": Error parsing ban packet");
            }
            //var duration = Math.Floor((Number(_local3[1]) / 3600000));
        }

        [BotMessageHandler(IncomingMessage.Type.Default)]
        public void Default(List<string> data)
        {

        }
        #endregion

        public void SendVersion()
        {
            Console.WriteLine(Username + ": Sending version " + Version);
            Send(Bot.Version.ToByteArray());
        }

        public bool SendLogin()
        {
            return SendLogin(CurrentRoom);
        }

        public bool SendLogin(string room)
        {
            byte[] pass =
                (Password == null) ? new byte[] { } : Crypto.SHA256String(Password).ToByteArray();
            Send(OutgoingMessage.Type.Login, Username.ToByteArray(), pass, room.ToByteArray());

            return true;
        }

        public string GetPlayerName(string id)
        {
            id = CleanName(id);
            try
            {
                return CleanName(CurrentPlayers.First(chunk => chunk.Contains(id))).Replace("*", "");
            }
            catch (Exception)
            {
                return "Unknown";
            }
        }

        public string CleanName(string name)
        {
            return name.Substring(0, name.IndexOf("#"));
        }
        public List<string> CleanNames(List<string> names)
        {
            for (int i = 0; i < names.Count; i++)
            {
                if (names[i].Contains("#"))
                    names[i] = CleanName(names[i]);
            }

            return names;
        }

        public void SetupMessageHandlers()
        {
            MessageHandlers = new SortedList<IncomingMessage.Type, MethodInfo>();

            // find all methods tagged MessageHandler
            var methods = typeof(Player).GetMethods()
                .Where(q => q.IsDefined(typeof(BotMessageHandlerAttribute), false));

            // store in a lookup based on operation
            foreach (var m in methods)
            {
                var type = ((BotMessageHandlerAttribute)Attribute
                    .GetCustomAttribute(m, typeof(BotMessageHandlerAttribute))).Type;
                MessageHandlers.Add(type, m);
            }
            Console.WriteLine(Username + ": " + MessageHandlers.Count + " message handlers loaded");
        }
    }
}
