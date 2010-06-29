using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Cheeeeeeeeese
{
    public static class Message
    {
        public const byte Delimiter = 1;
        public const byte End = 0;
    }
    public static class IncomingMessage
    {
        public enum Type : short
        {
            On420 = 4|20<<8,
            OnRoomStart = 5|5<<8,
            OnRoomNoWin = 8|18<<8,
            OnRoomGotCheese = 5|19<<8,
            OnRoomJoin = 5|21<<8,
            OnRoomPlayers = 8|9<<8,
            OnRoomTransform = 8|20<<8,
            OnRoomSync = 8|21<<8,
            OnUserLogin = 26|8<<8,
            OnPing = 26|26<<8,
            OnVersion = 26|27<<8,
            Default = -1,
        }
    }

    public static class IgnoredMessage
    {
        public enum Type : short
        {
            unknown0 = 4 | 3 << 8,
            unknown1 = 4 | 4 << 8,
            unknown2 = 4 | 6 << 8,
            unknown3 = 5 | 7 << 8,
            unknown4 = 5 | 8 << 8,
            unknown5 = 5 | 9 << 8,
            CreateNail = 5 | 14 << 8, // create nail?
            unknown6 = 5 | 15 << 8,
            unknown7 = 5 | 16 << 8,
            unknown8 = 5 | 17 << 8,
            CreateObject = 5 | 20 << 8, // create object
            Chat = 6 | 6 << 8,  // chat
            unknown9 = 6 | 17 << 8,
            RemovePlayer = 8 | 5 << 8,  // remove player
            unknown10 = 8 | 6 << 8,
            unknown11 = 8 | 7 << 8,
            unknown12 = 8 | 14 << 8,
            TitleList = 8 | 15 << 8, // list of titles
            ShamanResult = 8 | 17 << 8, // "Thanks to <V>%1<BL>, we gathered %2 cheese !"
        }
    }

    public static class OutgoingMessage
    {
        public enum Type : short
        {
            Login = 26|4<<8,
            Four20 = 4|20<<8,
            GotCheese = 5|18<<8,
            PingTime = 26|10<<8,
            Ping = 26|26<<8,
            Win = 5|18<<8,
        }
    }

    public class BotMessageHandlerAttribute : Attribute
    {
        public IncomingMessage.Type Type { get; set; }

        public BotMessageHandlerAttribute(IncomingMessage.Type type)
        {
            this.Type = type;
        }
    }
}
