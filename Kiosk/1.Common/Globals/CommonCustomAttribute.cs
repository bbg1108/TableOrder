using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kiosk.CommonEnum;

namespace Kiosk
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ImageInfoAttribute : Attribute
    {
        public string Source { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public ImageInfoAttribute(string source, int width = 50, int height = 50)
        {
            Source = source;
            Width = width;
            Height = height;
        }
    }

    public class KoreanTextAttribute : Attribute
    {
        public string Text { get; private set; }

        public KoreanTextAttribute(string text)
        {
            Text = text;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ToMessengerTypeAttribute : Attribute
    {
        public MessengerEnum MessengerType { get; private set; }

        public ToMessengerTypeAttribute(MessengerEnum type)
        {
            MessengerType = type;
        }
    }

    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
    public class MessageTypeAttribute : Attribute
    {
        public MsgIDEnum MsgID { get; private set; }
        //public EndianEnum Endian { get; private set; }

        public MessageTypeAttribute(MsgIDEnum msgID)
        {
            MsgID = msgID;
            //Endian = endian;
        }
    }
}
