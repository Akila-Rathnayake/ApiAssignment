using Xunit.Abstractions;
using Xunit.Sdk;

namespace ApiAssignment
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class TestPriorityAttribute : Attribute
    {
        public int Priority { get; private set; }
        public TestPriorityAttribute(int priority) => Priority = priority;
    }

    public class PriorityOrderer : ITestCaseOrderer
    {
        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
            where TTestCase : ITestCase
        {
            var sorted = new SortedDictionary<int, List<TTestCase>>();

            foreach (var testCase in testCases)
            {
                int priority = 0;

                var attr = testCase.TestMethod.Method
                    .GetCustomAttributes(typeof(TestPriorityAttribute).AssemblyQualifiedName)
                    .FirstOrDefault();

                if (attr != null)
                    priority = attr.GetNamedArgument<int>("Priority");

                if (!sorted.ContainsKey(priority))
                    sorted[priority] = new List<TTestCase>();

                sorted[priority].Add(testCase);
            }

            foreach (var list in sorted.Values)
                foreach (var testCase in list)
                    yield return testCase;
        }
    }
}
