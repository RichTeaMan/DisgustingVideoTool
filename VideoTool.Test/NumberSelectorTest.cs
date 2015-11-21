using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;
using System.Linq;

namespace VideoTool.Test
{
    [TestClass]
    public class NumberSelectorTest
    {
        private NumberSelector selector;
        
        [TestInitialize]
        public void Initialise()
        {
            selector = new NumberSelector();
        }

        [TestMethod]
        public void BasicRange()
        {
            var values = selector.Values("0-10");
            var result = values.ToArray();

            var expected = Enumerable.Range(0, 10).ToArray();
            CollectionAssert.AreEquivalent(expected, result);
        }

        [TestMethod]
        public void MultiRange()
        {
            var values = selector.Values("0-10, 20-30");
            var result = values.ToArray();

            var expected = Enumerable.Range(0, 10).Concat(Enumerable.Range(20, 10)).ToArray();
            CollectionAssert.AreEquivalent(expected, result);
        }

        [TestMethod]
        public void BasicList()
        {
            var values = selector.Values("1,3,5");
            var result = values.ToArray();

            var expected = new[] { 1, 3, 5 };
            CollectionAssert.AreEquivalent(expected, result);
        }

        [TestMethod]
        public void BasicListAndRange()
        {
            var values = selector.Values("1,3,5,10-20");
            var result = values.ToArray();

            var expected = new[] { 1, 3, 5 }.Concat(Enumerable.Range(10, 10)).ToArray();
            CollectionAssert.AreEquivalent(expected, result);
        }

        [TestMethod]
        public void UnboundedList()
        {
            int start = 10;
            var values = selector.Values(start + "-");
            var result = values.ToArray();

            var expected = Enumerable.Range(start, selector.MaxValue - start).ToArray();
            CollectionAssert.AreEquivalent(expected, result);
        }
    }
}
