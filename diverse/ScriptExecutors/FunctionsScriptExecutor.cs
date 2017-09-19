using System.IO;
using System.Linq;
using DbUpWrapper.SqlWriters;

namespace DbUpWrapper.ScriptExecutors
{
    internal class FunctionsScriptExecutor : ScriptExecutorBase
    {
        public FunctionsScriptExecutor(ISqlWriter sqlWriter)
            : base(sqlWriter)
        {
        }

        public override string ScriptFolderName
        {
            get { return "Functions"; }
        }

        public override bool DropBeforeCreate
        {
            get { return true; }
        }

        public override string DbObjectType
        {
            get { return "FUNCTION"; }
        }

        public override FileInfo[] GetScriptFilesToExecute(DirectoryInfo scriptsFolder, string version)
        {
            var scriptFilePaths = scriptsFolder.GetFiles("*.function.sql", SearchOption.AllDirectories).ToArray();
            return scriptFilePaths;
        }
    }
}