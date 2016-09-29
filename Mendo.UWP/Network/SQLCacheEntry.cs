using SQLite;
using System;

namespace Mendo.UWP.Network
{
    public class SQLCacheStringEntry
    {
        [PrimaryKey]
        [Indexed]
        public string Key { get; set; }
        public string Data { get; set; }

        [Indexed]
        public DateTime DateAdded { get; set; }

        [Indexed]
        public DateTime DateLastAccessed { get; set; }
    }

    public class SQLCacheEntry
    {
        [PrimaryKey]
        [Indexed]
        public string Key { get; set; }

        public byte[] Data { get; set; }

        [Indexed]
        public DateTime DateAdded { get; set; }

        [Indexed]
        public DateTime DateLastAccessed { get; set; }
    }
}
