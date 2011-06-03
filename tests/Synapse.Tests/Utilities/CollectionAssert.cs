using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Synapse.Tests.Utilities
{
    using MSTestCollectionAssert = Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert;
    public static class CollectionAssert
    {
        public static void AreElementsEqual<T>(IEnumerable<T> expectedSequence, IEnumerable<T> actualSequence)
        {
            MSTestCollectionAssert.AreEqual(expectedSequence.ToList(), actualSequence.ToList());
        }
    }
}
