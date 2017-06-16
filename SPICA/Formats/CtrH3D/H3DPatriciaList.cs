﻿using SPICA.Formats.Common;
using SPICA.Serialization.Attributes;

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace SPICA.Formats.CtrH3D
{
    [Inline]
    public class H3DPatriciaList<T> : INotifyCollectionChanged, IEnumerable<T> where T : INamed
    {
        private List<T>         Contents;
        private H3DPatriciaTree NameTree;

        public T this[int Index]
        {
            get
            {
                return Contents[Index];
            }
            set
            {
                Contents[Index] = value;
            }
        }

        public T this[string Name]
        {
            get
            {
                return Contents[FindIndex(Name)];
            }
            set
            {
                Contents[FindIndex(Name)] = value;
            }
        }

        public int Count { get { return Contents.Count; } }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public H3DPatriciaList()
        {
            Contents = new List<T>();
            NameTree = new H3DPatriciaTree();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Contents.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction Action, T NewItem, int Index = -1)
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(Action, NewItem, Index));
        }

        //List management methods
        public void Add(T Value)
        {
            Contents.Add(Value);
            NameTree.Add(((INamed)Value).Name);

            OnCollectionChanged(NotifyCollectionChangedAction.Add, Value);
        }

        public void Insert(int Index, T Value)
        {
            Contents.Insert(Index, Value);
            NameTree.Insert(Index, ((INamed)Value).Name);

            OnCollectionChanged(NotifyCollectionChangedAction.Replace, Value, Index);
        }

        public void Remove(T Value)
        {
            Contents.Remove(Value);
            NameTree.Remove(((INamed)Value).Name);

            OnCollectionChanged(NotifyCollectionChangedAction.Remove, Value);
        }

        public void Clear()
        {
            Contents.Clear();
            NameTree.Clear();

            OnCollectionChanged(NotifyCollectionChangedAction.Reset, default(T));
        }

        public bool Contains(string Name)
        {
            return NameTree.Contains(Name);
        }

        public int FindIndex(string Name)
        {
            return NameTree.Find(Name);
        }

        public string FindName(int Index)
        {
            return NameTree.Find(Index);
        }

        public void Remove(int Index)
        {
            Remove(this[Index]);
        }

        public void Remove(string Name)
        {
            Remove(this[Name]);
        }
    }
}