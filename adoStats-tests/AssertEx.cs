using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace adoStats_tests
{
    public static class AssertEx
    {
        public static void ComplexAreEqual<T>(this Assert assert, T expected, T actual)
        {
            var expectedSerialized = JsonSerializer.Serialize<T>(expected);
            var actualSerialized = JsonSerializer.Serialize<T>(actual);
            Assert.AreEqual(expectedSerialized,actualSerialized);
        }
    }
}
