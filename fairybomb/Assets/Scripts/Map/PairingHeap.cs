using System;
using System.Collections;
using System.Collections.Generic;

public class PairingHeap<T>  where T : IEquatable<T>
{
    class Entry
    {
        public T item;
        public int priority;

        public Entry(T _item, int _priority)
        {
            item = _item;
            priority = _priority;
        }

        public override string ToString()
        {
            return string.Format("Item: {0}, Prio: {1}", item.ToString(), priority);
        }
    }

    Entry root = null;
    List<PairingHeap<T>> children = null;

    public bool Empty
    {
        get
        {
            return Count == 0;
        }
    }

    public int Count
    {
        get
        {
            if (root == null)
            {
                return 0;
            }
            int aggregate = 1;
            for (int i = 0; i < children.Count; ++i)
            {
                aggregate += children[i].Count;
            }
            return aggregate;
        }
    }

    public PairingHeap()
    {
        root = null;
        children = null;
    }

    public PairingHeap(T item, int priority)
    {
        root = new Entry(item, priority);
        children = new List<PairingHeap<T>>();
    }

    public void Insert(T item, int priority)
    {
        PairingHeap<T> aux = new PairingHeap<T>(item, priority);
        Merge(aux);
    }

    void Merge(PairingHeap<T> other)
    {
        if (Empty && !other.Empty)
        {
            root = other.root;
            children = other.children;
        }
        else if (!Empty && !other.Empty)
        {
            int ourPrio = root.priority;
            int otherPrio = other.root.priority;
            if (ourPrio < otherPrio)
            {
                children.Add(other);
            }
            else
            {
                PairingHeap<T> us = new PairingHeap<T>(root.item, root.priority);
                us.children = new List<PairingHeap<T>>(children);

                root.item = other.root.item;
                root.priority = other.root.priority;
                children = other.children;
                children.Add(us);
            }
        }
    }

    public T Peek()
    {
        if (Count == 0)
        {
            return default(T);
        }
        return root.item;
    }

    public T RemoveTop()
    {
        if (Count == 0)
        {
            return default(T);
        }
        else if (Count == 1)
        {
            T top = Peek();
            root = null;
            children = null;
            return top;
        }
        else
        {
            T top = Peek();
            MergePairs(children);
            return top;
        }
    }

    bool UpdateKeyInternal(T item, int newPrio, PairingHeap<T> originalHeap)
    {
        bool equalsRoot = item.Equals(root.item);
        if (equalsRoot && (newPrio < root.priority))
        {
            root.priority = newPrio;
            return true;
        }
        bool stop = false;
        for (int i = 0; !stop && i < children.Count; ++i)
        {
            equalsRoot = children[i].root.item.Equals(item);
            if (equalsRoot)
            {
                if (newPrio >= children[i].root.priority) return true;
                PairingHeap<T> child = children[i];
                child.root.priority = newPrio;
                if (child.root.priority < root.priority)
                {
                    children.Remove(child);
                    originalHeap.Merge(child);
                }
                return true;
            }
            else
            {
                stop = children[i].UpdateKeyInternal(item, newPrio, originalHeap);
            }
        }
        return false;
    }

    public void UpdateKey(T item, int newPrio)
    {
        UpdateKeyInternal(item, newPrio, this);
    }

    void MergePairs(List<PairingHeap<T>> l)
    {
        if (l.Count == 0)
        {
            return;
        }
        else if (l.Count == 1)
        {
            if (root == null)
            {
                root = l[0].root;
            }
            else
            {
                root.item = l[0].root.item;
                root.priority = l[0].root.priority;
            }
            children = l[0].children;
        }
        else
        {
            PairingHeap<T> aux = new PairingHeap<T>();
            aux.Merge(l[0]);
            aux.Merge(l[1]);
            PairingHeap<T> aux2 = new PairingHeap<T>();
            aux2.MergePairs(l.GetRange(2, l.Count - 2));
            aux.Merge(aux2);
            if (root == null)
            {
                root = aux.root;
            }
            else
            {
                root.item = aux.root.item;
                root.priority = aux.root.priority;
            }
            children = aux.children;
        }
    }

    public void Clear()
    {
        root = null;
        for (int i = 0; i < children.Count; ++i)
        {
            children.Clear();
        }
        children = null;
    }

    public override string ToString()
    {
        return string.Format("[{0}], numChildren: {1}", root, children.Count);
    }

}
