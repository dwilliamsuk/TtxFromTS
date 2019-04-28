﻿using System;

namespace TtxFromTS.Teletext
{
    /// <summary>
    /// Provides an individual teletext packet.
    /// </summary>
    public class Packet
    {
        public enum PacketType
        {
            Header,
            PageBody,
            Fastext,
            TOPCommentary,
            PageReplacements,
            LinkedPages,
            PageEnhancements,
            MagazineEnhancements,
            BroadcastServiceData,
            Unspecified
        }

        /// <summary>
        /// Gets the framing code for the teletext packet.
        /// </summary>
        /// <value>The packet data.</value>
        public byte FramingCode { get; private set; }

        /// <summary>
        /// Gets the magazine number the packet corresponds to.
        /// </summary>
        /// <value>The magazine number.</value>
        public int? Magazine { get; private set; }

        /// <summary>
        /// Gets the packet number.
        /// </summary>
        /// <value>The packet number.</value>
        public int? Number { get; private set; }

        /// <summary>
        /// Gets the data contained within the packet.
        /// </summary>
        /// <value>The packet data.</value>
        public byte[] Data { get; private set; }

        /// <summary>
        /// Gets if the packet has been determined to contain unrecoverable errors.
        /// </summary>
        /// <value>True if there is an error, false if there isn't.</value>
        public bool DecodingError { get; private set; } = false;

        /// <summary>
        /// Gets the type of packet
        /// </summary>
        /// <value>The packet type.</value>
        public PacketType Type { get; private set; } = PacketType.Unspecified;

        /// <summary>
        /// Initialises a new instance of the <see cref="T:TtxFromTS.TeletextPacket"/> class.
        /// </summary>
        /// <param name="packetData">The teletext packet data to be decoded.</param>
        public Packet(byte[] packetData)
        {
            // Retrieve framing code
            FramingCode = packetData[1];
            // Check the framing code is valid, otherwise mark packet as containing errors
            if (FramingCode != 0xe4)
            {
                DecodingError = true;
            }
            // Retrieve and decode magazine number
            byte address1 = Decode.Hamming84(packetData[2]);
            Magazine = address1 & 0x07;
            // Check magazine number is valid, otherwise mark packet as containing errors, and change 0 to 8
            if (Magazine > 7)
            {
                Magazine = null;
                DecodingError = true;
            }
            else if (Magazine == 0)
            {
                Magazine = 8;
            }
            // Retrieve and decode packet number
            byte address2 = Decode.Hamming84(packetData[3]);
            Number = (address1 >> 3) | (address2 << 1);
            // Check the packet number is valid, otherwise mark packet as containing errors, and set the packet type
            if (Number == 0)
            {
                Type = PacketType.Header;
            }
            else if (Number <= 23)
            {
                Type = PacketType.PageBody;
            }
            else if (Number == 24)
            {
                Type = PacketType.Fastext;
            }
            else if (Number == 25)
            {
                Type = PacketType.TOPCommentary;
            }
            else if (Number == 26)
            {
                Type = PacketType.PageReplacements;
            }
            else if (Number == 27)
            {
                Type = PacketType.LinkedPages;
            }
            else if (Number == 28)
            {
                Type = PacketType.PageEnhancements;
            }
            else if (Number == 29)
            {
                Type = PacketType.MagazineEnhancements;
            }
            else if (Number == 30 && Magazine == 8)
            {
                Type = PacketType.BroadcastServiceData;
            }
            if (Number > 31)
            {
                Number = null;
                DecodingError = true;
            }
            // Retrieve packet data
            Data = new byte[packetData.Length - 4];
            Buffer.BlockCopy(packetData, 4, Data, 0, packetData.Length - 4);
        }
    }
}