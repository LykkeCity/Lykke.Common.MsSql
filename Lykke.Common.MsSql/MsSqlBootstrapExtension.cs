using System;
using System.Data.Common;
using Autofac;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Lykke.Common.MsSql
{
    [PublicAPI]
    public static class MsSqlBootstrapExtension
    {
        [Obsolete("Use RegisterMsSql with dbConnString parameter")]
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

        public static void RegisterMsSql<T>(
            this ContainerBuilder builder,
            string dbConnString,
            Func<string, T> connStringCreator,
            Func<DbConnection, T> dbConnectionCreator)
            where T : MsSqlContext
        {
            using (var context = connStringCreator(dbConnString))
            {
                context.IsTraceEnabled = true;

                context.Database.Migrate();
            }

            builder.RegisterInstance(
                    new MsSqlContextFactory<T>(
                        dbConnString,
                        connStringCreator,
                        dbConnectionCreator))
                .AsSelf()
                .As<IDbContextFactory<T>>()
                .As<ITransactionRunner>()
                .SingleInstance();
        }
    }
}