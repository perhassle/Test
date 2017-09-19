using System.IO;
using System.Linq;
using DbUpWrapper.SqlWriters;

namespace DbUpWrapper.ScriptExecutors
{
    internal class SchemaScriptExecutor : ScriptExecutorBase
    {
        public SchemaScriptExecutor(ISqlWriter sqlWriter)
            : base(sqlWriter)
        {
        }

        public override string ScriptFolderName
        {
            get { return "Schema"; }
        }

        public override bool DropBeforeCreate
        {
            get { return false; }
        }

        public override string DbObjectType
        {
            get { return null; }
        }

        public override FileInfo[] GetScriptFilesToExecute(DirectoryInfo schemaScriptFolder, string version)
        {
            var scriptFile = schemaScriptFolder.GetFiles(string.Format("schema.{0}.sql", version)).FirstOrDefault();
            if (scriptFile != null && scriptFile.Exists)
            {
                return new[] { scriptFile };
            }

            return new FileInfo[]{};
        }
    }
}
