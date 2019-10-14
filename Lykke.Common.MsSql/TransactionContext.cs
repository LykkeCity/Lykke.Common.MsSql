using System.Data.Common;

namespace Lykke.Common.MsSql
{
    public class TransactionContext
    {
        internal DbConnection DbConnection { get; }

        public TransactionContext(DbConnection dbConnection)
        {
            DbConnection = dbConnection;
        }
    }
}
