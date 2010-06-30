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
using Starksoft.Net.Proxy;

namespace Cheeeeeeeeese
{
    public class Player
    {
        #region Constants/Variables

        public const bool ShowOffMode = false;

        public const int DefaultDelay = 500;
        public const int ReconnectDelay = 15000;
        public const int Timeout = 20000;
        public const int BufferSize = 65536;
        public const string DefaultRoom = "fromageville";

        public SortedList<IncomingMessage.Type, MethodInfo> MessageHandlers;
        private TcpClient TcpClient;
        private NetworkStream NetStream;
        private byte[] RecvBuf = new byte[BufferSize];

        public bool Started { get; set; }
        public bool Connected { get; set; }
        private Object ConnectLock = new Object();
        public IPEndPoint Server { get; set; }
        public IPEndPoint Proxy { get; set; }
        public ProxyType ProxyType { get; set; }
        public bool UseProxy { get; set; }
        public string Version { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int UserId { get; set; }
        public int UserLevel { get; set; }
        public string CurrentRoom { get; set; }
        public List<string> CurrentPlayers { get; set; }
        public List<string> PlayersWithoutCheese { get; set; }
        public List<string> CurrentShamans { get; set; }
        public ulong Cheese { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime SendPing { get; set; }
        public DateTime SendWin { get; set; }
        public DateTime LastPing { get; set; }

        private char[] MDT = new char[10];
        private int CMDTEC;

        private Random rand = new Random((int)DateTime.Now.Ticks);

        public enum Objects
        {
            PurpleArrow = 0,
            LittleBox = 1,
            BigBox = 2,
            ShortPlank = 3,
            LongPlank = 4,
            GraySphere = 5,
            BouncyBall = 6,
            Trampoline = 7,
            ShortPurple = 8,
            LongPurple = 9,
            Anvil = 10,
            RedDot = 11,
            RedDotArrow = 12,
            RedDotArrow2 = 13,
            TealDot = 14,
            TealDotArrow = 15,
            TealDotArrow2 = 16,
            UpSphere = 17,
            DownSphere = 18,
            RightSphere = 19,
            LeftSphere = 20,
            GreenSphere = 21,
            YellowDot = 22,
            Bomb = 23,
            Explosion = 24,
            Cheese = 25,
        }



        /*
        0 = purple arrow
        1 = little box
        2 = big box
        3 = short plank
        4 = long plank
        5 = grayscale sphere
        6 = bouncy ball
        7 = bouncy platform
        8 = short purple wire thing
        9 = long purple wire thing
        10 = anvil
        11 = red dot
        12 = red dot with arrow
        13 = red dot with arrow
        14 = teal dot
        15 = teal dot with arrow
        16 = teal dot with arrow
        17 = up sphere
        18 = down sphere
        19 = right sphere
        20 = left sphere
        21 = green sphere
        22 = yellow dot
        23 = bomb
        24 = explosion
        25 = cheese
        */
        #endregion

        public Player(string username, string password, string room, string version, IPEndPoint server, IPEndPoint proxy, ProxyType proxyType) : this(username, password, room, version, server)
        {
            this.Proxy = proxy;
            this.ProxyType = proxyType;
            UseProxy = true;
        }

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

            StartTime = DateTime.Now;

            Connect();

            while (Bot.Running)
            {
                if (!Connected)
                {
                    /*
                    if ((DateTime.Now - StartTime).TotalMilliseconds >= Player.Timeout)
                    {
                        Console.WriteLine(Username + ": Timed out while trying to connect");
                        break;
                    }*/
                    Thread.Sleep(Delay);
                }

                else
                {
                    Tick();
                    Thread.Sleep(Delay);
                    StartTime = DateTime.Now;
                }
            }

            Console.WriteLine(Username + ": Got exit signal");
            Connected = false;
        }

        public void Reconnect()
        {
            if (Connected)
            {
                NetStream.Close();
                Connected = false;
                Thread.Sleep(ReconnectDelay);
                Connect();
            }
        }

        public bool Connect()
        {
            if (Connected) return false;

            lock (ConnectLock)
            {
                StartTime = DateTime.Now;
                Console.WriteLine(Username + ": Connecting...");

                try
                {
                    TcpClient = Retry.RetryAction<TcpClient>(() =>
                    {
                        if (!UseProxy)
                            return new TcpClient(Server.Address.ToString(), Server.Port);
                        else // try to connect through a proxy
                        {
                            // create an instance of the client proxy factory 
                            ProxyClientFactory proxyFactory = new ProxyClientFactory();

                            // use the proxy client factory to generically specify the type of proxy to create 
                            // the proxy factory method CreateProxyClient returns an IProxyClient object 
                            IProxyClient proxyClient = proxyFactory.CreateProxyClient(ProxyType.Socks5, Proxy.Address.ToString(), Proxy.Port);

                            Console.WriteLine(Username + ": Trying to connect using " + Enum.GetName(typeof(ProxyType), ProxyType) + " proxy " + Proxy.Address.ToString() + ":" + Proxy.Port);
                            // create a connection through the proxy to www.starksoft.com over port 80 
                            return proxyClient.CreateConnection(Server.Address.ToString(), Server.Port);
                        }
                    }, 10, 1000);
                }
                catch (Exception e)
                {
                    Console.WriteLine(Username + ": Failed to connect to " + Server.Address + ":" + Server.Port + (UseProxy ? ", likely culprit is a bad proxy server!" : ""));
                    return false;
                }

                Console.WriteLine(Username + ": Connected to " + Server.Address + ":" + Server.Port);

                NetStream = TcpClient.GetStream();
                NetStream.ReadTimeout = Timeout;
                NetStream.WriteTimeout = Timeout;

                Connected = true;

                SendVersion();
                SendPing = DateTime.MinValue;
                LastPing = DateTime.MinValue;
                SendWin = DateTime.MinValue;

                NetStream.BeginRead(RecvBuf, 0, RecvBuf.Length, new AsyncCallback(Receive), this);

                return true;
            }
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
                Console.WriteLine(Username + ": <-Pong");
            }

            if (SendPing != DateTime.MinValue && (now - SendPing).TotalSeconds >= 10)
            {
                Send(OutgoingMessage.Type.Ping);
                SendPing = DateTime.MinValue;
                Console.WriteLine(Username + ": <-Pong2");
            }

            if (SendWin != DateTime.MinValue && (now - SendWin).TotalSeconds >= 1)
            {
                //Send(OutgoingMessage.Type.CheckInventory);
                SendWin = DateTime.MinValue;
                Console.WriteLine(Username + ": Sending Win");
                Send(OutgoingMessage.Type.Win, new byte[] { 0 });
            }
        }

