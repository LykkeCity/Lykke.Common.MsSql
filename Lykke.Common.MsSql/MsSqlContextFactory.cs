using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Transactions;
using JetBrains.Annotations;

namespace Lykke.Common.MsSql
{
    [PublicAPI]
    public class MsSqlContextFactory<T> : IDbContextFactory<T>, ITransactionRunner
    {
        private readonly string _dbConnString;
        private readonly Func<T> _contextCreator;
        private readonly Func<string, T> _connStringCreator;
        private readonly Func<DbConnection, T> _dbConnectionCreator;

        [Obsolete("Use c-tor with dbConnString parameter")]
        public MsSqlContextFactory(Func<T> contextCreator)
        {
            _contextCreator = contextCreator;
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
            if (_dbConnectionCreator == null)
                throw new InvalidOperationException("C-tor for db context using db connection is not defined. Old MsSqlContextFactory c-tor was used.");

            return _dbConnectionCreator(transactionContext.DbConnection);
        }

        public async Task<TK> RunWithTransactionAsync<TK>(Func<TransactionContext, Task<TK>> func)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
            {
                using (var connection = new SqlConnection(_dbConnString))
                {
                    connection.Open();

                    var txContext = new TransactionContext { DbConnection = connection };

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

                    var txContext = new TransactionContext { DbConnection = connection };

                    await action(txContext);

                    scope.Complete();
                }
            }
        }
    }
}