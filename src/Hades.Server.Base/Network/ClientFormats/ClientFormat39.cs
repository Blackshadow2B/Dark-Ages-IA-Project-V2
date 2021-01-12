﻿#region

using System;
using System.Text;

#endregion

namespace Darkages.Network.ClientFormats
{
    public class ClientFormat39 : NetworkFormat
    {
        public ClientFormat39()
        {
            Secured = true;
            Command = 0x39;
        }

        public string Args { get; set; }
        public int Serial { get; set; }
        public ushort Step { get; set; }
        public byte Type { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            Type = reader.ReadByte();
            Serial = reader.ReadInt32();
            Step = reader.ReadUInt16();

            if (reader.GetCanRead())
            {
                var length = reader.ReadByte();

                if (Step == 0x0500 || Step == 0x0800 || Step == 0x9000)
                {
                    Args = Convert.ToString(length);
                }
                else
                {
                    var data = reader.ReadBytes(length);
                    Args = Encoding.ASCII.GetString(data);
                }
            }
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}