        #region Send/Receive

        public void Receive(IAsyncResult ar)
        {
            if (!Connected) return;
            try
            {
                // Read data from the remote device.
                int bytesRead = NetStream.EndRead(ar);

                if (bytesRead > 2)
                {
                    var packets = RecvBuf.SplitBytes(0, bytesRead);
                    foreach (byte[] packet in packets)
                    {
                        // read packet
                        ushort type = BitConverter.ToUInt16(packet, 0);

                        List<String> splitPacket = new List<string>();

                        if (packet.Length > 3)
                            splitPacket = packet.Skip(3).SplitStrings(Message.Delimiter, packet.Length - 3);

                        //var asdf = RecvBuf.Skip(3).ToUTF8();
                        //var splitPacket = asdf.Split(new char[] { '1' }).ToList();

                        // find the message handler method for this message type
                        if (MessageHandlers.ContainsKey((IncomingMessage.Type)type))
                        {
                            // found a handler, invoke it
                            var handler = MessageHandlers[(IncomingMessage.Type)type];
                            
                            /*
                            Console.WriteLine(Username + ": received " + Enum.GetName(typeof(IncomingMessage.Type), type));
                                                        
                            Console.WriteLine("[" + packet[0] + ", " + packet[1] + "] " + packet.Skip(3).ToArray().ToHexString());
                            Console.WriteLine(String.Join(", ", splitPacket.ToArray()));
                            Console.WriteLine("-----------------------------------------------");
                            */
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
                            //MessageHandlers[IncomingMessage.Type.Default].Invoke(this, new object[] { splitPacket });
                        }
                    }
                }

                // continue reading
                NetStream.BeginRead(RecvBuf, 0, RecvBuf.Length, new AsyncCallback(Receive), ar);
            }
            catch (Exception e)
            {
                if (Connected)
                {
                    Console.WriteLine(Username + ": Receive Got disconnected: " + e.Message + "\n" + e.InnerException);
                    Reconnect();
                }
            }
        }

        public void Send(OutgoingMessage.Type type)
        {
            Send(BitConverter.GetBytes((short)type));
        }

