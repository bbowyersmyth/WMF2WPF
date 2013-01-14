using System.Collections.Generic;

namespace WPFGDI
{
    internal class ObjectTable
    {
        private List<LogObject> _items = new List<LogObject>();

        public List<LogObject> Items
        {
            get
            {
                return this._items;
            }
        }

        public void AddObject(LogObject newItem)
        {
            //Find blank spot
            for (var i = 0; i < _items.Count; i++)
            {
                if (_items[i] == null)
                {
                    _items[i] = newItem;
                    return;
                }
            }

            // Add to end if we didnt find a blank spot;
            _items.Add(newItem);
        }

        public void DeleteObject(int index)
        {
            _items[index] = null;
        }

    }
}
