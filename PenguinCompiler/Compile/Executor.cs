using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace KSGFK
{
    public record ExecuteResult
    {
        public int ExitCode { get; init; }
        public double ElapsedTime { get; init; }
        public string Output { get; init; }

        public string GetReplyString() { return $"结束码:{ExitCode}\n输出:{Output}\n耗时:{ElapsedTime}"; }
    }

    public abstract class Executor
    {
        public abstract string TargetFileExtensionName { get; }

        public abstract Task<ExecuteResult> ExecuteAsync(
            FileInfo targetFile,
            string input,
            string workDirectory,
            CancellationToken token = default);

        public static async Task<ExecuteResult> DefaultExecutorAsync(
            string ext,
            FileInfo targetFile,
            string input,
            string workDirectory,
            CancellationToken token = default)
        {
            if (!targetFile.Exists)
            {
                return new ExecuteResult
                {
                    ExitCode = -233,
                    Output = "目标可执行文件不存在"
                };
            }

            if (targetFile.Extension != ext)
            {
                return new ExecuteResult
                {
                    ExitCode = -233,
                    Output = $"不能执行 {targetFile.Extension} 扩展名的文件"
                };
            }

            var compilerProcess = new Process();
            try
            {
                var startInfo = compilerProcess.StartInfo;
                startInfo.UseShellExecute = false;
                startInfo.FileName = targetFile.FullName;
                startInfo.CreateNoWindow = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardInput = true;
                startInfo.WorkingDirectory = workDirectory;

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