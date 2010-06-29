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
        public const int Timeout = 25000;
        public const int bufferSize = 65536;
        public const string DefaultRoom = "3";

        public SortedList<IncomingMessage.Type, MethodInfo> MessageHandlers;
        private TcpClient TcpClient;
        private NetworkStream NetStream;
        private byte[] recvBuf = new byte[bufferSize];

        public bool Started { get; set; }
        public bool Connected { get; set; }
        public IPEndPoint Server { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int UserId { get; set; }
        public int UserLevel { get; set; }
        public string CurrentRoom { get; set; }

        public DateTime SendPing { get; set; }
        public DateTime SendWin { get; set; }
        public DateTime LastPing { get; set; }
        #endregion

        public Player(string username, string password, string room, IPEndPoint server)
        {
            Started = false;
            Connected = false;
            this.Username = username;
            this.Password = password;
            this.CurrentRoom = room;
            this.Server = server;

            if (MessageHandlers == null) SetupMessageHandlers();

            Thread.CurrentThread.IsBackground = true;
        }

        public void Run()
        {
            Run(DefaultDelay);
        }

        public void Run(int Delay)
        {
            try
            {
                TcpClient = new TcpClient(Server.Address.ToString(), Server.Port);
            }
            catch (Exception e)
            {
                Console.WriteLine(Username + ": failed to connect to " + Server.Address + ":" + Server.Port);
                Console.WriteLine(e.Message);
                return;
            }

            Console.WriteLine(Username + ": connected to " + Server.Address + ":" + Server.Port);

            NetStream = TcpClient.GetStream();
            NetStream.ReadTimeout = Timeout;
            NetStream.WriteTimeout = Timeout;

            SendVersion();
            SendPing = DateTime.MinValue;
            LastPing = DateTime.MinValue;
            SendWin = DateTime.MinValue;

            NetStream.BeginRead(recvBuf, 0, recvBuf.Length, new AsyncCallback(Receive), this);

            while (Bot.Running)
            {
                if (!Connected) Thread.Sleep(Delay);

                else
                {
                    Tick();
                    Thread.Sleep(Delay);
                }
            }

            Console.WriteLine(Username + ": Got exit signal");
            Connected = false;
        }

        public void Tick()
        {
            var now = DateTime.Now;
            if (LastPing == DateTime.MinValue) LastPing = now;

            var elapsed = (now - LastPing).TotalSeconds;
            if (elapsed >= 10)
            {
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
                var packets = recvBuf.SplitBytes(0, bytesRead);
                foreach (byte[] packet in packets)
                {
                    // read packet
                    short type = BitConverter.ToInt16(packet, 0);
                    var splitPacket = recvBuf.Skip(3).SplitStrings(Message.Delimiter, bytesRead - 3);

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
                        Console.WriteLine(Username + ": received unknown packet type " + recvBuf[0] + ", " + recvBuf[1]);
                        // invoke default handler
                        MessageHandlers[IncomingMessage.Type.Default].Invoke(this, new object[] { splitPacket });
                    }
                }
            }

            try
            {
                // continue reading
                NetStream.BeginRead(recvBuf, 0, recvBuf.Length, new AsyncCallback(Receive), ar);
            }
            catch (Exception e)
            {
                Console.WriteLine(Username + ": Got disconnected");
                Connected = false;
            }
        }

        public void Send(OutgoingMessage.Type type)
        {
            Send(BitConverter.GetBytes((short)type));
        }

        public void Send(OutgoingMessage.Type type, params object[] args)
        {
            Console.WriteLine(Username + ": Sending " + Enum.GetName(typeof(OutgoingMessage.Type), type));

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
        [BotMessageHandler(IncomingMessage.Type.On420)]
        public void On420(List<string> data)
        {
            Send(OutgoingMessage.Type.Four20);
        }

        [BotMessageHandler(IncomingMessage.Type.OnRoomStart)]
        public void OnRoomStart(List<string> data)
        {
            CleanNames(data);
            Console.WriteLine(Username + ": start " + String.Join(", ", data.ToArray()));
            Send(OutgoingMessage.Type.RoomStart);
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
                Send(OutgoingMessage.Type.GotCheese, "0".ToByteArray());
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
            //CleanNames(data);
            //Console.WriteLine("players: " + String.Join(", ", data.ToArray()));
        }

        [BotMessageHandler(IncomingMessage.Type.OnRoomTransform)]
        public void OnRoomTransform(List<string> data)
        {

        }

        [BotMessageHandler(IncomingMessage.Type.OnRoomSync)]
        public void OnRoomSync(List<string> data)
        {

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

        [BotMessageHandler(IncomingMessage.Type.Default)]
        public void Default(List<string> data)
        {

        }
        #endregion

        public void SendVersion()
        {
            Console.WriteLine(Username + ": Sending version " + Bot.Version);
            Send(Bot.Version.ToByteArray());
        }

        public bool SendLogin()
        {
            return SendLogin(CurrentRoom);
        }

        public bool SendLogin(string room)
        {
            byte[] pass =
                (Password != null) ? new byte[] { } : Crypto.SHA256String(Password).ToByteArray();
            Send(OutgoingMessage.Type.Login, Username.ToByteArray(), pass, room.ToByteArray());

            return true;
        }

        public List<string> CleanNames(List<string> names)
        {
            for (int i = 0; i < names.Count; i++)
            {
                if (names[i].Contains("#"))
                    names[i] = names[i].Substring(0, names[i].IndexOf("#"));
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
