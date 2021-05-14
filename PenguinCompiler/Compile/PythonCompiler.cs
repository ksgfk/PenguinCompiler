using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KSGFK
{
    public class PythonCompiler : Compiler
    {
        public override string SourceFileExtension => ".py";

        public override bool IsSourceCRLF => false;

        public override Encoding Encoding => Encoding.UTF8;

        public override async Task<CompileResult> CompileAsync(
            FileInfo targetFile,
            IReadOnlyList<string> compileParams,
            string workDirectory,
            CancellationToken token = default)
        {
            return new()
            {
                IsSuccess = true,
                ExecutableFile = targetFile,
                Message = string.Empty,
                ElapsedTime = 0
            };
        }
    }
}