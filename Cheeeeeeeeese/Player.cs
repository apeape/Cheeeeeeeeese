using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Cheeeeeeeeese.Util;
using System.IO;
using System.Reflection;

namespace Cheeeeeeeeese
{
    public class Player
    {
        public SortedList<BotMessage.Type, MethodInfo> MessageHandlers;

        public const int timeout = 25000;
        public const int bufferSize = 65536;

        public const string DefaultRoom = "flowerbed";

        private TcpClient TcpClient;
        private NetworkStream NetStream;
        byte[] recvBuf = new byte[bufferSize];

        public bool Connected { get; set; }
        public IPEndPoint Server { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int UserId { get; set; }
        public int Level { get; set; }

        public Player(string username, string password, IPEndPoint server)
        {
            this.Username = username;
            this.Password = password;
            this.Server = server;

            if (MessageHandlers == null) SetupMessageHandlers();
        }

        public void SetupMessageHandlers()
        {
            MessageHandlers = new SortedList<BotMessage.Type, MethodInfo>();

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

        public void Run()
        {
            try
            {
                TcpClient = new TcpClient(Server.Address.ToString(), Server.Port);
            }
            catch (Exception e)
            {
                Console.WriteLine(Username + " failed to connect to " + Server.Address + ":" + Server.Port);
                Console.WriteLine(e.Message);
                return;
            }

            Console.WriteLine(Username + " connected to " + Server.Address + ":" + Server.Port);
            Connected = true;

            NetStream = TcpClient.GetStream();
            NetStream.ReadTimeout = timeout;
            NetStream.WriteTimeout = timeout;

            SendVersion();

            NetStream.BeginRead(recvBuf, 0, recvBuf.Length, new AsyncCallback(Receive), this);
        }

        public void Receive(IAsyncResult ar)
        {
            // Read data from the remote device.
            int bytesRead = NetStream.EndRead(ar);

            if (bytesRead > 2)
            {
                var packets = recvBuf.SplitBytes(0);
                foreach (byte[] packet in packets)
                {
                    // read packet type
                    short type = BitConverter.ToInt16(packet, 0);

                    // find the message handler method for this message type
                    var handler = MessageHandlers[(BotMessage.Type)type];

                    var splitPacket = recvBuf.Skip(2).ToUTF8().Split(new string[] {"\x01"}, StringSplitOptions.None);

                    if (handler != null)
                    {
                        // found a handler, invoke it
                        Console.WriteLine("received " + Enum.GetName(typeof(BotMessage.Type), type));
                        handler.Invoke(this, new object[] { splitPacket });
                    }
                    else
                    {
                        Console.WriteLine("received unknown packet type " + type.ToString("{0:x2}"));
                        // invoke default handler
                        MessageHandlers[BotMessage.Type.Default].Invoke(this, new object[] { splitPacket });
                    }
                }
            }

            // continue reading
            NetStream.BeginRead(recvBuf, 0, recvBuf.Length, new AsyncCallback(Receive), ar);
        }

        public void Send(OutgoingMessage.Type type, byte[] data)
        {
            var buffer = BitConverter.GetBytes((short)type).Concat(data);
            Send(buffer.ToArray());
        }

        public void Send(byte[] data)
        {
            NetStream.Write(data, 0, data.Length);
        }

        [BotMessageHandler(BotMessage.Type.On420)]
        public void On420(List<string> data)
        {
            Send(OutgoingMessage.Type.Four20, null);
        }

        [BotMessageHandler(BotMessage.Type.OnRoomStart)]
        public void OnRoomStart(List<string> data)
        {

        }

        [BotMessageHandler(BotMessage.Type.OnRoomNoWin)]
        public void OnRoomNoWin(List<string> data)
        {
            Console.WriteLine("Can't win yet");
        }

        [BotMessageHandler(BotMessage.Type.OnRoomGotCheese)]
        public void OnRoomGotCheese(List<string> data)
        {
            int id = Int32.Parse(data[0]);
            if (id == UserId)
            {
                Console.WriteLine("GOT DA CHEEZ!");
                Send(OutgoingMessage.Type.GotCheese, "0".ToByteArray());
            }
        }

        [BotMessageHandler(BotMessage.Type.OnRoomJoin)]
        public void OnRoomJoin(List<string> data)
        {

        }

        [BotMessageHandler(BotMessage.Type.OnRoomPlayers)]
        public void OnRoomPlayers(List<string> data)
        {
            Console.WriteLine("players: " + Username + ", " + String.Join(", ", data.ToArray()));
        }

        [BotMessageHandler(BotMessage.Type.OnRoomTransform)]
        public void OnRoomTransform(List<string> data)
        {

        }

        [BotMessageHandler(BotMessage.Type.OnRoomSync)]
        public void OnRoomSync(List<string> data)
        {

        }

        [BotMessageHandler(BotMessage.Type.OnUserLogin)]
        public void OnUserLogin(List<string> data)
        {
            UserId = Int32.Parse(data[1]);
        }

        [BotMessageHandler(BotMessage.Type.OnPing)]
        public void OnPing(List<string> data)
        {

        }

        [BotMessageHandler(BotMessage.Type.OnVersion)]
        public void OnVersion(List<string> data)
        {
            Console.WriteLine(Username + ": " + data[0] + " players");
            SendLogin(DefaultRoom);
        }

        [BotMessageHandler(BotMessage.Type.Default)]
        public void Default(List<string> data)
        {

        }

        public void SendVersion()
        {
            Send(Bot.Version.ToByteArray());
        }

        public bool SendLogin()
        {
            return SendLogin(DefaultRoom);
        }

        public bool SendLogin(string room)
        {
            List<byte> packet = new List<byte>();

            packet.AddRange(Username.ToByteArray());

            if (!String.IsNullOrEmpty(Password))
                packet.AddRange(Crypto.SHA256String(Password).ToByteArray());
            else packet.Add(0); // todo: verify this is necessary

            packet.AddRange(room.ToByteArray());

            Send(OutgoingMessage.Type.Login, packet.ToArray());

            return true;
        }

        public void Tick()
        {
        }
    }
}
