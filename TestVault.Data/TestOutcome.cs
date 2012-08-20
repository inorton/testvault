using System;

namespace TestVault.Data
{
    public enum TestOutcome 
    {
        Passed,
        Failed,
        Ignored,
        Inconclusive,
        Unknown,
    }

    public static partial class Extensions {
        public static string Code(this TestOutcome o)
        {
            switch (o)
            {
                case TestOutcome.Failed:
                    return "f";
                case TestOutcome.Ignored:
                    return "i";
                case TestOutcome.Inconclusive:
                    return "n";
                case TestOutcome.Passed:
                    return "*";
                case TestOutcome.Unknown:
                    return "?";
                default:
                    return o.ToString();
            }
        }
    }
}

