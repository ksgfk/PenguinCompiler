using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace KSGFK
{
    public class PythonExecutor : Executor
    {
        public override string TargetFileExtensionName => ".py";

        public override async Task<ExecuteResult> ExecuteAsync(
            FileInfo targetFile,
            string input,
            string workDirectory,
            CancellationToken token = default)
        {
            var compilerProcess = new Process();
            try
            {
                var startInfo = compilerProcess.StartInfo;
                startInfo.UseShellExecute = false;
                startInfo.FileName = "py";
                startInfo.CreateNoWindow = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardInput = true;
                startInfo.WorkingDirectory = workDirectory;
                startInfo.ArgumentList.Add(targetFile.FullName);

                compilerProcess.Start();
                if (!string.IsNullOrEmpty(input))
                {
                    await using var inputStream = compilerProcess.StandardInput;
                    await inputStream.WriteAsync(input);
                }
                await compilerProcess.WaitForExitAsync(token);

                using var outputStream = compilerProcess.StandardOutput;
                var output = await outputStream.ReadToEndAsync();

                return new ExecuteResult
                {
                    ExitCode = compilerProcess.ExitCode,
                    Output = output,
                    ElapsedTime = compilerProcess.GetRunningTimeMS()
                };
            }
            catch (TaskCanceledException)
            {
                return new ExecuteResult
                {
                    ExitCode = -233,
                    Output = "TLE"
                };
            }
            catch (Exception e)
            {
                return new ExecuteResult
                {
                    ExitCode = -233,
                    Output = $"未成功执行,原因:{e.Message}"
                };
            }
            finally
            {
                compilerProcess.Kill(true);
                compilerProcess.Close();
                compilerProcess.Dispose();
            }
        }
    }
}