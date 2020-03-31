using System;
using System.Collections;
using EdiHelper.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EdiHelper {
    public class EdiObjectReader : IEdiObjectReader {
        #region Fields
        // Dict<SegGroup, Dict<Indexer, List<Tuple<Tag,Placeholder,Value>>>>
        //private Dictionary<int, Dictionary<int, List<Tuple<string,string,string>>>> _dict;
        private Dictionary<int, List<Tuple<string, string, string>>[]> _dict;

        #endregion

        #region Ctor
        public EdiObjectReader() {
            _dict = new Dictionary<int, List<Tuple<string, string, string>>[]>();
        }

        #endregion

        #region Methods
        public void Read(object o, int? group = null, int? counter = null, int? size = null) {
            var ediList = from p in o.GetType().GetProperties().Where(a => a.IsDefined(typeof(EdiAttribute), false))
                          let at = (EdiAttribute)p.GetCustomAttribute(typeof(EdiAttribute))
                          select new { Property = p, Attribute = at };

            foreach (var item in ediList) {
                var g = item.Attribute.Group != 0 ? item.Attribute.Group : group ?? 0;
                var c = counter ?? 0;
                var s = size ?? 1;

                var t = new Tuple<string, string, string>(item.Attribute.Tag, item.Attribute.Placeholder, item.Property.GetValue(o)?.ToString());

                if (!_dict.TryGetValue(g, out var innerDict)) {
                    innerDict = new List<Tuple<string, string, string>>[s];
                    innerDict[c] = new List<Tuple<string, string, string>> { t };
                    _dict.Add(g, innerDict);
                } else {
                    if (innerDict[c] == null)
                        innerDict[c] = new List<Tuple<string, string, string>>();

                    innerDict[c].Add(t);
                }

            }

            var ediLists =
                from l in o.GetType().GetProperties().Where(a => a.IsDefined(typeof(EdiListAttribute), false))
                let at = (EdiListAttribute)l.GetCustomAttribute(typeof(EdiListAttribute))
                select new { Property = l, Attribute = at };

            foreach (var list in ediLists) {
                var t = list.Property.GetValue(o);

                if (!(t is ICollection col)) continue;

                var c = 0;

                var s = col.Count;

                foreach (var colVal in col) {
                    Read(colVal, list.Attribute.Group, c, s);
                    c++;
                }
            }
        }

        public List<Tuple<string, string, string>>[] GetSegmentGroup(int id) {
            return _dict.TryGetValue(id, out var array) ? array : null;
        }
        #endregion
    }

    public interface IEdiObjectReader
    {
        void Read(object o, int? group = null, int? counter = null, int? size = null);

        List<Tuple<string, string, string>>[] GetSegmentGroup(int id);
    }
}
