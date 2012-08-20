using System;
using TestVault.Data;

namespace TestVault.Core
{
    public enum Backend {
        SQLite,
        MySQL,
        Postgress,
    }

    public class TestStore
    {
        public ITestVaultData Open( Backend type )
        {
            if ( type == Backend.SQLite )
                return new TestVault.Data.SQLite.SQLiteTestData();

            throw new NotSupportedException( String.Format("unsupported backend {0}", type) );
        }
    }
}

