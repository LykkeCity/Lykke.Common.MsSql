using JetBrains.Annotations;

namespace Lykke.Common.MsSql
{
    [PublicAPI]
    public interface IDbContextFactory<out T>
    {
        T CreateDataContext();

        T CreateDataContext(TransactionContext transactionContext);
    }
}