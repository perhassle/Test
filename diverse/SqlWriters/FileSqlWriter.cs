using System;
using System.Collections.Generic;
using System.IO;
using DbUp.Engine;

namespace DbUpWrapper.SqlWriters
{
    internal class FileSqlWriter : ISqlWriter
    {
        private readonly string mFilePath;
        private readonly DateTime mDateTimeOfExecution;
        private readonly string mScriptFolderName;
        private readonly string mVersion;

        public FileSqlWriter(string filePath, DateTime dateTimeOfExecution, string scriptFolderName, string version)
        {
            mFilePath = filePath;
            mDateTimeOfExecution = dateTimeOfExecution;
            mScriptFolderName = scriptFolderName;
            mVersion = version;
        }

        public int Write(List<SqlScript> scripts)
        {
            var sqlDirectoryName = "sql_" + mDateTimeOfExecution.ToString("yyyy-MM-dd HHmmss");
            var sqlDirectory = mFilePath + "\\" + sqlDirectoryName;
            var filepath = sqlDirectory + "\\" + mScriptFolderName + "." + mVersion + ".sql";

            if (Directory.Exists(sqlDirectory) == false)
            {
                Directory.CreateDirectory(sqlDirectory);
            }

            foreach (var script in scripts)
            {
                Console.WriteLine("Writing to file {0}.", filepath);
                File.AppendAllText(filepath, script.Contents);
                File.AppendAllText(filepath, Environment.NewLine);
                File.AppendAllText(filepath, "GO");
                File.AppendAllText(filepath, Environment.NewLine);
                File.AppendAllText(filepath, string.Format("-- Above written for file {0} at {1}.", script.Name, DateTime.Now));
                File.AppendAllText(filepath, Environment.NewLine);
                File.AppendAllText(filepath, Environment.NewLine);
            }

            return 0;
        }
    }
}