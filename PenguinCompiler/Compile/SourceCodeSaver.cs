using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace KSGFK
{
    public abstract class SourceCodeSaver
    {
        public abstract string TargetFileExtension { get; }

        public abstract ValueTask SaveAsync(
            string source,
            List<string> compileParam,
            FileInfo targetFile,
            Encoding encoding);
    }
}