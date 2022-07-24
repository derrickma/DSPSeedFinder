using System.IO;
using System.Xml.Serialization;

namespace DspFindSeed
{
    public static class LDB
    {
        private static string                                     protoResDir = "Prototypes/";
        public static  ThemeProtoSet _themes;
        public static  VeinProtoSet  _veins;
        public static  ItemProtoSet  _items;

        private static T LoadTable<T>(ref T tmp) where T : ProtoTable
        {
            if ((object) tmp != null)
                return tmp;
            string        str           = LDB.protoResDir + typeof (T).Name;
            XmlSerializer xmlSerializer = new XmlSerializer(typeof (T));
            tmp = xmlSerializer.Deserialize((Stream) File.OpenRead(str + ".xml")) as T;
            if (tmp is VeinProtoSet veinProtoSet)
                veinProtoSet.OnAfterDeserialize();
            if (tmp is ItemProtoSet itemProtoSet)
                itemProtoSet.OnAfterDeserialize();
            if (tmp is ThemeProtoSet themeProtoSet)
                themeProtoSet.OnAfterDeserialize();
            return tmp;
        }

        public static ThemeProtoSet themes => LDB.LoadTable<ThemeProtoSet>(ref LDB._themes);

        public static VeinProtoSet veins => LDB.LoadTable<VeinProtoSet>(ref LDB._veins);

        public static ItemProtoSet items => LDB.LoadTable<ItemProtoSet>(ref LDB._items);
    }
    
    public abstract class ProtoTable
    {
        public string TableName;

        public abstract void Init(int length);

        public abstract Proto this[int index] { get; set; }

        public abstract int Length { get; }
    }
}