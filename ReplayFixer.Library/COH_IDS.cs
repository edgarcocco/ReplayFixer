using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ReplayFixer.Library
{
    public static class COH_IDS
    {
        public static readonly byte NULL = 0x0;
        public static readonly byte[] INT_MAX_VALUE = new byte[] {
            0xff,0xff,0xff,0xff
        };
        public static readonly byte[] COH__REC = new byte[] {
            0x43, 0x4f, 0x48, 0x5f, 0x5f, 0x52, 0x45, 0x43
        };
        public static readonly byte[] RELIC_CHUNKY = new byte[] {
            0x52, 0x65, 0x6c, 0x69, 0x63, 0x20, 0x43, 0x68, 0x75, 0x6e, 0x6b, 0x79
        };
        public static readonly byte[] DATASDSC = new byte[] {
            0x44, 0x41, 0x54, 0x41, 0x53, 0x44, 0x53, 0x43
        };
        // equals to RelicCoH
        public static readonly byte[] RELICCOH = new byte[] {
            0x52, 0x65, 0x6c, 0x69, 0x63, 0x43, 0x4f, 0x48
        };

        public static readonly byte[] RELICCoH = new byte[] {
            0x52, 0x65, 0x6c, 0x69, 0x63, 0x43, 0x6f, 0x48
        };

        public static readonly byte[] FOLDINFO = new byte[] {
            0x01, 0x46, 0x4f, 0x4c, 0x44, 0x49, 0x4e, 0x46, 0x4f, 0x01
        };

        public static readonly byte[] FOLDFLDR = new byte[] {
            0x46, 0x4f, 0x4c, 0x44, 0x46, 0x4c, 0x44, 0x52,
        };

        public static readonly byte[] DATAPBGA = {
    0x44, 0x41, 0x54, 0x41, 0x50, 0x42, 0x47, 0x41,
};



        // this array equals to \Users\ with NULLs
        public static readonly byte[] USERS = new byte[]
        {
            0x5c, 0x00, 0x55, 0x00, 0x73, 0x00, 0x65, 0x00, 0x72, 0x00, 0x73, 0x00, 0x5c
        };
        // hex representation of AM with NULL separator, apply RemoveByteNulls() if needed without null
        public static readonly byte[] AM = new byte[] {
           0x41, 0x00, 0x4d,
        };
        public static readonly byte[] PM = new byte[] {
           0x50, 0x00, 0x4d,
        };
    }
}

