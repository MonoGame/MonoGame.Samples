#region Copyright (c) 2007 Thomas H. Aylesworth
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"), 
// to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, 
// and/or sell copies of the Software, and to permit persons to whom the 
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in 
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
//
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace SwampLib
{
    /// <summary>
    /// Represents a fixed-size pool of available items that can be removed
    /// as needed and returned when finished.
    /// </summary>
    public class Pool<T> : IEnumerable<T> where T : new()
    {
        /// <summary>
        /// Represents an entry in a Pool collection.
        /// </summary>
        public struct Node
        {
            /// <summary>
            /// Used internally to track which entry in the Pool
            /// is associated with this Node.
            /// </summary>
            internal int NodeIndex;

            /// <summary>
            /// Item stored in Pool.
            /// </summary>
            public T Item;
        }


        /// <summary>
        /// Fixed Pool of item nodes.
        /// </summary>
        private Node[] pool;


        /// <summary>
        /// Array containing the active/available state for each item node 
        /// in the Pool.
        /// </summary>
        private bool[] active;


        /// <summary>
        /// Queue of available item node indices.
        /// </summary>
        private Queue<int> available;


        /// <summary>
        /// Gets the number of available items in the Pool.
        /// </summary>
        /// <remarks>
        /// Retrieving this property is an O(1) operation.
        /// </remarks>
        public int AvailableCount
        {
            get { return available.Count; }
        }


        /// <summary>
        /// Gets the number of active items in the Pool.
        /// </summary>
        /// <remarks>
        /// Retrieving this property is an O(1) operation.
        /// </remarks>
        public int ActiveCount
        {
            get { return pool.Length - available.Count; }
        }


        /// <summary>
        /// Gets the total number of items in the Pool.
        /// </summary>
        /// <remarks>
        /// Retrieving this property is an O(1) operation.
        /// </remarks>
        public int Capacity
        {
            get { return pool.Length; }
        }


        /// <summary>
        /// Initializes a new instance of the Pool class.
        /// </summary>
        /// <param name="numItems">Total number of items in the Pool.</param>
        /// <exception cref="ArgumentException">
        /// Number of items is less than 1.
        /// </exception>
        /// <remarks>
        /// This constructor is an O(n) operation, where n is capacity.
        /// </remarks>
        public Pool(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(
                              "Pool must contain at least one item.");
            }

            pool = new Node[capacity];
            active = new bool[capacity];
            available = new Queue<int>(capacity);

            for (int i = 0; i < capacity; i++)
            {
                pool[i] = new Node();
                pool[i].NodeIndex = i;
                pool[i].Item = new T();

                active[i] = false;
                available.Enqueue(i);
            }
        }


        /// <summary>
        /// Makes all items in the Pool available.
        /// </summary>
        /// <remarks>
        /// This method is an O(n) operation, where n is Capacity.
        /// </remarks>
        public void Clear()
        {
            available.Clear();

            for (int i = 0; i < pool.Length; i++)
            {
                active[i] = false;
                available.Enqueue(i);
            }
        }


        /// <summary>
        /// Removes an available item from the Pool and makes it active.
        /// </summary>
        /// <returns>The node that is removed from the available Pool.</returns>
        /// <exception cref="InvalidOperationException">
        /// There are no available items in the Pool.
        /// </exception>
        /// <remarks>
        /// This method is an O(1) operation.
        /// </remarks>
        public Node Get()
        {
            int nodeIndex = available.Dequeue();
            active[nodeIndex] = true;
            return pool[nodeIndex];
        }


        /// <summary>
        /// Removes an available item from the Pool,makes it active and adds the object to the node.
        /// </summary>
        /// <returns>The node that is removed from the available Pool.</returns>
        /// <exception cref="InvalidOperationException">
        /// There are no available items in the Pool.
        /// </exception>
        /// <remarks>
        /// This method is an O(1) operation.
        /// </remarks>
        public Node Add(T item)
        {
            int nodeIndex = available.Dequeue();
            active[nodeIndex] = true;
            pool[nodeIndex].Item = item;
            return pool[nodeIndex];
        }

        /// <summary>
        /// Returns an active item to the available Pool.
        /// </summary>
        /// <param name="item">The node to return to the available Pool.</param>
        /// <exception cref="ArgumentException">
        /// The node being returned is invalid.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The node being returned was not active.
        /// This probably means the node was previously returned.
        /// </exception>
        /// <remarks>
        /// This method is an O(1) operation.
        /// </remarks>
        public void Return(Node item)
        {
            if ((item.NodeIndex < 0) || (item.NodeIndex > pool.Length))
            {
                throw new ArgumentException("Invalid item node.");
            }

            if (!active[item.NodeIndex])
            {
                throw new InvalidOperationException("Attempt to return an inactive node.");
            }

            active[item.NodeIndex] = false;
            available.Enqueue(item.NodeIndex);
        }


        /// <summary>
        /// Sets the value of the item in the Pool associated with the 
        /// given node.
        /// </summary>
        /// <param name="item">The node whose item value is to be set.</param>
        /// <exception cref="ArgumentException">
        /// The node being returned is invalid.
        /// </exception>
        /// <remarks>
        /// This method is necessary to modify the value of a value type stored
        /// in the Pool.  It copies the value of the node's Item field into the
        /// Pool.
        /// This method is an O(1) operation.
        /// </remarks>
        public void SetItemValue(Node item)
        {
            if ((item.NodeIndex < 0) || (item.NodeIndex > pool.Length))
            {
                throw new ArgumentException("Invalid item node.");
            }

            pool[item.NodeIndex].Item = item.Item;
        }

        public void UpdateItem(int nodeindex, T item)
        {
            if ((nodeindex < 0) || (nodeindex > pool.Length))
            {
                throw new ArgumentException("Invalid item node.");
            }
            pool[nodeindex].Item = item;
        }


        /// <summary>
        /// Copies the active items to an existing one-dimensional Array, 
        /// starting at the specified array index. 
        /// </summary>
        /// <param name="array">
        /// The one-dimensional array to which active Pool items will be 
        /// copied.
        /// </param>
        /// <param name="arrayIndex">
        /// The index in array at which copying begins.
        /// </param>
        /// <returns>The number of items copied.</returns>
        /// <remarks>
        /// This method is an O(n) operation, where n is the smaller of 
        /// capacity or the array length.
        /// </remarks>
        public int CopyTo(T[] array, int arrayIndex)
        {
            int index = arrayIndex;

            foreach (Node item in pool)
            {
                if (active[item.NodeIndex])
                {
                    array[index++] = item.Item;

                    if (index == array.Length)
                    {
                        return index - arrayIndex;
                    }
                }
            }

            return index - arrayIndex;
        }


        /// <summary>
        /// Gets an enumerator that iterates through the active items 
        /// in the Pool.
        /// </summary>
        /// <returns>Enumerator for the active items.</returns>
        /// <remarks>
        /// This method is an O(n) operation, 
        /// where n is Capacity divided by ActiveCount. 
        /// </remarks>
        public IEnumerator<T> GetEnumerator()
        {
            foreach (Node item in pool)
            {
                if (active[item.NodeIndex])
                {
                    yield return item.Item;
                }
            }
        }


        /// <summary>
        /// Gets an enumerator that iterates through the active nodes 
        /// in the Pool.
        /// </summary>
        /// <remarks>
        /// This method is an O(n) operation, 
        /// where n is Capacity divided by ActiveCount. 
        /// </remarks>
        public IEnumerable<Node> ActiveNodes
        {
            get
            {
                foreach (Node item in pool)
                {
                    if (active[item.NodeIndex])
                    {
                        yield return item;
                    }
                }
            }
        }


        /// <summary>
        /// Gets an enumerator that iterates through all of the nodes 
        /// in the Pool.
        /// </summary>
        /// <remarks>
        /// This method is an O(1) operation. 
        /// </remarks>
        public IEnumerable<Node> AllNodes
        {
            get
            {
                foreach (Node item in pool)
                {
                    yield return item;
                }
            }
        }


        /// <summary>
        /// Implementation of the IEnumerable interface.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
