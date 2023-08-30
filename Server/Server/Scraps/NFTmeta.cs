using System;

namespace Server.Scraps
{
    [Serializable]
    public class NFTmeta
    {
        public string name;
        public string image;
        public string description;
        public Attribute[] attributes;
    }

    [Serializable]
    public class Attribute
    {
        public string trait_type;
        public string value;
    }
}
