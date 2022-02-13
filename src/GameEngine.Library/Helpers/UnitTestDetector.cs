using System.Diagnostics;

namespace GameEngine.Library.Helpers;

public static class UnitTestDetector
{
    /// <summary>
    ///     memoized value of whether we're running in unit test mode
    /// </summary>
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
        => _isInUnitTest ??= new StackTrace()
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

    public static void ForceTestMode(bool? value)
    {
        _isInUnitTest = value;
    }
}