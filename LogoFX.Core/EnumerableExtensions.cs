﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace LogoFX.Core
{    
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Applies the action to the input collection one by one
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ie">The ie.</param>
        /// <param name="action">The action.</param>
        public static IEnumerable<T> ForEachByOne<T>(this IEnumerable<T> ie, Action<T> action)
        {
            if (ie == null)
            {
                throw new ArgumentNullException("ie");
            }            
            using (IEnumerator<T> enumerator = ie.GetEnumerator())
            {                
                while (enumerator.MoveNext())
                {
                    action(enumerator.Current);
                    yield return enumerator.Current;
                }
            }
        }

        /// <summary>
        /// Performs an action on each item and increases an index of the enumerable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ie"></param>
        /// <param name="action"></param>
        public static IEnumerable<T> ForEachByOne<T>(this IEnumerable<T> ie, Action<T, int> action)
        {
            if (ie == null)
            {
                throw new ArgumentNullException("ie");
            }  
            int index = 0;
            using (IEnumerator<T> enumerator = ie.GetEnumerator())
            {                
                while (enumerator.MoveNext())
                {
                    action(enumerator.Current, index++);
                    yield return enumerator.Current;
                }
            }
        }

        /// <summary>
        /// Performs an action on range of elements and increases an index of the enumerable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ie"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="action"></param>
        public static IEnumerable<T> ForEachByOne<T>(this IEnumerable<T> ie, int start, int end, Action<T, int> action)
        {
            return ie.Skip(start)
                .Take(end - start + 1)
                .ForEachByOne(action);
        }

        /// <summary>
        /// Same as apply
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ie">The ie.</param>
        /// <param name="action">The action.</param>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> ie, Action<T> action)
        {
            if (ie == null)
            {
                throw new ArgumentNullException("ie");
            }  
            using (IEnumerator<T> enumerator = ie.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    action(enumerator.Current);
                }
            }
            return ie;
        }

        /// <summary>
        /// Performs an action on each item and increases an index of the enumerable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ie"></param>
        /// <param name="action"></param>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> ie, Action<T, int> action)
        {
            if (ie == null)
            {
                throw new ArgumentNullException("ie");
            }  
            int index = 0;
            using (IEnumerator<T> enumerator = ie.GetEnumerator())
            {                
                while (enumerator.MoveNext())
                {
                    action(enumerator.Current, index++);
                }
            }
            return ie;
        }

        /// <summary>
        /// Performs an action on range of elements and increases an index of the enumerable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ie"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="action"></param>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> ie, int start, int end, Action<T, int> action)
        {
            return ie.Skip(start)
                .Take(end - start + 1)
                .ForEach(action);
        }
    }
}
