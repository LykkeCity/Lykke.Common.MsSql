using System;
using Autofac;
using Microsoft.EntityFrameworkCore;

namespace Lykke.Common.MsSql
{
    public static class PostgresBootstrapExtension
    {
        public static void RegisterMsSql<T>(this ContainerBuilder builder, Func<T> contextCreator) where T: MsSqlContext
        {
            using (var context = contextCreator.Invoke())
            {
                context.IsTraceEnabled = true;
                
                context.Database.Migrate();
            }
            
            builder.RegisterInstance(new MsSqlContextFactory<T>(contextCreator))
                .AsSelf()
                .SingleInstance();
        }
    }
}