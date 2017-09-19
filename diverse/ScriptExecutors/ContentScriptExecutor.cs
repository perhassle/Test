using System.IO;
using System.Linq;
using DbUpWrapper.SqlWriters;

namespace DbUpWrapper.ScriptExecutors
{
    internal class ContentScriptExecutor : ScriptExecutorBase
    {
        public ContentScriptExecutor(ISqlWriter sqlWriter)
            : base(sqlWriter)
        {
        }

        public override string ScriptFolderName
        {
            get { return "Content"; }
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
            var insertScriptFile = scriptsFolder.GetFiles(string.Format("Content*Inserts.{0}*.sql", version)).FirstOrDefault();
            var updateScriptFile = scriptsFolder.GetFiles(string.Format("Content*Updates.{0}*.sql", version)).FirstOrDefault();

            if (insertScriptFile != null 
                && insertScriptFile.Exists
                && updateScriptFile != null
                && updateScriptFile.Exists)
            {
                return new[] { insertScriptFile, updateScriptFile };
            }

            if (insertScriptFile != null && insertScriptFile.Exists)
            {
                return new[] { insertScriptFile };
            }

            if (updateScriptFile != null && updateScriptFile.Exists)
            {
                return new[] { updateScriptFile };
            }

            return new FileInfo[] { };
        }
    }
}