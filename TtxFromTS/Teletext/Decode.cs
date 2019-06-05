﻿using System;
using System.Collections;

namespace TtxFromTS.Teletext
{
    /// <summary>
    /// Provides methods to decode data.
    /// </summary>
    internal static class Decode
    {
        #region Lookup Tables
        /// <summary>
        /// Lookup table of decoded bytes for each Hamming 8/4 encoded byte value.
        /// </summary>
        private static readonly byte[] _hamming84Table = {
            0x01, 0xff, 0x01, 0x01, 0xff, 0x00, 0x01, 0xff, 0xff, 0x02, 0x01, 0xff, 0x0a, 0xff, 0xff, 0x07,
            0xff, 0x00, 0x01, 0xff, 0x00, 0x00, 0xff, 0x00, 0x06, 0xff, 0xff, 0x0b, 0xff, 0x00, 0x03, 0xff,
            0xff, 0x0c, 0x01, 0xff, 0x04, 0xff, 0xff, 0x07, 0x06, 0xff, 0xff, 0x07, 0xff, 0x07, 0x07, 0x07,
            0x06, 0xff, 0xff, 0x05, 0xff, 0x00, 0x0d, 0xff, 0x06, 0x06, 0x06, 0xff, 0x06, 0xff, 0xff, 0x07,
            0xff, 0x02, 0x01, 0xff, 0x04, 0xff, 0xff, 0x09, 0x02, 0x02, 0xff, 0x02, 0xff, 0x02, 0x03, 0xff,
            0x08, 0xff, 0xff, 0x05, 0xff, 0x00, 0x03, 0xff, 0xff, 0x02, 0x03, 0xff, 0x03, 0xff, 0x03, 0x03,
            0x04, 0xff, 0xff, 0x05, 0x04, 0x04, 0x04, 0xff, 0xff, 0x02, 0x0f, 0xff, 0x04, 0xff, 0xff, 0x07,
            0xff, 0x05, 0x05, 0x05, 0x04, 0xff, 0xff, 0x05, 0x06, 0xff, 0xff, 0x05, 0xff, 0x0e, 0x03, 0xff,
            0xff, 0x0c, 0x01, 0xff, 0x0a, 0xff, 0xff, 0x09, 0x0a, 0xff, 0xff, 0x0b, 0x0a, 0x0a, 0x0a, 0xff,
            0x08, 0xff, 0xff, 0x0b, 0xff, 0x00, 0x0d, 0xff, 0xff, 0x0b, 0x0b, 0x0b, 0x0a, 0xff, 0xff, 0x0b,
            0x0c, 0x0c, 0xff, 0x0c, 0xff, 0x0c, 0x0d, 0xff, 0xff, 0x0c, 0x0f, 0xff, 0x0a, 0xff, 0xff, 0x07,
            0xff, 0x0c, 0x0d, 0xff, 0x0d, 0xff, 0x0d, 0x0d, 0x06, 0xff, 0xff, 0x0b, 0xff, 0x0e, 0x0d, 0xff,
            0x08, 0xff, 0xff, 0x09, 0xff, 0x09, 0x09, 0x09, 0xff, 0x02, 0x0f, 0xff, 0x0a, 0xff, 0xff, 0x09,
            0x08, 0x08, 0x08, 0xff, 0x08, 0xff, 0xff, 0x09, 0x08, 0xff, 0xff, 0x0b, 0xff, 0x0e, 0x03, 0xff,
            0xff, 0x0c, 0x0f, 0xff, 0x04, 0xff, 0xff, 0x09, 0x0f, 0xff, 0x0f, 0x0f, 0xff, 0x0e, 0x0f, 0xff,
            0x08, 0xff, 0xff, 0x05, 0xff, 0x0e, 0x0d, 0xff, 0xff, 0x0e, 0x0f, 0xff, 0x0e, 0x0e, 0xff, 0x0e
        };

