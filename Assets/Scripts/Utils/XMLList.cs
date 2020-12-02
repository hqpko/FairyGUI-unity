using System;
using System.Collections.Generic;

namespace FairyGUI.Utils
{
    public class XMLList
    {
        public List<XML> rawList;

        public XMLList()
        {
            rawList = new List<XML>();
        }

        public XMLList(List<XML> list)
        {
            rawList = list;
        }

        public void Add(XML xml)
        {
            rawList.Add(xml);
        }

        public void Clear()
        {
            rawList.Clear();
        }

        public int Count => rawList.Count;

        public XML this[int index] => rawList[index];

        public Enumerator GetEnumerator()
        {
            return new Enumerator(rawList, null);
        }

        public Enumerator GetEnumerator(string selector)
        {
            return new Enumerator(rawList, selector);
        }

        private static List<XML> _tmpList = new List<XML>();

        public XMLList Filter(string selector)
        {
            var allFit = true;
            _tmpList.Clear();
            var cnt = rawList.Count;
            for (var i = 0; i < cnt; i++)
            {
                var xml = rawList[i];
                if (xml.name == selector)
                    _tmpList.Add(xml);
                else
                    allFit = false;
            }

            if (allFit)
            {
                return this;
            }
            else
            {
                var ret = new XMLList(_tmpList);
                _tmpList = new List<XML>();
                return ret;
            }
        }

        public XML Find(string selector)
        {
            var cnt = rawList.Count;
            for (var i = 0; i < cnt; i++)
            {
                var xml = rawList[i];
                if (xml.name == selector)
                    return xml;
            }

            return null;
        }

        public void RemoveAll(string selector)
        {
            rawList.RemoveAll(xml => xml.name == selector);
        }

        public struct Enumerator
        {
            private List<XML> _source;
            private string _selector;
            private int _index;
            private int _total;
            private XML _current;

            public Enumerator(List<XML> source, string selector)
            {
                _source = source;
                _selector = selector;
                _index = -1;
                if (_source != null)
                    _total = _source.Count;
                else
                    _total = 0;
                _current = null;
            }

            public XML Current => _current;

            public bool MoveNext()
            {
                while (++_index < _total)
                {
                    _current = _source[_index];
                    if (_selector == null || _current.name == _selector)
                        return true;
                }

                return false;
            }

            public void Erase()
            {
                _source.RemoveAt(_index);
                _total--;
            }

            public void Reset()
            {
                _index = -1;
            }
        }
    }
}