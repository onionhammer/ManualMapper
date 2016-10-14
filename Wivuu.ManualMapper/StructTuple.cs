using System;

namespace Wivuu.ManualMapper
{
    internal struct StructTuple<T, K>
    {
        public readonly T Item1;
        public readonly K Item2;

        public StructTuple(T item1, K item2)
        {
            this.Item1 = item1;
            this.Item2 = item2;
        }

        public override int GetHashCode() =>
            Item1.GetHashCode() + Item2.GetHashCode();
    }

    internal static class StructTuple
    {
        public static StructTuple<T, K> Create<T, K>(T item1, K item2) =>
            new StructTuple<T, K>(item1, item2);
    }
}