        /// <summary>
        /// Lookup table of data values for the first byte of a Hamming 24/18 triplet.
        /// </summary>
        private static readonly byte[] _hamming2418Byte1Table = {
            0x00, 0x01, 0x00, 0x01, 0x02, 0x03, 0x02, 0x03, 0x04, 0x05, 0x04, 0x05, 0x06, 0x07, 0x06, 0x07,
            0x08, 0x09, 0x08, 0x09, 0x0a, 0x0b, 0x0a, 0x0b, 0x0c, 0x0d, 0x0c, 0x0d, 0x0e, 0x0f, 0x0e, 0x0f,
            0x00, 0x01, 0x00, 0x01, 0x02, 0x03, 0x02, 0x03, 0x04, 0x05, 0x04, 0x05, 0x06, 0x07, 0x06, 0x07,
            0x08, 0x09, 0x08, 0x09, 0x0a, 0x0b, 0x0a, 0x0b, 0x0c, 0x0d, 0x0c, 0x0d, 0x0e, 0x0f, 0x0e, 0x0f
        };

        /// <summary>
        /// Lookup table of parity results for Hamming 24/18 encoded bytes.
        /// </summary>
        private static readonly byte[,] _hamming2418ParityTable = {
            {
                0x00, 0x21, 0x22, 0x03, 0x23, 0x02, 0x01, 0x20, 0x24, 0x05, 0x06, 0x27, 0x07, 0x26, 0x25, 0x04,
                0x25, 0x04, 0x07, 0x26, 0x06, 0x27, 0x24, 0x05, 0x01, 0x20, 0x23, 0x02, 0x22, 0x03, 0x00, 0x21,
                0x26, 0x07, 0x04, 0x25, 0x05, 0x24, 0x27, 0x06, 0x02, 0x23, 0x20, 0x01, 0x21, 0x00, 0x03, 0x22,
                0x03, 0x22, 0x21, 0x00, 0x20, 0x01, 0x02, 0x23, 0x27, 0x06, 0x05, 0x24, 0x04, 0x25, 0x26, 0x07,
                0x27, 0x06, 0x05, 0x24, 0x04, 0x25, 0x26, 0x07, 0x03, 0x22, 0x21, 0x00, 0x20, 0x01, 0x02, 0x23,
                0x02, 0x23, 0x20, 0x01, 0x21, 0x00, 0x03, 0x22, 0x26, 0x07, 0x04, 0x25, 0x05, 0x24, 0x27, 0x06,
                0x01, 0x20, 0x23, 0x02, 0x22, 0x03, 0x00, 0x21, 0x25, 0x04, 0x07, 0x26, 0x06, 0x27, 0x24, 0x05,
                0x24, 0x05, 0x06, 0x27, 0x07, 0x26, 0x25, 0x04, 0x00, 0x21, 0x22, 0x03, 0x23, 0x02, 0x01, 0x20,
                0x28, 0x09, 0x0a, 0x2b, 0x0b, 0x2a, 0x29, 0x08, 0x0c, 0x2d, 0x2e, 0x0f, 0x2f, 0x0e, 0x0d, 0x2c,
                0x0d, 0x2c, 0x2f, 0x0e, 0x2e, 0x0f, 0x0c, 0x2d, 0x29, 0x08, 0x0b, 0x2a, 0x0a, 0x2b, 0x28, 0x09,
                0x0e, 0x2f, 0x2c, 0x0d, 0x2d, 0x0c, 0x0f, 0x2e, 0x2a, 0x0b, 0x08, 0x29, 0x09, 0x28, 0x2b, 0x0a,
                0x2b, 0x0a, 0x09, 0x28, 0x08, 0x29, 0x2a, 0x0b, 0x0f, 0x2e, 0x2d, 0x0c, 0x2c, 0x0d, 0x0e, 0x2f,
                0x0f, 0x2e, 0x2d, 0x0c, 0x2c, 0x0d, 0x0e, 0x2f, 0x2b, 0x0a, 0x09, 0x28, 0x08, 0x29, 0x2a, 0x0b,
                0x2a, 0x0b, 0x08, 0x29, 0x09, 0x28, 0x2b, 0x0a, 0x0e, 0x2f, 0x2c, 0x0d, 0x2d, 0x0c, 0x0f, 0x2e,
                0x29, 0x08, 0x0b, 0x2a, 0x0a, 0x2b, 0x28, 0x09, 0x0d, 0x2c, 0x2f, 0x0e, 0x2e, 0x0f, 0x0c, 0x2d,
                0x0c, 0x2d, 0x2e, 0x0f, 0x2f, 0x0e, 0x0d, 0x2c, 0x28, 0x09, 0x0a, 0x2b, 0x0b, 0x2a, 0x29, 0x08
            },
            {
                0x00, 0x29, 0x2a, 0x03, 0x2b, 0x02, 0x01, 0x28, 0x2c, 0x05, 0x06, 0x2f, 0x07, 0x2e, 0x2d, 0x04,
                0x2d, 0x04, 0x07, 0x2e, 0x06, 0x2f, 0x2c, 0x05, 0x01, 0x28, 0x2b, 0x02, 0x2a, 0x03, 0x00, 0x29,
                0x2e, 0x07, 0x04, 0x2d, 0x05, 0x2c, 0x2f, 0x06, 0x02, 0x2b, 0x28, 0x01, 0x29, 0x00, 0x03, 0x2a,
                0x03, 0x2a, 0x29, 0x00, 0x28, 0x01, 0x02, 0x2b, 0x2f, 0x06, 0x05, 0x2c, 0x04, 0x2d, 0x2e, 0x07,
                0x2f, 0x06, 0x05, 0x2c, 0x04, 0x2d, 0x2e, 0x07, 0x03, 0x2a, 0x29, 0x00, 0x28, 0x01, 0x02, 0x2b,
                0x02, 0x2b, 0x28, 0x01, 0x29, 0x00, 0x03, 0x2a, 0x2e, 0x07, 0x04, 0x2d, 0x05, 0x2c, 0x2f, 0x06,
                0x01, 0x28, 0x2b, 0x02, 0x2a, 0x03, 0x00, 0x29, 0x2d, 0x04, 0x07, 0x2e, 0x06, 0x2f, 0x2c, 0x05,
                0x2c, 0x05, 0x06, 0x2f, 0x07, 0x2e, 0x2d, 0x04, 0x00, 0x29, 0x2a, 0x03, 0x2b, 0x02, 0x01, 0x28,
                0x30, 0x19, 0x1a, 0x33, 0x1b, 0x32, 0x31, 0x18, 0x1c, 0x35, 0x36, 0x1f, 0x37, 0x1e, 0x1d, 0x34,
                0x1d, 0x34, 0x37, 0x1e, 0x36, 0x1f, 0x1c, 0x35, 0x31, 0x18, 0x1b, 0x32, 0x1a, 0x33, 0x30, 0x19,
                0x1e, 0x37, 0x34, 0x1d, 0x35, 0x1c, 0x1f, 0x36, 0x32, 0x1b, 0x18, 0x31, 0x19, 0x30, 0x33, 0x1a,
                0x33, 0x1a, 0x19, 0x30, 0x18, 0x31, 0x32, 0x1b, 0x1f, 0x36, 0x35, 0x1c, 0x34, 0x1d, 0x1e, 0x37,
                0x1f, 0x36, 0x35, 0x1c, 0x34, 0x1d, 0x1e, 0x37, 0x33, 0x1a, 0x19, 0x30, 0x18, 0x31, 0x32, 0x1b,
                0x32, 0x1b, 0x18, 0x31, 0x19, 0x30, 0x33, 0x1a, 0x1e, 0x37, 0x34, 0x1d, 0x35, 0x1c, 0x1f, 0x36,
                0x31, 0x18, 0x1b, 0x32, 0x1a, 0x33, 0x30, 0x19, 0x1d, 0x34, 0x37, 0x1e, 0x36, 0x1f, 0x1c, 0x35,
                0x1c, 0x35, 0x36, 0x1f, 0x37, 0x1e, 0x1d, 0x34, 0x30, 0x19, 0x1a, 0x33, 0x1b, 0x32, 0x31, 0x18
            },
            {
                0x3f, 0x0e, 0x0d, 0x3c, 0x0c, 0x3d, 0x3e, 0x0f, 0x0b, 0x3a, 0x39, 0x08, 0x38, 0x09, 0x0a, 0x3b,
                0x0a, 0x3b, 0x38, 0x09, 0x39, 0x08, 0x0b, 0x3a, 0x3e, 0x0f, 0x0c, 0x3d, 0x0d, 0x3c, 0x3f, 0x0e,
                0x09, 0x38, 0x3b, 0x0a, 0x3a, 0x0b, 0x08, 0x39, 0x3d, 0x0c, 0x0f, 0x3e, 0x0e, 0x3f, 0x3c, 0x0d,
                0x3c, 0x0d, 0x0e, 0x3f, 0x0f, 0x3e, 0x3d, 0x0c, 0x08, 0x39, 0x3a, 0x0b, 0x3b, 0x0a, 0x09, 0x38,
                0x08, 0x39, 0x3a, 0x0b, 0x3b, 0x0a, 0x09, 0x38, 0x3c, 0x0d, 0x0e, 0x3f, 0x0f, 0x3e, 0x3d, 0x0c,
                0x3d, 0x0c, 0x0f, 0x3e, 0x0e, 0x3f, 0x3c, 0x0d, 0x09, 0x38, 0x3b, 0x0a, 0x3a, 0x0b, 0x08, 0x39,
                0x3e, 0x0f, 0x0c, 0x3d, 0x0d, 0x3c, 0x3f, 0x0e, 0x0a, 0x3b, 0x38, 0x09, 0x39, 0x08, 0x0b, 0x3a,
                0x0b, 0x3a, 0x39, 0x08, 0x38, 0x09, 0x0a, 0x3b, 0x3f, 0x0e, 0x0d, 0x3c, 0x0c, 0x3d, 0x3e, 0x0f,
                0x1f, 0x2e, 0x2d, 0x1c, 0x2c, 0x1d, 0x1e, 0x2f, 0x2b, 0x1a, 0x19, 0x28, 0x18, 0x29, 0x2a, 0x1b,
                0x2a, 0x1b, 0x18, 0x29, 0x19, 0x28, 0x2b, 0x1a, 0x1e, 0x2f, 0x2c, 0x1d, 0x2d, 0x1c, 0x1f, 0x2e,
                0x29, 0x18, 0x1b, 0x2a, 0x1a, 0x2b, 0x28, 0x19, 0x1d, 0x2c, 0x2f, 0x1e, 0x2e, 0x1f, 0x1c, 0x2d,
                0x1c, 0x2d, 0x2e, 0x1f, 0x2f, 0x1e, 0x1d, 0x2c, 0x28, 0x19, 0x1a, 0x2b, 0x1b, 0x2a, 0x29, 0x18,
                0x28, 0x19, 0x1a, 0x2b, 0x1b, 0x2a, 0x29, 0x18, 0x1c, 0x2d, 0x2e, 0x1f, 0x2f, 0x1e, 0x1d, 0x2c,
                0x1d, 0x2c, 0x2f, 0x1e, 0x2e, 0x1f, 0x1c, 0x2d, 0x29, 0x18, 0x1b, 0x2a, 0x1a, 0x2b, 0x28, 0x19,
                0x1e, 0x2f, 0x2c, 0x1d, 0x2d, 0x1c, 0x1f, 0x2e, 0x2a, 0x1b, 0x18, 0x29, 0x19, 0x28, 0x2b, 0x1a,
                0x2b, 0x1a, 0x19, 0x28, 0x18, 0x29, 0x2a, 0x1b, 0x1f, 0x2e, 0x2d, 0x1c, 0x2c, 0x1d, 0x1e, 0x2f
            }
        };

