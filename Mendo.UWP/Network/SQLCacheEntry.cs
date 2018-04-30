using System;

namespace Mendo.UWP.Network
{
    public class SQLCacheStringEntry
    {
        public string Key { get; set; }
        public string Data { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime DateLastAccessed { get; set; }
    }

    public class SQLCacheEntry
    {
        public string Key { get; set; }
        public byte[] Data { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime DateLastAccessed { get; set; }
    }
}
