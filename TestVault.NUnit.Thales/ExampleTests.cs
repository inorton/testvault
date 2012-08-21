using System;
using NUnit.Framework;

namespace TestVault.NUnit.Thales
{
    [TestFixture]
    public class ExampleTests : TesTestBase
    {
        [Test]
        [Repeat(100)]
        public void TestRepeatedTest()
        {
        }

        [Test]
        public void TestInconclusiveTest()
        {
            Assert.Inconclusive("dunno");
        }

        [Test]
        public void TestOccasaionallyFails()
        {
            var rnd = new System.Security.Cryptography.RNGCryptoServiceProvider();
            var buf = new byte[1];
            rnd.GetBytes(buf);
            if ( buf[0] > 100 ) Assert.Fail("too big");
        }

        [Test]
        public void TestAlwaysFails()
        {
            Assert.Fail("never");
        }

        [Test]
        public void TestAlwaysIgnored()
        {
            Assert.Ignore("skip");
        }

        [Test]
        public void TestOccasaionallyIgnored()
        {
            var rnd = new System.Security.Cryptography.RNGCryptoServiceProvider();
            var buf = new byte[1];
            rnd.GetBytes(buf);
            if ( buf[0] > 100 ) Assert.Ignore("too big");
        }
    }
}