        public void Send(OutgoingMessage.Type type, params string[] args)
        {
            // this code is baby shit
            List<byte[]> args2 = new List<byte[]>();
            args2.Add(BitConverter.GetBytes((short)type));
            foreach (string arg in args)
            {
                args2.Add(arg.ToByteArray());
            }

            Send(args2.ToArray());
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
            if (!Connected) return;

            List<byte> data = new List<byte>();

            //var _loc_2:* = String(this.CMDTEC % 9000 + 1000).split("");
            //this.Serveur.send(this.MDT[_loc_2[0]] + this.MDT[_loc_2[1]] + this.MDT[_loc_2[2]] + this.MDT[_loc_2[3]] + param1);
            //var _loc_3:String = this;
            //var _loc_4:* = this.CMDTEC + 1;
            //_loc_3.CMDTEC = _loc_4;

            if (CMDTEC != 0)
            {
                string CMDTEC2 = (CMDTEC % 9000 + 1000).ToString();
                char a = MDT[Int32.Parse(CMDTEC2[0].ToString())];
                char b = MDT[Int32.Parse(CMDTEC2[1].ToString())];
                char c = MDT[Int32.Parse(CMDTEC2[2].ToString())];
                char d = MDT[Int32.Parse(CMDTEC2[3].ToString())];

                data.Add((byte)a);
                data.Add((byte)b);
                data.Add((byte)c);
                data.Add((byte)d);
            }

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

            try
            {
                NetStream.Write(data.ToArray(), 0, data.Count());
                CMDTEC++;
            }
            catch (Exception e)
            {
                if (Connected)
                {
                    Console.WriteLine(Username + ": Send() Got disconnected, reconnecting...");
                    Reconnect();
                }
            }
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
                {
                    Console.Title = Username + ": Current Cheese: " + Cheese;
                    Console.WriteLine(Username + ": Current Cheese: " + Cheese);
                }
            }
            catch (Exception)
            {
                Console.WriteLine(Username + ": Malformed shop/inventory packet... wtf? - " + String.Join(", ", data.ToArray()));
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
                //Thread.Sleep(DefaultDelay);
                // go in the hole if we're not a shaman
                if (!CurrentShamans.Contains(Username))
                    Send(OutgoingMessage.Type.EnterHole, "0".ToByteArray());
                else Console.WriteLine(Username + ": Can't win yet, we're the shaman");
            }
            // remove whoever got it from the players without cheese list
            else
            {
                PlayersWithoutCheese.RemoveAll(p => p.Contains(id.ToString()));
                if (PlayersWithoutCheese.Count == 1 && PlayersWithoutCheese.Where(p => p.Contains(Username)).Count() == 1 && CurrentShamans.Contains(Username))
                {
                    // finally we can finish!
                    Console.WriteLine(Username + ": Everyone else is done, going to the hole!");
                    Send(OutgoingMessage.Type.EnterHole, "0".ToByteArray());
                }
                else
                    Console.WriteLine(Username + ": " + PlayersWithoutCheese.Count + " players still don't have the cheese!");
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
            PlayersWithoutCheese = data;
            Console.WriteLine(Username + ": got player list: " + String.Join(", ", CleanNames(data).ToArray()));
        }

        [BotMessageHandler(IncomingMessage.Type.OnRoomTransform)]
        public void OnRoomTransform(List<string> data)
        {
            //Console.WriteLine(Username + ": Transform: " + String.Join(", ", data.ToArray()));

            CurrentShamans = new List<string>();
            try
            {
                if (data.Count == 1) // single shaman
                {
                    CurrentShamans.Add(GetPlayerName(data[0]));
                    Console.WriteLine(Username + ": Shaman: " + CurrentShamans[0]);
                }
                else
                {
                    CurrentShamans.Add(GetPlayerName(data[0]));
                    CurrentShamans.Add(GetPlayerName(data[1]));
                    Console.WriteLine(Username + ": Shamans: " + CurrentShamans[0] + ", " + CurrentShamans[1]);
                }
            }
            catch (Exception)
            {
                Console.WriteLine(Username + ": Error reading shamans");
            }
        }

