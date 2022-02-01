using System.Diagnostics;

namespace WordMasterMind.Library.Helpers;

public static class UnitTestDetector
{
    private static bool? _isInUnitTest;

    private static readonly HashSet<string> UnitTestAttributes = new()
    {
        "Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute",
        "NUnit.Framework.TestFixtureAttribute",
    };

    /// <summary>
    ///     Use reflection to determine if the current assembly is a unit test.
    ///     Memoization prevents repeated reflection.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public static bool IsRunningInUnitTest
    {
        get
        {
            _isInUnitTest ??= new StackTrace()
                .GetFrames()
                .Any(predicate: f =>
                {
                    var method = f.GetMethod();
                    if (method == null) return false;
                    var declaringType = method.DeclaringType;
                    return declaringType != null &&
                           declaringType
                               .GetCustomAttributes(inherit: false)
                               .Any(predicate: x =>
                                   UnitTestAttributes.Contains(item: x.GetType().FullName ??
                                                                     throw new InvalidOperationException()));
                });

            return _isInUnitTest.Value;
        }
    }
}