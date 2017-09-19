using System;
using System.Collections.Generic;
using DbUp;
using DbUp.Engine;
using DbUp.Helpers;

namespace DbUpWrapper.SqlWriters
{
    internal class DatabaseSqlWriter : ISqlWriter
    {
        private readonly string mConnectionstring;

        public DatabaseSqlWriter(string connectionstring)
        {
            mConnectionstring = connectionstring;
        }

        public int Write(List<SqlScript> scripts)
        {
            var upgrader =
                DeployChanges.To
                    .SqlDatabase(mConnectionstring)
                    .WithScripts(scripts)
                    .JournalTo(new NullJournal());

            upgrader.Configure(c =>
            {
                c.ScriptExecutor.ExecutionTimeoutSeconds = 300;
                Console.WriteLine("Configure ExecutionTimeoutSeconds to " + c.ScriptExecutor.ExecutionTimeoutSeconds);
            });

            var builder = upgrader.Build();

            var result = builder.PerformUpgrade();

            if (!result.Successful)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(result.Error);
                Console.ResetColor();
                return -1;
            }

            return 0;
        }
    }
}