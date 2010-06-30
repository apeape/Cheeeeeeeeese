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
        public enum Type : ushort
        {
            /*
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
            OnInventory = 20|20<<8,
            OnLoginError = 26 | 3 << 8,
            OnChat = 26 | 4 << 8,
            OnBanned = 26 | 17 << 8,
            OnPermaBanned = 26 | 18 << 8,
             */
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
            OnInventory = 20|20<<8,
            OnLoginError = 26 | 3 << 8,
            OnChat = 26 | 4 << 8,
            OnBanned = 26 | 17 << 8,
            OnPermaBanned = 26 | 18 << 8,
        }
    }

    public static class IgnoredMessage
    {
        public enum Type : ushort
        {
            unknown4_3 = 4 | 3 << 8,
            unknown4_4 = 4 | 4 << 8,
            unknown4_6 = 4 | 6 << 8,
            unknown4_8 = 4 | 8 << 8,
            unknown4_9 = 4 | 9 << 8,
            unknown5_7 = 5 | 7 << 8,
            unknown5_8 = 5 | 8 << 8,
            unknown5_9 = 5 | 9 << 8,
            CreateNail = 5 | 14 << 8, // create nail?
            unknown5_15 = 5 | 15 << 8,
            unknown5_16 = 5 | 16 << 8,
            unknown5_17 = 5 | 17 << 8,
            CreateObject = 5 | 20 << 8, // create object
            Chat = 6 | 6 << 8,  // chat
            unknown6_17 = 6 | 17 << 8,
            RemovePlayer = 8 | 5 << 8,  // remove player
            unknown8_6 = 8 | 6 << 8,
            unknown8_7 = 8 | 7 << 8,
            unknown8_8 = 8 | 8 << 8,
            unknown8_14 = 8 | 14 << 8,
            TitleList = 8 | 15 << 8, // list of titles
            ShamanResult = 8 | 17 << 8, // "Thanks to <V>%1<BL>, we gathered %2 cheese !"
        }
    }

    public static class OutgoingMessage
    {
        public enum Type : ushort
        {
            Login = 26|4<<8,
            Four20 = 4|20<<8,
            EnterHole = 5|18<<8,
            PingTime = 26|2<<8,
            Ping = 26|26<<8,
            Win = 5|18<<8,
            GrabCheese = 5|19<<8,
            CheckInventory = 20|20<<8,
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
