using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Voxel_Fortress
{
    class UniqueList<T> : IList<T>
    {
        private List<T> _list = new List<T>();
        private Dictionary<T, int> _indices = new Dictionary<T, int>();

        public T this[int index]
        {
            get
            {
                return _list[index];
            }

            set
            {
                //We cannot have duplicate values.
                if (Contains(value))
                    return;
                //Otherwise changing the value at any index is fine.
                _list[index] = value;
            }
        }

        private void RebuildIndices()
        {
            _indices.Clear();

            for(int i = 0; i < _list.Count; i++)
            {
                _indices[_list[i]] = i;
            }
        }

        public int Count
        {
            get
            {
                return _list.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((IList<T>)_list).IsReadOnly;
            }
        }

        public int IndexOf(T item)
        {
            if (Contains(item))
                return _indices[item];
            else
                return -1;
        }

        public void Add(T item)
        {
            if (Contains(item))
                return;

            int itemIndex = _list.Count;
            _list.Add(item);
            _indices[item] = itemIndex;
        }

        public void Clear()
        {
            _list.Clear();
            _indices.Clear();
        }

        public bool Contains(T item)
        {
            return _indices.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_list).GetEnumerator();
        }

        public void Insert(int index, T item)
        {
            ((IList<T>)_list).Insert(index, item);
            RebuildIndices();
        }

        public void RemoveAt(int index)
        {
            ((IList<T>)_list).RemoveAt(index);
            RebuildIndices();
        }

        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index < 0)
                return false;
            RemoveAt(index);
            return true;   
        }

        public int IndexAdd(T item)
        {
            int index = IndexOf(item);
            if(index < 0)
            {
                index = _list.Count;
                _list.Add(item);
                _indices[item] = index;
            }
            return index;
        }
    }
}