        /// <summary>
        /// Lookup table of bit corrections for Hamming 24/18 encoded bytes, based on parity results.
        /// </summary>
        private static readonly uint[] _hamming2418ErrorTable = {
            0x00000000, 0x80000000, 0x80000000, 0x80000000, 0x80000000, 0x80000000, 0x80000000, 0x80000000,
            0x80000000, 0x80000000, 0x80000000, 0x80000000, 0x80000000, 0x80000000, 0x80000000, 0x80000000,
            0x80000000, 0x80000000, 0x80000000, 0x80000000, 0x80000000, 0x80000000, 0x80000000, 0x80000000,
            0x80000000, 0x80000000, 0x80000000, 0x80000000, 0x80000000, 0x80000000, 0x80000000, 0x80000000,
            0x00000000, 0x00000000, 0x00000000, 0x00000001, 0x00000000, 0x00000002, 0x00000004, 0x00000008,
            0x00000000, 0x00000010, 0x00000020, 0x00000040, 0x00000080, 0x00000100, 0x00000200, 0x00000400,
            0x00000000, 0x00000800, 0x00001000, 0x00002000, 0x00004000, 0x00008000, 0x00010000, 0x00020000,
            0x80000000, 0x80000000, 0x80000000, 0x80000000, 0x80000000, 0x80000000, 0x80000000, 0x80000000
        };

