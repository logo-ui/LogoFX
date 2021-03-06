﻿// Partial Copyright (c) LogoUI Software Solutions LTD
// Partial Copyright (c) Puzzlehead LTD
// Vlad Spivak, Igal Abachi
// This source file is the part of LogoFX Framework http://logofx.codeplex.com
// See accompanying licences and credits.

using System;
using System.Collections.Generic;
using System.Linq;
using LogoFX.Core;
using LogoFX.Client.Mvvm.Model.Contracts;

namespace LogoFX.Client.Mvvm.Model
{
    public static class Extensions
    {
        /// <summary>
        /// Clones the source contents into the destination's , 
        /// while notifying to the gui on all the diffs between the source and the destination
        /// </summary>
        /// <remarks>Generalized version - most constraints reduced to serve maximum usecases</remarks>
        /// <typeparam name="T">Destination type</typeparam>
        /// <typeparam name="TW">source type</typeparam>
        /// <param name="destination">The destination list</param>
        /// <param name="source">The source list</param>
        /// <param name="creator">Factory method to produce T based on TW</param>
        /// <param name="comparator">Predicate to compare instances of T and TW</param>
        public static void Merge<T, TW>(this IList<T> destination, IList<TW> source, Func<TW, T> creator, Func<T, TW, bool> comparator)
            where T : class
            where TW : class
        {
            if (comparator == null)
                throw new ArgumentException("comparator");
            if (creator == null)
                throw new ArgumentException("creator");
            if (source == null)
                throw new ArgumentException("source");

            //make a copy for thread saftey, so that it won't be changed from another thread in the middle of the enumeration
            //fixes: InvalidOperation_EnumFailedVersion: "Collection was modified; enumeration operation may not execute."
            var destinationCopy = destination.ToList().Where(a => a != null);
            var sourceCopy = source.ToList().Where(a => a != null);

            IList<T> old = destinationCopy.Where(a => !sourceCopy.Any(b => comparator(a, b))).ToList();
            old.ForEach(a => destination.Remove(a));

            source.Where(a => a != null).ForEach(a =>
            {
                T found = destination.Where(b => b != null).FirstOrDefault(b => comparator(b, a));
                if (found == null)
                    destination.Add(creator(a));
                else if (found is IMergeable<TW> &&
                    // ReSharper disable ConditionIsAlwaysTrueOrFalse
                    //in case T and TW is the same type those two can be the same object
                     !ReferenceEquals(found, a)
                    // ReSharper restore ConditionIsAlwaysTrueOrFalse
                    )
                    ((IMergeable<TW>)found).Merge(a);
            });
        }
    }
}
