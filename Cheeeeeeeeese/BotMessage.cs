using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Cheeeeeeeeese
{
    public static class BotMessage
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

    public class BotMessageHandlerAttribute : Attribute
    {
        public BotMessage.Type Type { get; set; }

        public BotMessageHandlerAttribute(BotMessage.Type type)
        {
            this.Type = type;
        }
    }
}
