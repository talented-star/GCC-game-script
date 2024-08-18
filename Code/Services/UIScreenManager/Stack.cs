using System.Collections.Generic;

namespace GrabCoin.UI.ScreenManager
{
    public class Stack<T>
    {
        public bool IsEmpty => elements.Count == 0;

        private List<T> elements = new List<T>();

        public void Push(T element)
        {
            if (Contains(element))
                Remove(element);
            elements.Add(element);
        }

        public T Peek()
        {
            if (elements.Count <= 0)
                return default(T);
            return elements[elements.Count - 1];
        }

        public T Pop()
        {
            if (elements.Count <= 0)
                return default(T);

            var result = elements[elements.Count - 1];
            elements.RemoveAt(elements.Count - 1);
            return result;
        }

        public void Remove(T element)
        {
            elements.Remove(element);
        }

        public bool Contains(T element)
        {
            return elements.Contains(element);
        }
    }

}