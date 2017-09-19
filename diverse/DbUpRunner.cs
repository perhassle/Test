using System;
using System.Collections.Generic;
using System.IO;
using DbUpWrapper.ScriptExecutors;
using DbUpWrapper.SqlWriters;

namespace DbUpWrapper
{
    public class DbUpRunner
    {
        /// <summary>
        /// Skriver till fil eller databas beroende på om connectionstring eller targetscriptfilepath är angivet.
        /// </summary>
        /// <param name="args">args kan ha följande innehåll:-connectionstring:"Data Source=SQLDEV6;Initial Catalog=master;Integrated Security=True" -installationpath:"C:\net\0570\App\Web\A0012\Internetkontoret.DatabaseScripts" -environment:"UAT" -vonames:"ALY,ALX,GRY,APR,AAI" -version:"2.31" -targetscriptfilepath:"C:\Temp\Script"</param>
        /// <returns></returns>
        public int Run(string[] args)
        {
            var runner = new DbUpRunner();
            var connectionString = GetParameterValue(args, "connectionstring");
            var scriptFilePath = GetParameterValue(args, "targetscriptfilepath");
            var installationPath = GetParameterValue(args, "installationpath");
            var environment = GetParameterValue(args, "environment");
            var voNames = GetParameterValue(args, "vonames");
            var version = GetParameterValue(args, "version");
            var databasePrefix = GetParameterValue(args, "databaseprefix");

            if (string.IsNullOrWhiteSpace(connectionString + scriptFilePath))
            {
                Console.WriteLine("Växlarna connectionstring eller scriptfilepath måste anges.");
                throw new ArgumentException("Växlarna connectionstring eller scriptfilepath måste anges.");
            }

            if (string.IsNullOrWhiteSpace(installationPath + environment + voNames + version))
            {
                Console.WriteLine("Växlarna installationPath, environment, voNames, och version måste anges.");
                throw new ArgumentException("Växlarna installationPath, environment, voNames, och version måste anges.");
            }

            var returnvalue = 0;
            if (string.IsNullOrEmpty(scriptFilePath) == false)
            {
                returnvalue = runner.CreateScriptFiles(
                    installationPath,
                    environment,
                    voNames,
                    version,
                    scriptFilePath,
                    databasePrefix);
            }
            else
            {
                returnvalue = runner.UpgradeDatabase(
                           connectionString,
                           installationPath,
                           environment,
                           voNames,
                           version);
            }

            return returnvalue;
        }

        public int RunSpecific(string[] args)
        {
            var connectionString = GetParameterValue(args, "connectionstring");
            var installationPath = GetParameterValue(args, "installationpath");
            var scriptFileName = GetParameterValue(args, "scriptFileName");
            var environment = GetParameterValue(args, "environment");
            var voNames = GetParameterValue(args, "vonames");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                Console.WriteLine("Växeln connectionstring måste anges.");
                throw new ArgumentException("Växeln connectionstring måste anges.");
            }

            if (string.IsNullOrWhiteSpace(installationPath + scriptFileName + environment + voNames))
            {
                Console.WriteLine("Växlarna installationPath, scriptFileName, environment och voNames måste anges.");
                throw new ArgumentException("Växlarna installationPath, scriptFileName, environment och voNames måste anges.");
            }

            var executor = new CustomScriptExecutor(new DatabaseSqlWriter(connectionString));

            var returnvalue = executor.ExecuteScript(new FileInfo(Path.Combine(installationPath, scriptFileName)), environment, "", voNames.Split(','), true);

            return returnvalue;
        }