        [BotMessageHandler(IncomingMessage.Type.OnRoomSync)]
        public void OnRoomSync(List<string> data)
        {
            if (CurrentPlayers.Count < 2)
            {
                Console.WriteLine(Username + ": All alone, can't get any cheese =(");
                return;
            }
            // should be able to pick it up as long as we don't go in the hole until everyone else is done
            /*
            else if (CurrentShamans.Contains(Username))
            {
                Console.WriteLine(Username + ": You are a shaman, can't get any cheese =(");
            }*/
            else
            {
                Console.WriteLine(Username + ": " + CurrentPlayers.Count + " players present: Grabbing cheese");
                //Thread.Sleep(rand.Next(3000, 7000));
                Send(OutgoingMessage.Type.GrabCheese);
            }

            //Spiral(Objects.PurpleArrow);

            if (ShowOffMode)
            {
                int proc = rand.Next(0, 100);
                if (proc < 25)
                    FireWorks();
                else if (proc > 25 && proc < 50)
                    Garbage();
                else if (proc > 50 && proc < 75)
                    Cheeeeese();
                else
                    Spiral(Objects.PurpleArrow);
            }
            
        }

        private Object PartyTimeLock = new Object();

        public const int stageWidth = 850;
        public const int stageHeight = 400;
        public const int stageCenterX = stageWidth / 2;
        public const int stageCenterY = stageHeight / 2;

        public void Spiral(Objects obj)
        {
            lock (PartyTimeLock)
            {
                if (CurrentShamans.Contains(Username))
                {
                    Console.WriteLine(Username + ": doing spiral");

                    VerticalObjectWipe(60, false, Objects.PurpleArrow);
                    Thread.Sleep(600);

                    double anglemod = 0;
                    for (int i = 0; i < 5; i++, anglemod += 0.3)
                    {
                        
                        Console.WriteLine(Username + ": round " + i);
                        for (double angle = 0, radius = 10; angle < 360; angle += 0.7 + anglemod, radius += 1.2)
                        {
                            int x = stageCenterX + (int)(radius * Math.Cos(angle));
                            int y = stageCenterY + (int)(radius * Math.Sin(angle));

                            if (obj == Objects.PurpleArrow)
                                PlaceArrow("0", x.ToString(), y.ToString(), angle.ToString(), false);
                            else
                                PlaceObject(obj.ToString(), x.ToString(), y.ToString(), angle.ToString(), false);

                            Thread.Sleep(10);
                        }
                        Thread.Sleep(250);
                    }
                }
            }
        }

        public void VerticalObjectWipe(int delay, bool TopDown, Objects obj)
        {
            if (TopDown)
            {
                for (int y = 50; y <= 400; y += 20)
                {
                    for (int x = 25; x < 850; x += 28)
                    {
                        if (obj == Objects.PurpleArrow)
                            PlaceArrow("0", x.ToString(), y.ToString(), (x + y % 360).ToString(), false);
                        else
                            PlaceObject(obj.ToString(), x.ToString(), y.ToString(), (x + y % 360).ToString(), false);

                    }
                    Thread.Sleep(60);
                }
            }
            else
            {
                for (int y = 400; y >= 50; y -= 20)
                {
                    for (int x = 25; x < 850; x += 35)
                    {
                        if (obj == Objects.PurpleArrow)
                            PlaceArrow("0", x.ToString(), y.ToString(), (x + y % 360).ToString(), false);
                        else
                            PlaceObject(obj.ToString(), x.ToString(), y.ToString(), (x + y % 360).ToString(), false);
                    }
                    Thread.Sleep(delay);
                }
            }
        }

        public void Garbage()
        {
            lock (PartyTimeLock)
            {
                if (CurrentShamans.Contains(Username))
                {
                    Console.WriteLine(Username + ": doing garbage avalanch");
                    int x;
                    int y;

                    VerticalObjectWipe(100, false, Objects.PurpleArrow);

                    x = rand.Next(25, 850);
                    y = rand.Next(0, 400);


                    for (int i = 0; i < 15; i++)
                    {
                        for (int j = 0; j < 30; j++)
                        {
                            PlaceObject(rand.Next(0, 25).ToString(), (x = rand.Next(25, 850)).ToString(), (y = rand.Next(0, 100)).ToString(), (x + y % 360).ToString(), false);
                            Thread.Sleep(30);
                        }
                        Thread.Sleep(100);
                    }
                }
            }
        }

