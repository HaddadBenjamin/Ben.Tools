using System;
using System.Collections.Generic;
using System.Linq;
using BenTools.Extensions.BaseTypes;

namespace BenTools.Extensions.Sequences
{
    public static class EnumerableExtension
    {
        private static Random Random = new Random();

        #region Filters
        public static IEnumerable<ElementType> WhereWithIndex<ElementType>(this IEnumerable<ElementType> sequence, Func<ElementType, int, bool> predicate) =>
            sequence.Select((element, index) => new { element, index })
                    .Where((elementWithIndex) => predicate(elementWithIndex.element, elementWithIndex.index))
                    .Select((elementWithIndex) => elementWithIndex.element);

        public static IEnumerable<ElementType> WhereIndexIsBetween<ElementType>(this IEnumerable<ElementType> sequence, int minimumIndex, int maxmimumIndex) =>
            sequence.WhereWithIndex((element, index) => index >= minimumIndex && index <= maxmimumIndex);

        public static IEnumerable<ElementType> WhereIndexIsAtLeast<ElementType>(this IEnumerable<ElementType> sequence, int minimumIndex) =>
            sequence.WhereIndexIsBetween(minimumIndex, int.MaxValue);

        public static IEnumerable<ElementType> WhereIndexIsAtMost<ElementType>(this IEnumerable<ElementType> sequence, int maximumIndex) =>
            sequence.WhereIndexIsBetween(0, maximumIndex);

        public static IEnumerable<ElementType> DistinctByColumn<ElementType, PredicateKey>(
            this IEnumerable<ElementType> sequence,
            Func<ElementType, PredicateKey> predicate) =>
            sequence.GroupBy(predicate)
                    .Select(element => element.First());
        #endregion

        #region Count Method(s)
        public static bool CountAtLeast<ElementType>(this IEnumerable<ElementType> sequence, int numberOfElements) => sequence.Count() >= numberOfElements;

        public static bool CountAtMost<ElementType>(this IEnumerable<ElementType> sequence, int numberOfElements) => sequence.Count() <= numberOfElements;

        public static bool CountIsBetween<ElementType>(this IEnumerable<ElementType> sequence, int minimum, int maximum) => sequence.Count() >= minimum && sequence.Count() <= maximum;

        public static bool CountCompare<ElementType>(this IEnumerable<ElementType> sequence, IEnumerable<ElementType> otherSequence) => sequence.Count() == otherSequence.Count();
        #endregion

        #region Utilities
        public static void Consume<ElementType>(this IEnumerable<ElementType> sequence)
        {
            foreach (var element in sequence);
        }

        public static void Foreach<ElementType>(
            this IEnumerable<ElementType> sequence,
            Action<ElementType> action)
        {
            foreach (var element in sequence)
                action(element);
        }

        public static void Foreach<ElementType>(
            this IEnumerable<ElementType> sequence,
            Action<ElementType, int> action)
        {
            var index = 0;

            foreach (var element in sequence)
                action(element, index++);
        }
        
        public static int IndexOf<ElementType>(this IEnumerable<ElementType> sequence, ElementType elementToSearch, IEqualityComparer<ElementType> comparer = default(IEqualityComparer<ElementType>))
        {
            comparer = comparer ?? EqualityComparer<ElementType>.Default;

            var elementFound = sequence.Select((element, index) => new { element, index })
                                       .FirstOrDefault(elementWithIndex => comparer.Equals(elementWithIndex.element, elementToSearch));

            return elementFound == null ? -1 : elementFound.index;
        }

        public static int IndexOf<ElementType>(this IEnumerable<ElementType> sequence, Func<ElementType, bool> predicate, IEqualityComparer<ElementType> comparer = default(IEqualityComparer<ElementType>)) =>
            sequence.IndexOf(sequence.FirstOrDefault(predicate), comparer);
        
        public static IEnumerable<ElementType> CopySequence<ElementType>(this IEnumerable<ElementType> sequence) => sequence.DeepCopy();

        /// <summary>
        /// Mélange une sequence.
        /// </summary>
        public static IEnumerable<ElementType> Shuffle<ElementType>(this IEnumerable<ElementType> sequence) => sequence.OrderBy(element => Guid.NewGuid());
        #endregion

        #region Element(s) Insert & Remove & Merge
        public static IEnumerable<ElementType> AddElements<ElementType>(
            this IEnumerable<ElementType> sequence,
            IEnumerable<ElementType> elementsToAdd)
        {
            if (sequence is List<ElementType> list)
            {
                list.AddRange(elementsToAdd);
                return list;
            }

            return sequence.Concat(elementsToAdd);
        }

        public static IEnumerable<ElementType> RemoveNullElements<ElementType>(this IEnumerable<ElementType> sequence)
            where ElementType : class =>
            sequence.Where(element => element != null);

        public static IEnumerable<ElementType> RemoveElements<ElementType>(this IEnumerable<ElementType> sequence, Func<ElementType, bool> predicate) => sequence.Except(sequence.Where(predicate));
        
