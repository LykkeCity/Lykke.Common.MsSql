using System;

namespace Lykke.Common.MsSql
{
    public class MsSqlContextFactory<T>
    {
        private readonly Func<T> _contextCreator;

        public MsSqlContextFactory(Func<T> contextCreator)
        {
            _contextCreator = contextCreator;
        }

        public T CreateDataContext()
        {
            return _contextCreator.Invoke();
        }
    }
}