        public void Cheeeeese()
        {
            lock (PartyTimeLock)
            {
                if (CurrentShamans.Contains(Username))
                {
                    Console.WriteLine(Username + ": doing cheeese avalanch");
                    int x;
                    int y;

                    VerticalObjectWipe(100, false, Objects.PurpleArrow);

                    x = rand.Next(25, 850);
                    y = rand.Next(0, 400);

                    for (int i = 0; i < 20; i++)
                    {
                        for (int j = 0; j < 25; j++)
                        {
                            PlaceObject("25", (x = rand.Next(25, 850)).ToString(), (y = rand.Next(0, 100)).ToString(), (x + y % 360).ToString(), false);
                            Thread.Sleep(10);
                        }
                        Thread.Sleep(45);
                    }
                }
            }
        }

        public void FireWorks()
        {
            lock (PartyTimeLock)
            {
                if (CurrentShamans.Contains(Username))
                {
                    Console.WriteLine(Username + ": doing fireworks");
                    for (int i = 0; i < 15; i++)
                    {
                        VerticalObjectWipe(60, false, Objects.PurpleArrow);
                        Thread.Sleep(600);

                        VerticalObjectWipe(60, true, Objects.TealDot);
                        Thread.Sleep(300);
                        
                        VerticalObjectWipe(60, false, Objects.PurpleArrow);
                        Thread.Sleep(600);

                        VerticalObjectWipe(60, true, Objects.YellowDot);
                        Thread.Sleep(300);

                        VerticalObjectWipe(60, false, Objects.RedDot);
                        Thread.Sleep(300);

                        VerticalObjectWipe(60, true, Objects.PurpleArrow);
                        Thread.Sleep(600);

                        VerticalObjectWipe(60, false, Objects.TealDot);
                        Thread.Sleep(300);
                    }

                }
            }
        }

        public void PlaceObject(string id, string x, string y, string rotation, bool ghost)
        {
            string ghostbool = ghost ? "0" : "1";
            Send(OutgoingMessage.Type.StartPlaceObject, id, x, y, ghostbool);
            Send(OutgoingMessage.Type.ValidatePlaceObject, id, x, y, rotation, "0", "0", ghostbool);
            Send(OutgoingMessage.Type.FinishPlaceObject);
        }

        public void PlaceArrow(string id, string x, string y, string rotation, bool ghost)
        {
            string ghostbool = ghost ? "0" : "1";
            Send(OutgoingMessage.Type.ValidatePlaceObject, id, x, y, rotation, "0", "0", ghostbool);
        }

        [BotMessageHandler(IncomingMessage.Type.OnUserLogin)]
        public void OnUserLogin(List<string> data)
        {
            UserId = Int32.Parse(data[1]);
            UserLevel = Int32.Parse(data[2]);
            Console.WriteLine(data[0] + " logged in, " + UserId + ", " + UserLevel);
        }

        [BotMessageHandler(IncomingMessage.Type.OnPing)]
        public void OnPing(List<string> data)
        {
            Console.WriteLine(Username + ": ->Ping");
            SendPing = DateTime.Now;
        }

        [BotMessageHandler(IncomingMessage.Type.OnVersion)]
        public void OnVersion(List<string> data)
        {
            /*
                    data = _loc_3[2].split(""); // data
                    i = 0;
                    while (i < 10)
                    {
                        
                        x = int(data[i]);
                        if (_loc_27 == 0)
                        {
                            this.MDT[i] = String.fromCharCode(10);
                        }
                        else
                        {
                            this.MDT[i] = String.fromCharCode(x);
                        }
                        i++;
                    }
                    this.CMDTEC = int(_loc_3[3]);
             */

            var data2 = data[1];
            for (int i = 0; i < 10; i++)
            {
                int x = Int32.Parse(data2[i].ToString());
                if (x == 0)
                    MDT[i] = '\x0a';
                else
                    MDT[i] = (char)x;
            }
            CMDTEC = Int32.Parse(data[2]);

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
                Console.WriteLine(Username + " failed to log in: Already logged in");
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
                String.IsNullOrEmpty(Password) ? new byte[] { } : Crypto.SHA256String(Password).ToByteArray();
            Send(OutgoingMessage.Type.Login, Username.ToByteArray(), pass, room.ToByteArray());

            return true;
        }

        public string GetPlayerName(string id)
        {
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
            var Cleaned = new List<string>();
            for (int i = 0; i < names.Count; i++)
            {
                if (names[i].Contains("#"))
                    Cleaned.Add(CleanName(names[i]));
            }

            return Cleaned;
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
