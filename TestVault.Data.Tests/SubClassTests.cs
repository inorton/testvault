using System;
using NUnit.Framework;
using TestVault.NUnit;

namespace TestVault.Data.Tests
{

    public class SubClassTests : TestVaultTestBase
    {

        public override string TestVaultGroup
        {
            get
            {
                return "OtherTests";
            }
        }

        [Test]
        public void OneTest()
        {
        }
    }
}

