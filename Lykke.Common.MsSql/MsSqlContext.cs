using System;
using System.Data.Common;
using System.Linq;
using Falcon.Numerics;
using JetBrains.Annotations;
using Lykke.Common.MsSql.Attributes;
using Lykke.Common.MsSql.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace Lykke.Common.MsSql
{
    [PublicAPI]
    public abstract class MsSqlContext : DbContext
    {
        private const int DefaultCommandTimeout = 30;

        private readonly string _schema;
        private readonly bool _isForMocks;
        private readonly int? _commandTimeoutSeconds;
        private readonly DbConnection _dbConnection;

        private string _connectionString;

        public bool IsTraceEnabled { set; get; }

        /// <summary>
        /// Constructor used for migrations.
        /// </summary>
        protected MsSqlContext(string schema, int commandTimeoutSeconds = DefaultCommandTimeout)
        {
            _schema = schema;
            _commandTimeoutSeconds = commandTimeoutSeconds;
        }

        /// <summary>
        /// Constructor used for factory.
        /// </summary>
        protected MsSqlContext(
            string schema,
            string connectionString,
            bool isTraceEnabled,
            int commandTimeoutSeconds = DefaultCommandTimeout)
        {
            _schema = schema;
            _connectionString = connectionString;

            IsTraceEnabled = isTraceEnabled;

            _isForMocks = false;

            _commandTimeoutSeconds = commandTimeoutSeconds;
        }

        /// <summary>
        /// Constructor used for mocks.
        /// </summary>
        protected MsSqlContext(string schema, DbContextOptions contextOptions)
            : this(schema, contextOptions, true)
        {
        }

        /// <summary>
        /// Constructor used to customize db context options 
        /// </summary>
        protected MsSqlContext(
            string schema,
            DbContextOptions options,
            bool isForMocks = false,
            int commandTimeoutSeconds = DefaultCommandTimeout)
            : base(options)
        {
            _schema = schema;
            _isForMocks = isForMocks;
            _commandTimeoutSeconds = commandTimeoutSeconds;
        }

        /// <summary>
        /// Constructor used for factory with db connection and customization.
        /// </summary>
        protected MsSqlContext(
            string schema,
            DbConnection dbConnection,
            bool isForMocks = false,
            int commandTimeoutSeconds = DefaultCommandTimeout)
        {
            _schema = schema;
            _dbConnection = dbConnection;
        }

        protected virtual void OnLykkeConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected sealed override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (_isForMocks)
                return;

            if (IsTraceEnabled)
            {
                var loggerFactory =
#if (NETCOREAPP3_0 || NETCOREAPP3_1)
                LoggerFactory.Create(builder => { builder.AddConsole(); });
#elif NETSTANDARD2_0
                new LoggerFactory(new[] { new ConsoleLoggerProvider((_, __) => true, true) });
#else
#error unknown target framework
#endif
                optionsBuilder.UseLoggerFactory(loggerFactory);
            }

            if (_dbConnection == null)
            {
                // Manual connection string entry for migrations.
                while (string.IsNullOrEmpty(_connectionString))
                {
                    Console.Write("Enter connection string: ");

                    _connectionString = Console.ReadLine();
                }
                optionsBuilder = optionsBuilder
                    .UseSqlServer(_connectionString, x => x
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, _schema)
                        .CommandTimeout(_commandTimeoutSeconds ?? DefaultCommandTimeout));
            }
            else
            {
                optionsBuilder = optionsBuilder
                    .UseSqlServer(_dbConnection, x => x
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, _schema)
                        .CommandTimeout(_commandTimeoutSeconds ?? DefaultCommandTimeout));
            }

            optionsBuilder.ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning));

            OnLykkeConfiguring(optionsBuilder);

            base.OnConfiguring(optionsBuilder);
        }

        protected abstract void OnLykkeModelCreating(ModelBuilder modelBuilder);

        protected sealed override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(_schema);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var properties = entityType.ClrType.GetProperties();

                var money18Properties = properties.Where(p => p.PropertyType == typeof(Money18));
                foreach (var property in money18Properties)
                {
                    modelBuilder
                        .Entity(entityType.Name)
                        .Property(property.Name)
                        .HasConversion(
                            Attribute.IsDefined(property, typeof(Padding))
                                ? Money18PaddedConverter.Instance
                                : (ValueConverter)Money18Converter.Instance);
                }

                var nullableMoney18Properties = properties.Where(p => p.PropertyType == typeof(Money18?));
                foreach (var property in nullableMoney18Properties)
                {
                    modelBuilder
                        .Entity(entityType.Name)
                        .Property(property.Name)
                        .HasConversion(
                            Attribute.IsDefined(property, typeof(Padding))
                            ? NullableMoney18PaddedConverter.Instance
                            : (ValueConverter)NullableMoney18Converter.Instance);
                }
            }

            OnLykkeModelCreating(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }
    }
}
