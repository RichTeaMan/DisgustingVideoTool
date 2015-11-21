using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoTool
{
    public class NumberSelector
    {
        /// <summary>
        /// Gets or sets maximum result in an unbounded selection.
        /// Defaults to 1000.
        /// </summary>
        public int MaxValue { get; set; } = 1000;
        public IEnumerable<int> Values(string selector)
        {
            selector = selector.Replace(" ", "");

            var selectorSplits = selector.Split(',');
            foreach (var s in selectorSplits)
            {
                var values = internalValues(s);
                foreach (var v in values)
                {
                    yield return v;
                }
            }
        }

        private IEnumerable<int> internalValues(string selector)
        {
            if (selector.Contains("-"))
            {
                return internalRange(selector);
            }
            else
            {
                var value = int.Parse(selector);
                return Enumerable.Repeat(value, 1);
            }
        }

        private IEnumerable<int> internalRange(string selector)
        {
            if (selector.Count(s => s == '-') == 1)
            {
                int begin = 0;
                int end = MaxValue;
                var splits = selector.Split('-');
                if (!string.IsNullOrEmpty(splits[0]))
                {
                    begin = int.Parse(splits[0]);
                }
                if (!string.IsNullOrEmpty(splits[1]))
                {
                    end = int.Parse(splits[1]);
                }

                if (begin >= end)
                {
                    throw new ArgumentException("Beginning of range cannot be greater than end.");
                }
                else
                {
                    return Enumerable.Range(begin, end - begin);
                }
            }
            else
            {
                throw new ArgumentException("A range must have exactly one '-'.");
            }
        }
    }
}
