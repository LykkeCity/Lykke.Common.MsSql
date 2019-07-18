using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace Lykke.Common.MsSql
{
    public abstract class MsSqlContext : DbContext
    {
        private string _connectionString;
        private readonly string _schema;
        public bool IsTraceEnabled { set; get; }

        private readonly bool _isForMocks;

        private readonly int? _commandTimeoutSeconds;

        private const int DefaultCommandTimeout = 30;
        
        /// <summary>
        /// Constructor used for mocks.
        /// </summary>
        /// <param name="schema">Database schema name.</param>
        /// <param name="contextOptions">Context creation options.</param>
        public MsSqlContext(string schema, DbContextOptions contextOptions) : base(contextOptions)
        {
            _schema = schema;
            
            _isForMocks = true;
        }

        /// <summary>
        /// Constructor used for migrations.
        /// </summary>
        /// <param name="schema">The schema which should be used.</param>
        /// <param name="commandTimeoutSeconds">The command timeout</param>
        public MsSqlContext(string schema, int commandTimeoutSeconds = DefaultCommandTimeout)
        {
            _schema = schema;
            
            _isForMocks = false;

            _commandTimeoutSeconds = commandTimeoutSeconds;
        }

        /// <summary>
        /// Constructor used for factories etc.
        /// </summary>
        /// <param name="schema">The schema which should be used.</param>
        /// <param name="connectionString">Connection string to the database.</param>
        /// <param name="isTraceEnabled">Whether or not display EF logs.</param>
        /// <param name="commandTimeoutSeconds">The command timeout</param>
        public MsSqlContext(string schema, string connectionString, bool isTraceEnabled, int commandTimeoutSeconds = DefaultCommandTimeout)
        {
            _schema = schema;
            _connectionString = connectionString;
            
            IsTraceEnabled = isTraceEnabled;
            
            _isForMocks = false;

            _commandTimeoutSeconds = commandTimeoutSeconds;
        }

        /// <summary>
        /// Constructor used to customize db context options 
        /// </summary>
        /// <param name="options">The database context options</param>
        /// <param name="isForMocks">Designates if context will be used for mocking</param>
        /// <param name="commandTimeoutSeconds">The command timeout</param>
        public MsSqlContext(DbContextOptions options, bool isForMocks = false, int commandTimeoutSeconds = DefaultCommandTimeout) : base(options)
        {
            _isForMocks = isForMocks;

            _commandTimeoutSeconds = commandTimeoutSeconds;
        }

        protected abstract void OnLykkeConfiguring(DbContextOptionsBuilder optionsBuilder);
        
        protected sealed override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (_isForMocks)
                return;
            
            if (IsTraceEnabled)
            {
                optionsBuilder.UseLoggerFactory(new LoggerFactory(new[] {new ConsoleLoggerProvider((_, __) => true, true)}));
            }

            // Manual connection string entry for migrations.
            while (string.IsNullOrEmpty(_connectionString))    
            {
                Console.Write("Enter connection string: ");

                _connectionString = Console.ReadLine();
            }

            optionsBuilder
                .UseSqlServer(_connectionString, x => x
                    .MigrationsHistoryTable(HistoryRepository.DefaultTableName, _schema)
                    .CommandTimeout(_commandTimeoutSeconds ?? DefaultCommandTimeout))
                .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning));

            OnLykkeConfiguring(optionsBuilder);
            
            base.OnConfiguring(optionsBuilder);
        }
        
        protected abstract void OnLykkeModelCreating(ModelBuilder modelBuilder);
        
        protected sealed override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(_schema);

            OnLykkeModelCreating(modelBuilder);
            
            base.OnModelCreating(modelBuilder);
        }
    }
}