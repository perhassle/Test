using System.IO;
using System.Linq;
using DbUpWrapper.SqlWriters;

namespace DbUpWrapper.ScriptExecutors
{
    internal class ViewsScriptExecutor : ScriptExecutorBase
    {
        public ViewsScriptExecutor(ISqlWriter sqlWriter)
            : base(sqlWriter)
        {
        }

        public override string ScriptFolderName
        {
            get { return "Views"; }
        }

        public override bool DropBeforeCreate
        {
            get { return true; }
        }

        public override string DbObjectType
        {
            get { return "VIEW"; }
        }

        public override FileInfo[] GetScriptFilesToExecute(DirectoryInfo scriptsFolder, string version)
        {
            var scriptFiles = scriptsFolder.GetFiles("*.view.sql", SearchOption.AllDirectories).ToArray();
            return scriptFiles;
        }
    }
}