using System.IO;
using System.Linq;
using DbUpWrapper.SqlWriters;

namespace DbUpWrapper.ScriptExecutors
{
    internal class CustomScriptExecutor : ScriptExecutorBase
    {
        public CustomScriptExecutor(ISqlWriter sqlWriter)
            : base(sqlWriter)
        {
        }

        public override string ScriptFolderName
        {
            get { return string.Empty; }
        }

        public override bool DropBeforeCreate
        {
            get { return false; }
        }

        public override string DbObjectType
        {
            get { return null; }
        }

        public override FileInfo[] GetScriptFilesToExecute(DirectoryInfo scriptsFolder, string version)
        {
           return new FileInfo[] { };
        }
    }
}