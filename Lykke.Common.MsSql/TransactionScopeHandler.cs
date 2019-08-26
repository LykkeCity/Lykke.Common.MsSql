using System;
using System.Threading.Tasks;
using System.Transactions;
using Lykke.Common.MsSql.Exceptions;

namespace Lykke.Common.MsSql
{
    public class TransactionScopeHandler
    {
        public async Task<T> WithTransactionAsync<T>(Func<Task<T>> func)
        {
            try
            {
                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var result = await func();

                    scope.Complete();

                    return result;
                }
            }
            catch (TransactionAbortedException e)
            {
                throw new CommitTransactionException();
            }
        }
        
        public async Task WithTransactionAsync(Func<Task> action)
        {
            try
            {
                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    await action();

                    scope.Complete();
                }
            }
            catch (TransactionAbortedException e)
            {
                throw new CommitTransactionException();
            }
        }
    }
}