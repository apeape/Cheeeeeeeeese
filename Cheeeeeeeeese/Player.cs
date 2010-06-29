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

            NetStream.BeginRead(recvBuf, 0, recvBuf.Length, new AsyncCallback(Receive), this);
        }

        public void Receive(IAsyncResult ar)
        {
            // Read data from the remote device.
            int bytesRead = NetStream.EndRead(ar);

            if (bytesRead > 2)
            {
                var packets = recvBuf.SplitBytes();
                foreach (byte[] packet in packets)
                {
                    // read packet type
                    short type = BitConverter.ToInt16(packet, 0);

                    // find the message handler method for this message type
                    var handler = MessageHandlers[(BotMessage.Type)type];

                    if (handler != null)
                    {
                        // found a handler, invoke it
                        Console.WriteLine("received " + Enum.GetName(typeof(BotMessage.Type), type));
                        handler.Invoke(this, new object[] { recvBuf.Skip(2) });
                    }
                    else
                    {
                        Console.WriteLine("received unknown packet type " + type.ToString("{0:x2}"));
                        // invoke default handler
                        MessageHandlers[BotMessage.Type.Default].Invoke(this, new object[] { recvBuf.Skip(2) });
                    }
                }
            }

            // continue reading
            NetStream.BeginRead(recvBuf, 0, recvBuf.Length, new AsyncCallback(Receive), ar);
        }

        public void Send(BotMessage.Type type, byte[] data)
        {
            var buffer = BitConverter.GetBytes((short)type).Concat(data);
            NetStream.Write(buffer.ToArray(), 0, buffer.Count());
        }

        [BotMessageHandler(BotMessage.Type.On420)]
        public void On420(NetworkStream NetStream, byte[] data)
        {

        }

        [BotMessageHandler(BotMessage.Type.OnRoomStart)]
        public void OnRoomStart(NetworkStream NetStream, byte[] data)
        {

        }

        [BotMessageHandler(BotMessage.Type.OnRoomNoWin)]
        public void OnRoomNoWin(NetworkStream NetStream, byte[] data)
        {

        }

        [BotMessageHandler(BotMessage.Type.OnRoomGotCheese)]
        public void OnRoomGotCheese(NetworkStream NetStream, byte[] data)
        {

        }

        [BotMessageHandler(BotMessage.Type.OnRoomJoin)]
        public void OnRoomJoin(NetworkStream NetStream, byte[] data)
        {

        }

        [BotMessageHandler(BotMessage.Type.OnRoomPlayers)]
        public void OnRoomPlayers(NetworkStream NetStream, byte[] data)
        {

        }

        [BotMessageHandler(BotMessage.Type.OnRoomTransform)]
        public void OnRoomTransform(NetworkStream NetStream, byte[] data)
        {

        }

        [BotMessageHandler(BotMessage.Type.OnRoomSync)]
        public void OnRoomSync(NetworkStream NetStream, byte[] data)
        {

        }

        [BotMessageHandler(BotMessage.Type.OnUserLogin)]
        public void OnUserLogin(NetworkStream NetStream, byte[] data)
        {

        }

        [BotMessageHandler(BotMessage.Type.OnPing)]
        public void OnPing(NetworkStream NetStream, byte[] data)
        {

        }

        [BotMessageHandler(BotMessage.Type.OnVersion)]
        public void OnVersion(NetworkStream NetStream, byte[] data)
        {

        }

        [BotMessageHandler(BotMessage.Type.Default)]
        public void Default(NetworkStream NetStream, byte[] data)
        {

        }
    }
}
