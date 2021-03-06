﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCMUT.EMRCorefResol
{
    public static class IndexedEnumerableHelper
    {
        public static IIndexedEnumerable<T> ToIndexedEnumerable<T>(this IList<T> list)
        {
            return new DefaultIndexedEnumerable<T>(list);
        }

        public static IIndexedEnumerable<T> ToIndexedEnumerable<T>(this T[] array)
        {
            return new DefaultIndexedEnumerable<T>(array);
        }

        public static IIndexedEnumerable<T> ToIndexedEnumerable<T>(this IEnumerable<T> sequence)
        {
            return new DefaultIndexedEnumerable<T>(sequence);
        }

        public static IEnumerable<TResult> SelectIndex<TSource, TResult>(this IIndexedEnumerable<TSource> iEnum,
            Func<TSource, int, TResult> selector)
        {
            for (int i = 0; i < iEnum.Count; i++)
            {
                yield return selector(iEnum[i], i);
            }
        }
    }

    internal class DefaultIndexedEnumerable<T> : IIndexedEnumerable<T>
    {
        private readonly IList<T> _items;

        public DefaultIndexedEnumerable(IList<T> list)
        {
            _items = list;
        }

        public DefaultIndexedEnumerable(T[] array)
            : this(array.ToList())
        {
        }

        public DefaultIndexedEnumerable(IEnumerable<T> sequence)
            : this(sequence.ToList())
        {
        }

        public T this[int index]
        {
            get { return _items[index]; }
        }

        public int Count
        {
            get { return _items.Count; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
