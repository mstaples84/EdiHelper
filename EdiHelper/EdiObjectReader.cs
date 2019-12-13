using System;
using EdiHelper.Attributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EdiHelper
{
    public class EdiObjectReader : IEdiObjectReader
    {
        private Dictionary<string, ICollection<ICollection<Tuple<string,string,int?>>>> _storages;
        public EdiObjectReader()
        {
            _storages = new Dictionary<string, ICollection<ICollection<Tuple<string,string,int?>>>>();
        }

        public void Read(object o)
        {
            var ediList = from p in o.GetType().GetProperties().Where(a => a.IsDefined(typeof(EdiAttribute), false))
                          let at = (EdiAttribute)p.GetCustomAttribute(typeof(EdiAttribute))
                          select new { Property = p, Attribute = at };

            var innerDict = new Dictionary<string, ICollection<Tuple<string,string,int?>>>();

            var orderedEdiList = ediList.OrderBy(t => t.Attribute.Tag);

            foreach (var item in orderedEdiList)
            {
                var tuple = new Tuple<string, string, int?>(item.Attribute.Placeholder,
                    item.Property.GetValue(o).ToString(), item.Attribute.Group);

                if (!innerDict.TryGetValue(item.Attribute.Tag, out var innerCollection))
                {
                    innerCollection = new List<Tuple<string,string,int?>>();

                    innerDict.Add(item.Attribute.Tag, innerCollection);
                }

                innerCollection.Add(tuple);
            }

            foreach (var item in innerDict)
            {
                if (!_storages.TryGetValue(item.Key, out var outerCollection))
                {
                    outerCollection = new List<ICollection<Tuple<string,string,int?>>>();

                    _storages.Add(item.Key, outerCollection);
                }

                outerCollection.Add(item.Value);
            }

            var ediObjectProperties = o.GetType().GetProperties().Where(p => p.IsDefined(typeof(EdiObjectAttribute), false));

            foreach (var ediObjectProperty in ediObjectProperties)
            {
                var value = ediObjectProperty.GetValue(o);

                Read(value);
            }

            var ediListProperties = o.GetType().GetProperties().Where(p => p.IsDefined(typeof(EdiListAttribute), false));

            foreach (var ediListProperty in ediListProperties)
            {
                var t = ediListProperty.GetValue(o);

                if (!(t is ICollection list)) continue;

                foreach (var value in list)
                {
                    Read(value);
                }
            }
        }

        public ICollection<ICollection<Tuple<string, string, int?>>> Get(string tag)
        {
            return _storages.TryGetValue(tag, out var collection) ? collection : null;
        }
    }

    public interface IEdiObjectReader
    {
        void Read(object o);

        ICollection<ICollection<Tuple<string,string,int?>>> Get(string tag);
    }
}
