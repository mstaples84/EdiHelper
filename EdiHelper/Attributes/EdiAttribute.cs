using System;

namespace EdiHelper.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public class EdiAttribute : Attribute
    {
        public string Tag { get; set; }
        public string Placeholder { get; set; }
        public int Group { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class EdiObjectAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class EdiListAttribute : Attribute { }
}