        public static IEnumerable<ElementType> MergeBy<ElementType, ElementKey>(this IEnumerable<ElementType> sequence, IEnumerable<ElementType> elementsToMerge, Func<ElementType, ElementKey> predicate) =>
            sequence.Union(elementsToMerge)
                    .Reverse()
                    .GroupBy(predicate)
                    .Select(element => element.First());
        #endregion

        #region Predicate
        public static bool ContainsAll<ElementType>(this IEnumerable<ElementType> sequence, IEnumerable<ElementType> elements) => sequence.All(elements.Contains);

        public static bool ContainsAny<ElementType>(this IEnumerable<ElementType> sequence, IEnumerable<ElementType> elements) => sequence.Any(elements.Contains);
        #endregion

        #region Element(s) Generation
        public static ElementType RandomElement<ElementType>(this IEnumerable<ElementType> sequence) =>
            sequence.RandomElements(1)
                    .Single();

        public static IEnumerable<ElementType> RandomElements<ElementType>(
            this IEnumerable<ElementType> sequence,
            int numbersOfElementsToGenerate = 10)
        {
            var sequenceCount = sequence.Count();

            return Enumerable.Repeat(sequence, numbersOfElementsToGenerate)
                             .Select(element => sequence.ElementAt(Random.Next(sequenceCount)));
        }
        #endregion

        #region Selection and Group
        /// <summary>
        /// GroupByColumns(3) : 
        ///                             [1, 2, 3]
        /// [1, 2, 3, 4, 5, 6, 7]  =>   [4, 5, 6]
        ///                             [7,     ]
        /// </summary>
        public static IEnumerable<IEnumerable<ElementType>> GroupByColumns<ElementType>(
            this IEnumerable<ElementType> sequence,
            int numberOfColumns) =>
            sequence
                .AsParallel() // à retirer si la séquence possède peu d'éléments.
                .Select((value, columnIndex) =>
                    columnIndex <= sequence.Count() / numberOfColumns ?
                        sequence.Where((element, index) => index >= columnIndex * numberOfColumns &&
                                                           index < (columnIndex + 1) * numberOfColumns)
                        : null)
                .Take(numberOfColumns);

        /// <summary>
        /// GroupByLines(3) :
        ///                             [1, 4, 7]
        /// [1, 2, 3, 4, 5, 6, 7]  =>   [2, 5,  ]
        ///                             [3, 6,  ]
        /// </summary>
        public static IEnumerable<IEnumerable<ElementType>> GroupByLines<ElementType>(
            this IEnumerable<ElementType> sequence,
            int numberOfLines) =>
            sequence
                .AsParallel() // à retirer si la séquence possède peu d'éléments.
                .Select((value, lineIndex) =>
                    lineIndex < numberOfLines ?
                        sequence.Where((element, index) => index % numberOfLines == lineIndex) :
                        null)
                .Take(numberOfLines);

        /// <summary>
        /// [1, 2, 3]         [1, 4, 7]
        /// [4, 5, 6]   ==>   [2, 5, 8]
        /// [7, 8, 9]         [3, 6, 9]
        /// </summary>
        public static IEnumerable<IEnumerable<ElementType>> Transpose<ElementType>(
            this IEnumerable<IEnumerable<ElementType>> jaggedArray) =>
            jaggedArray
                .AsParallel() // à retirer si le tableau en escalier possède peu d'éléments.
                .SelectMany(row => row.Select((element, index) => new { value = element, index = index }))
                .GroupBy(element => element.index, element => element.value, (index, value) => value);

        /// <summary>
        /// sequence : { Id : 1, C = 'A' }, { Id : 1, C = 'B' }, { Id : 2, C = 'C' }
        /// keySelector : (element) => element.Id
        /// 
        /// ==> [1] {{{ Id : 1, C = 'A' }, { Id : 1, C = 'B' }}
        ///     [2] { { Id : 2, C = 'C' }}
        ///     
        /// It's also possible to do :
        /// - var x = sequence.ToLookUp(element => element.Id) 
        /// - var y = x[yourId].ToList();
        /// </summary>
        public static Dictionary<KeyType, IEnumerable<ElementType>> GroupByKey<KeyType, ElementType>(this IEnumerable<ElementType> sequence, Func<ElementType, KeyType> keySelector) =>
            sequence.GroupBy(keySelector)
                    .ToDictionary(group => group.Key, group => group.AsEnumerable());
        #endregion

        #region Null Methods
        /// <summary>
        /// COLEASCE like Sql, return the firt element not null, if none are found return null.
        /// </summary>
        public static ElementType GetFirstElementNotNull<ElementType>(this IEnumerable<ElementType> sequence)
            where ElementType : class =>
            sequence.FirstOrDefault(element => element != null);

        /// <summary>
        /// NULLIF like Sql, return null if all the elements are equals, if none around found return the first not null.
        /// </summary>
        public static ElementType NullIfAllEqual<ElementType>(this IEnumerable<ElementType> sequence)
            where ElementType : class =>
            sequence.All(element => element == sequence.FirstOrDefault())
                ? null
                : sequence.First(element => element != null);
        #endregion
    }
}
