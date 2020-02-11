using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Transactions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Lykke.Common.MsSql
{
    /// <summary>
    /// Factory for db context creation.
    /// </summary>
    /// <typeparam name="T">DB context type.</typeparam>
    [PublicAPI]
    public class MsSqlContextFactory<T> : IDbContextFactory<T>, ITransactionRunner
        where T : DbContext
    {
        private readonly string _dbConnString;
        private readonly Func<T> _contextCreator;
        private readonly Func<string, T> _connStringCreator;
        private readonly Func<DbConnection, T> _dbConnectionCreator;

        [Obsolete("Use other c-tors")]
        public MsSqlContextFactory(Func<T> contextCreator)
        {
            _contextCreator = contextCreator;
        }

        public MsSqlContextFactory(Func<DbContextOptions, T> contextCreator, DbContextOptions contextOptions)
        {
            _contextCreator = () => contextCreator(contextOptions);
        }

        public MsSqlContextFactory(
            string dbConnString,
            Func<string, T> connStringCreator,
            Func<DbConnection, T> dbConnectionCreator)
        {
            _dbConnString = dbConnString ?? throw new ArgumentNullException(nameof(dbConnString));
            _connStringCreator = connStringCreator ?? throw new ArgumentNullException(nameof(connStringCreator));
            _dbConnectionCreator = dbConnectionCreator ?? throw new ArgumentNullException(nameof(dbConnectionCreator));
        }

        public T CreateDataContext()
        {
            return _connStringCreator != null
                ? _connStringCreator(_dbConnString)
                : _contextCreator();
        }

        public T CreateDataContext(TransactionContext transactionContext)
        {
            if (_dbConnectionCreator == null || transactionContext?.DbConnection == null)
                return CreateDataContext();

            return _dbConnectionCreator(transactionContext.DbConnection);
        }

        public async Task<TK> RunWithTransactionAsync<TK>(Func<TransactionContext, Task<TK>> func)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
            {
                using (var connection = new SqlConnection(_dbConnString))
                {
                    connection.Open();

                    var txContext = new TransactionContext(connection);

                    var result = await func(txContext);

                    scope.Complete();

                    return result;
                }
            }
        }

        public async Task RunWithTransactionAsync(Func<TransactionContext, Task> action)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
            {
                using (var connection = new SqlConnection(_dbConnString))
                {
                    connection.Open();

                    var txContext = new TransactionContext(connection);

                    await action(txContext);

                    scope.Complete();
                }
            }
        }
    }
}