        /// <param name="installationPath">Komplett sökväg till katalog där Scriptskatalogen ligger</param>
        /// <param name="environment">dev, tst, uat, pro</param>
        /// <param name="voNamesCommaSeparated">Namn på VO, t.ex. ALY</param>
        /// <param name="version">Oftast Major.Minor. Vid delleverans används Major.Minor.Revision</param>
        /// <param name="writeScriptFileToPath">Dit scriptfilen ska skrivas om fil ska skrivas istället för databasuppdatering.</param>
        /// <param name="databasePrefix"></param>
        /// <returns>0 om det gick bra, -1 om något gick fel</returns>
        public int CreateScriptFiles(string installationPath, string environment, string voNamesCommaSeparated, string version, string writeScriptFileToPath, string databasePrefix)
        {
            Console.WriteLine("Tar emot följande parametrar:");
            Console.WriteLine("installationPath: " + installationPath);
            Console.WriteLine("environment: " + environment);
            Console.WriteLine("version: " + version);
            Console.WriteLine("voNamesCommaSeparated: " + voNamesCommaSeparated);
            Console.WriteLine("writeScriptFileToPath: " + writeScriptFileToPath);
            Console.WriteLine("databasePrefix: " + databasePrefix);

            var dateTimeOfExecution = DateTime.Now;

            ScriptExecutorBase[] scriptExecutors =
            {
                new SchemaScriptExecutor(new FileSqlWriter(writeScriptFileToPath, dateTimeOfExecution, "Schema", version)),
                new FunctionsScriptExecutor(new FileSqlWriter(writeScriptFileToPath, dateTimeOfExecution, "Functions", version)),
                new StoredProceduresScriptExecutor(new FileSqlWriter(writeScriptFileToPath, dateTimeOfExecution, "Stored Procedures", version)),
                new ViewsScriptExecutor(new FileSqlWriter(writeScriptFileToPath, dateTimeOfExecution, "Views", version)),
                new ContentScriptExecutor(new FileSqlWriter(writeScriptFileToPath, dateTimeOfExecution, "Content", version))
            };

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Genererar scriptfiler för {0} i miljö {1} och version {2}", voNamesCommaSeparated, environment, version);
            Console.ResetColor();

            return RunScriptExecutors(installationPath, environment, voNamesCommaSeparated, version, databasePrefix, scriptExecutors);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString">Databaskoppling</param>
        /// <param name="installationPath">Komplett sökväg till katalog där Scriptskatalogen ligger</param>
        /// <param name="environment">dev, tst, uat, pro</param>
        /// <param name="voNamesCommaSeparated">Namn på VO, t.ex. ALY</param>
        /// <param name="version">Oftast Major.Minor. Vid delleverans används Major.Minor.Revision</param>
        /// <returns>0 om det gick bra, -1 om något gick fel</returns>
        public int UpgradeDatabase(
            string connectionString,
            string installationPath,
            string environment,
            string voNamesCommaSeparated,
            string version)
        {
            Console.WriteLine("Tar emot följande parametrar:");
            Console.WriteLine("connectionString: " + connectionString);
            Console.WriteLine("installationPath: " + installationPath);
            Console.WriteLine("environment: " + environment);
            Console.WriteLine("voNamesCommaSeparated: " + voNamesCommaSeparated);
            Console.WriteLine("version: " + version);

            ISqlWriter sqlWriter = new DatabaseSqlWriter(connectionString);

            ScriptExecutorBase[] scriptExecutors =
            {
                new SchemaScriptExecutor(sqlWriter),
                new FunctionsScriptExecutor(sqlWriter),
                new StoredProceduresScriptExecutor(sqlWriter),
                new ViewsScriptExecutor(sqlWriter),
                new ContentScriptExecutor(sqlWriter)
            };

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Uppdaterar databas för {0} i miljö {1} och version {2}", voNamesCommaSeparated, environment, version);
            Console.ResetColor();

            return RunScriptExecutors(installationPath, environment, voNamesCommaSeparated, version, "", scriptExecutors);
        }

        private static int RunScriptExecutors(
            string installationPath,
            string environment,
            string voNamesCommaSeparated,
            string version,
            string databasePrefix,
            IEnumerable<ScriptExecutorBase> scriptExecutors)
        {
            foreach (var scriptExecutor in scriptExecutors)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Kör " + scriptExecutor.GetType().FullName);
                Console.ResetColor();

                var returnCode = scriptExecutor.Execute(installationPath, environment, voNamesCommaSeparated, version, databasePrefix);
                if (returnCode == ReturnCode.Error)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Fel vid körning av {0}.", scriptExecutor.GetType().FullName);
                    Console.ResetColor();
                    return -1;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Körning av {0} OK.", scriptExecutor.GetType().FullName);
                Console.ResetColor();
            }

            return 0;
        }

        private static string GetParameterValue(string[] args, string key)
        {
            foreach (var arg in args)
            {
                var pattern = "-" + key + ":";
                if (arg.StartsWith(pattern, StringComparison.InvariantCultureIgnoreCase))
                {
                    return arg.Replace(pattern, string.Empty);
                }
            }

            return null;
        }
    }
}