        /// <summary>
        /// Lookup table of bytes with the bit order reversed.
        /// </summary>
        private static readonly byte[] _reverseByte = {
            0x00, 0x80, 0x40, 0xc0, 0x20, 0xa0, 0x60, 0xe0, 0x10, 0x90, 0x50, 0xd0, 0x30, 0xb0, 0x70, 0xf0,
            0x08, 0x88, 0x48, 0xc8, 0x28, 0xa8, 0x68, 0xe8, 0x18, 0x98, 0x58, 0xd8, 0x38, 0xb8, 0x78, 0xf8,
            0x04, 0x84, 0x44, 0xc4, 0x24, 0xa4, 0x64, 0xe4, 0x14, 0x94, 0x54, 0xd4, 0x34, 0xb4, 0x74, 0xf4,
            0x0c, 0x8c, 0x4c, 0xcc, 0x2c, 0xac, 0x6c, 0xec, 0x1c, 0x9c, 0x5c, 0xdc, 0x3c, 0xbc, 0x7c, 0xfc,
            0x02, 0x82, 0x42, 0xc2, 0x22, 0xa2, 0x62, 0xe2, 0x12, 0x92, 0x52, 0xd2, 0x32, 0xb2, 0x72, 0xf2,
            0x0a, 0x8a, 0x4a, 0xca, 0x2a, 0xaa, 0x6a, 0xea, 0x1a, 0x9a, 0x5a, 0xda, 0x3a, 0xba, 0x7a, 0xfa,
            0x06, 0x86, 0x46, 0xc6, 0x26, 0xa6, 0x66, 0xe6, 0x16, 0x96, 0x56, 0xd6, 0x36, 0xb6, 0x76, 0xf6,
            0x0e, 0x8e, 0x4e, 0xce, 0x2e, 0xae, 0x6e, 0xee, 0x1e, 0x9e, 0x5e, 0xde, 0x3e, 0xbe, 0x7e, 0xfe,
            0x01, 0x81, 0x41, 0xc1, 0x21, 0xa1, 0x61, 0xe1, 0x11, 0x91, 0x51, 0xd1, 0x31, 0xb1, 0x71, 0xf1,
            0x09, 0x89, 0x49, 0xc9, 0x29, 0xa9, 0x69, 0xe9, 0x19, 0x99, 0x59, 0xd9, 0x39, 0xb9, 0x79, 0xf9,
            0x05, 0x85, 0x45, 0xc5, 0x25, 0xa5, 0x65, 0xe5, 0x15, 0x95, 0x55, 0xd5, 0x35, 0xb5, 0x75, 0xf5,
            0x0d, 0x8d, 0x4d, 0xcd, 0x2d, 0xad, 0x6d, 0xed, 0x1d, 0x9d, 0x5d, 0xdd, 0x3d, 0xbd, 0x7d, 0xfd,
            0x03, 0x83, 0x43, 0xc3, 0x23, 0xa3, 0x63, 0xe3, 0x13, 0x93, 0x53, 0xd3, 0x33, 0xb3, 0x73, 0xf3,
            0x0b, 0x8b, 0x4b, 0xcb, 0x2b, 0xab, 0x6b, 0xeb, 0x1b, 0x9b, 0x5b, 0xdb, 0x3b, 0xbb, 0x7b, 0xfb,
            0x07, 0x87, 0x47, 0xc7, 0x27, 0xa7, 0x67, 0xe7, 0x17, 0x97, 0x57, 0xd7, 0x37, 0xb7, 0x77, 0xf7,
            0x0f, 0x8f, 0x4f, 0xcf, 0x2f, 0xaf, 0x6f, 0xef, 0x1f, 0x9f, 0x5f, 0xdf, 0x3f, 0xbf, 0x7f, 0xff
        };
        #endregion

