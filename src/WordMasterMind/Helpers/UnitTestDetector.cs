using System.Diagnostics;

namespace WordMasterMind.Helpers;

public static class UnitTestDetector
{
    public static readonly HashSet<string> UnitTestAttributes = new()
    {
        "Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute",
        "NUnit.Framework.TestFixtureAttribute"
    };

    public static bool IsRunningInUnitTest
    {
        get
        {
            return new StackTrace().GetFrames()
                .Any(predicate: f =>
                {
                    var method = f.GetMethod();
                    if (method == null) return false;
                    var declaringType = method.DeclaringType;
                    return declaringType != null && declaringType.GetCustomAttributes(inherit: false)
                        .Any(predicate: x =>
                            UnitTestAttributes.Contains(item: x.GetType().FullName ??
                                                              throw new InvalidOperationException()));
                });
        }
    }
}