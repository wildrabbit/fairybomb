using System;
using System.Collections;
using System.Collections.Generic;

public class PriorityQueue<T>
    where T : IEquatable<T>
{
    PairingHeap<T> heap;

    public PriorityQueue()
    {
        heap = new PairingHeap<T>();
    }

    public void Enqueue(T item, int priority)
    {
        heap.Insert(item, priority);
    }

    public T Dequeue()
    {
        return heap.RemoveTop();
    }

    public T Peek()
    {
        return heap.Peek();
    }

    public void Clear()
    {
        heap.Clear();
    }

    public int Count
    {
        get
        {
            return heap.Count;
        }
    }

    public void UpdateKey(T elem, int newPrio)
    {
        heap.UpdateKey(elem, newPrio);
    }
}