        #region Decode Methods
        /// <summary>
        /// Decodes Hamming 8/4 byte back to original value byte.
        /// </summary>
        /// <returns>Either the original encoded byte, or 0xff indicating an unrecoverable error.</returns>
        /// <param name="encodedByte">The Hamming encoded byte.</param>
        internal static byte Hamming84(byte encodedByte)
        {
            // Get decoded byte from the lookup table
            return _hamming84Table[encodedByte];
        }

        /// <summary>
        /// Decodes Hamming 24/18 triplet back to original value triplet.
        /// </summary>
        /// <returns>Either the original encoded triplet, or 0xffffff indicating an unrecoverable error.</returns>
        /// <param name="encodedByte">The Hamming encoded bytes.</param>
        internal static byte[] Hamming2418(byte[] encodedBytes)
        {
            // Get the bytes without the protection bits (i.e. just data)
            int[] dataBytes = new int[3];
            dataBytes[0] = _hamming2418Byte1Table[encodedBytes[0] >> 2];
            dataBytes[1] = encodedBytes[1] & 0x7f;
            dataBytes[2] = encodedBytes[2] & 0x7f;
            // Combine the data bytes in to a single int
            int data = dataBytes[0] | (dataBytes[1] << 4) | (dataBytes[2] << 11);
            // Get parity check results for the encoded triplet
            int parity = _hamming2418ParityTable[0, encodedBytes[0]] ^ _hamming2418ParityTable[1, encodedBytes[1]] ^ _hamming2418ParityTable[2, encodedBytes[2]];
            // Combine the decoded data with parity results to correct single bit errors, or to return 0xXXXX when 2 or more errors occur
            uint correctedData = (uint)data ^ _hamming2418ErrorTable[parity];
            // If there is not unrecoverable errors, convert the corrected data back to a triplet of bytes and return it, otherwise return 0xffffff
            if (correctedData < 0x80000000)
            {
                // Convert back to bytes
                byte[] decodedBytes = new byte[3];
                decodedBytes[0] = (byte)(correctedData & 0x0000003f);
                decodedBytes[1] = (byte)((correctedData >> 6) & 0x0000003f);
                decodedBytes[2] = (byte)((correctedData >> 12) & 0x0000003f);
                // Return triplet
                return decodedBytes;
            }
            else
            {
                // Return error state
                return new byte[] { 0xff, 0xff, 0xff };
            }
        }

        /// <summary>
        /// Checks an odd parity encoded bit for errors, and returns the original value if there isn't any.
        /// </summary>
        /// <returns>The original value byte if there is no errors, or 0x20 (i.e. a space) if there is.</returns>
        /// <param name="encodedByte">The odd parity encoded byte.</param>
        internal static byte OddParity(byte encodedByte)
        {
            // Convert byte to an array of bits
            BitArray bits = new BitArray(new byte[] { encodedByte });
            // Count the number of 1 bits
            int bitCount = 0;
            for (int i = 0; i < 8; i++)
            {
                if (bits[i])
                {
                    bitCount++;
                }
            }
            // If the number of 1 bits is odd, return the original value byte, otherwise return 0x00
            if (bitCount % 2 != 0)
            {
                return (byte)(encodedByte & 0x7f);
            }
            else
            {
                return 0x20;
            }
        }

        /// <summary>
        /// Reverses the bits in a byte.
        /// </summary>
        /// <returns>The reversed byte.</returns>
        /// <param name="originalByte">The byte to be reversed.</param>
        internal static byte Reverse(byte originalByte)
        {
            return _reverseByte[originalByte];
        }
        #endregion
    }
}