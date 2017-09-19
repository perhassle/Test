using System.IO;
using System.Linq;
using DbUpWrapper.SqlWriters;

namespace DbUpWrapper.ScriptExecutors
{
    internal class StoredProceduresScriptExecutor : ScriptExecutorBase
    {
        public StoredProceduresScriptExecutor(ISqlWriter sqlWriter)
            : base(sqlWriter)
        {
        }

        public override string ScriptFolderName
        {
            get { return "Stored Procedures"; }
        }

        public override bool DropBeforeCreate
        {
            get { return true; }
        }
        
        public override string DbObjectType
        {
            get { return "PROCEDURE"; }
        }

        public override FileInfo[] GetScriptFilesToExecute(DirectoryInfo scriptsFolder, string version)
        {
            var scriptFiles = scriptsFolder.GetFiles("*.proc.sql", SearchOption.AllDirectories).ToArray();
            return scriptFiles;
        }
    }
}