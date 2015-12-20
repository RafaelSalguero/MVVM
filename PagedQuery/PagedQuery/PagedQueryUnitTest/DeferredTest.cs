using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tonic.Patterns.PagedQuery;
using Tonic.Patterns.PagedQuery.Composer;

namespace PagedQueryUnitTest
{
    [TestClass]
    public class DeferredTest
    {
        [TestMethod]
        public void ToArrayTest()
        {
            var data = Enumerable.Range(1, 10000);

            var dataQ = data.AsQueryable();
            var C = QueryFactory.Create(data);

            var c2 = C.Select((x) => x * 2).Where((x) => x > 10);

            var arr = data.Select((x) => x * 2).Where((x) => x > 10).ToArray();

            var Query = c2.ToArray();
            if (!Query.SequenceEqual(new int[] { 12, 14, 16, 18, 20 }))
                Assert.Fail();
        }

        [TestMethod]
        public void ProjectionTest()
        {
            var data = Enumerable.Range(1, 5);

            var C = QueryFactory.Create(data.AsQueryable());

            var c2 = C.Select((x) => x.ToString());

            var Query = c2.ToArray();

            if (!Query.SequenceEqual(new string[] { "1", "2", "3", "4", "5" }))
                Assert.Fail();
        }

        [TestMethod]
        public void WhereTest()
        {
            var data = Enumerable.Range(1, 10);

            var C = QueryFactory.Create(data.AsQueryable());

            var c2 = C.Where(x => x > 5).Select(x => x * 2).Select((x) => x.ToString());

            var Query = c2.ToArray();

            if (!Query.SequenceEqual(data.Where(x => x > 5).Select(x => (x * 2).ToString())))
                Assert.Fail();
        }

        [TestMethod]
        public void AggregatorTest()
        {
            var data = Enumerable.Range(1, 10);

            var C = QueryFactory.Create(data.AsQueryable());

            var c2 = C.Where(x => x > 5).Select((x) => (float)x * 0.5F);

            var Query = c2.Sum();

            Assert.AreEqual(20.0F, Query);
        }
    }
}
