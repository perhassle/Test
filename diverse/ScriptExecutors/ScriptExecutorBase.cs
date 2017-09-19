using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DbUp.Engine;
using DbUpWrapper.SqlWriters;

namespace DbUpWrapper.ScriptExecutors
{
    internal abstract class ScriptExecutorBase
    {
        private readonly ISqlWriter mSqlWriter;

        protected ScriptExecutorBase(ISqlWriter sqlWriter)
        {
            mSqlWriter = sqlWriter;
        }

        public abstract string ScriptFolderName { get; }

        public abstract bool DropBeforeCreate { get; }

        public abstract string DbObjectType { get; }

        public abstract FileInfo[] GetScriptFilesToExecute(DirectoryInfo scriptsFolder, string version);
        
        public virtual ReturnCode Execute(string installationPath, string environment, string voNamesCommaSeparated, string version, string databasePrefix)
        {
            var voNamesArray = voNamesCommaSeparated.Split(',');
            var specificScriptsFolder = installationPath + "\\Scripts\\" + ScriptFolderName;
            var specificScriptsDir = new DirectoryInfo(specificScriptsFolder);
            if (specificScriptsDir.Exists == false)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Katalogen {0} existerar inte!", specificScriptsDir.FullName);
                Console.ResetColor();
                return ReturnCode.Warning;
            }

            var scriptFilesToExecute = GetScriptFilesToExecute(specificScriptsDir, version);

            var scriptsExecuted = 0;

            foreach (var scriptFile in scriptFilesToExecute)
            {
                if (DropBeforeCreate)
                {
                    var dropReturnCode = DropDbObjectIfExists(scriptFile);
                    if (dropReturnCode == -1)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Fel vid borttagning av databasobjekt motsvarande " + scriptFile);
                        Console.ResetColor();
                        return ReturnCode.Error;
                    }
                }

                var returnCode = ExecuteScript(scriptFile, environment, databasePrefix, voNamesArray);
                if (returnCode == -1)
                {
                    return ReturnCode.Error;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Script {0} kördes", scriptFile);
                Console.ResetColor();

                scriptsExecuted++;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("{0} script kördes.", scriptsExecuted);
            Console.ResetColor();

            return ReturnCode.Ok;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scriptFile"></param>
        /// <returns>-1 för fel. 0 för OK.</returns>
        private int DropDbObjectIfExists(FileInfo scriptFile)
        {
            if (string.IsNullOrEmpty(DbObjectType))
            {
                throw new ArgumentException("SysObjectsXType får inte vara tom om databasobjektet ska droppas.");
            }

            var indexOfFirstDot = scriptFile.Name.IndexOf(".", StringComparison.Ordinal);
            var indexOfSecondDot = scriptFile.Name.IndexOf(".", indexOfFirstDot + 1, StringComparison.Ordinal);
            var dbObjectName = scriptFile.Name.Substring(0, indexOfSecondDot);
            Console.WriteLine("Tar bort {0} om den existerar.", dbObjectName);

            var dropScript = string.Format(
                @"IF EXISTS (
                    SELECT * FROM sysobjects WHERE id = object_id(N'{0}')
                )
                    DROP {1} {0}
                GO",
                dbObjectName,
                DbObjectType.ToUpper());

            var scripts = new List<SqlScript> { SqlScript.FromStream(scriptFile.Name, new MemoryStream(Encoding.UTF8.GetBytes(dropScript)), Encoding.UTF8) };

            return mSqlWriter.Write(scripts);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="environment"></param>
        /// <param name="databasePrefix"></param>
        /// <param name="voNames"></param>
        /// <param name="useDebugLogging"></param>
        /// <returns>0 för OK. -1 för fel.</returns>
        public virtual int ExecuteScript(FileInfo file, string environment, string databasePrefix, string[] voNames, bool useDebugLogging = false)
        {
            Console.WriteLine("Kör script {0}", file.Name);
            var fileAsString = File.ReadAllText(file.FullName, Encoding.UTF8);

            environment = environment.ToLower();

            fileAsString = ReplaceDatabaseNames(environment, databasePrefix, voNames, fileAsString);

            if (useDebugLogging)
            {
                Console.WriteLine("Scriptets innehåll: {0}", fileAsString);
            }

            var scripts = new List<SqlScript> { SqlScript.FromStream(file.Name, new MemoryStream(Encoding.UTF8.GetBytes(fileAsString)), Encoding.UTF8) };
            return mSqlWriter.Write(scripts);
        }

        private static string ReplaceDatabaseNames(string environment, string databasePrefix, string[] voNames, string fileAsString)
        {
            foreach (var voName in voNames)
            {
                var developmentDatabaseName = "UD" + voName + "BAS";

                if (string.IsNullOrEmpty(databasePrefix))
                {
                    switch (environment)
                    {
                        case "dev":
                            fileAsString = fileAsString.Replace(developmentDatabaseName, "UD" + voName + "BAS");
                            break;
                        case "tst":
                        case "major":
                        case "minor":
                            fileAsString = fileAsString.Replace(developmentDatabaseName, "TD" + voName + "BAS");
                            break;
                        case "uat":
                            fileAsString = fileAsString.Replace(developmentDatabaseName, "ZD" + voName + "BAS");
                            break;
                        case "pro":
                            fileAsString = fileAsString.Replace(developmentDatabaseName, "PR" + voName + "BAS");
                            break;
                        case "utb":
                            fileAsString = fileAsString.Replace(developmentDatabaseName, "ED" + voName + "BAS");
                            break;
                        default:
                            throw new Exception("Uknown environment: " + environment);
                    }
                }
                else
                {
                    fileAsString = fileAsString.Replace(developmentDatabaseName, databasePrefix + voName + "BAS");
                }
            }

            return fileAsString;
        }

    }
}
