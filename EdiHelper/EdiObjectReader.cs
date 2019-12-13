using EdiHelper.Attributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EdiHelper
{
    public class EdiObjectReader : IEdiObjectReader
    {
        private Dictionary<string, ICollection<ICollection<KeyValuePair<string, string>>>> _storages;
        public EdiObjectReader()
        {
            _storages = new Dictionary<string, ICollection<ICollection<KeyValuePair<string, string>>>>();
        }

        public void Find(object o)
        {
            var ediList = from p in o.GetType().GetProperties().Where(a => a.IsDefined(typeof(EdiAttribute), false))
                          let at = (EdiAttribute)p.GetCustomAttribute(typeof(EdiAttribute))
                          select new { Property = p, Attribute = at };

            var innerDict = new Dictionary<string, ICollection<KeyValuePair<string, string>>>();

            var orderedEdiList = ediList.OrderBy(t => t.Attribute.Tag);

            foreach (var item in orderedEdiList)
            {
                var kv = new KeyValuePair<string, string>(item.Attribute.Placeholder, item.Property.GetValue(o).ToString());

                if (!innerDict.TryGetValue(item.Attribute.Tag, out var innerCollection))
                {
                    innerCollection = new List<KeyValuePair<string, string>>();

                    innerDict.Add(item.Attribute.Tag, innerCollection);
                }

                innerCollection.Add(kv);
            }

            foreach (var item in innerDict)
            {
                if (!_storages.TryGetValue(item.Key, out var outerCollection))
                {
                    outerCollection = new List<ICollection<KeyValuePair<string, string>>>();

                    _storages.Add(item.Key, outerCollection);
                }

                outerCollection.Add(item.Value);
            }

            var ediObjectProperties = o.GetType().GetProperties().Where(p => p.IsDefined(typeof(EdiObjectAttribute), false));

            foreach (var ediObjectProperty in ediObjectProperties)
            {
                var value = ediObjectProperty.GetValue(o);

                Find(value);
            }

            var ediListProperties = o.GetType().GetProperties().Where(p => p.IsDefined(typeof(EdiListAttribute), false));

            foreach (var ediListProperty in ediListProperties)
            {
                var t = ediListProperty.GetValue(o);

                if (!(t is ICollection list)) continue;

                foreach (var value in list)
                {
                    Find(value);
                }
            }
        }

        public ICollection<ICollection<KeyValuePair<string, string>>> Get(string tag)
        {
            return _storages.TryGetValue(tag, out var collection) ? collection : null;
        }
    }

    public interface IEdiObjectReader
    {
        void Find(object o);

        ICollection<ICollection<KeyValuePair<string, string>>> Get(string tag);
    }
}
