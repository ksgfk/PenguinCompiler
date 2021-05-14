using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace KSGFK
{
    public class WindowsExeExecutor : Executor
    {
        public override string TargetFileExtensionName => ".exe";

        public override async Task<ExecuteResult> ExecuteAsync(
            FileInfo targetFile,
            string input,
            string workDirectory,
            CancellationToken token = default)
        {
            return await DefaultExecutorAsync(TargetFileExtensionName, targetFile, input, workDirectory, token);
        }
    }
}