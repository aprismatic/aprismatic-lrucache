namespace Aprismatic.Cache
{
    internal class DequeElem<T>
    {
        public DequeElem<T> prev, next;
        public T item;

        public DequeElem(T newValue)
        {
            item = newValue;
            prev = null;
            next = null;
        }
    }
}
