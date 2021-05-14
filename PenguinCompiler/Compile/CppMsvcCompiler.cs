using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KSGFK
{
    public class CppMsvcCompiler : Compiler
    {
        private static readonly UTF8Encoding _utf8WithBom = new(true);

        public override string SourceFileExtension => ".cpp";
        public override bool IsSourceCRLF => true;
        public override Encoding Encoding => _utf8WithBom;

        public override async Task<CompileResult> CompileAsync(
            FileInfo targetFile,
            IReadOnlyList<string> compileParams,
            string workDirectory,
            CancellationToken token = default)
        {
            if (!targetFile.Exists)
            {
                return new CompileResult
                {
                    IsSuccess = false,
                    Message = "目标源文件不存在"
                };
            }

            if (targetFile.Extension != SourceFileExtension)
            {
                return new CompileResult
                {
                    IsSuccess = false,
                    Message = $"不能编译 {targetFile.Extension} 扩展名的文件"
                };
            }

            var dir = targetFile.Directory?.FullName ?? string.Empty;
            var outputName = Path.Combine(dir, targetFile.Name + ".exe");

            var compilerProcess = new Process();
            try
            {
                var startInfo = compilerProcess.StartInfo;
                startInfo.UseShellExecute = false;
                startInfo.FileName = "cl.exe";
                startInfo.CreateNoWindow = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardInput = false; //没有输入
                startInfo.WorkingDirectory = workDirectory;

                startInfo.ArgumentList.Add("/std:c++17"); //开启C++17
                startInfo.ArgumentList.Add("/Fe:");
                startInfo.ArgumentList.Add(outputName);
                if (compileParams != null)
                {
                    foreach (var param in compileParams)
                    {
                        startInfo.ArgumentList.Add(param);
                    }
                }
                startInfo.ArgumentList.Add(targetFile.FullName);

                compilerProcess.Start();
                await compilerProcess.WaitForExitAsync(token);

                using var output = compilerProcess.StandardOutput;
                var message = await output.ReadToEndAsync();

                CompileResult result;
                if (compilerProcess.ExitCode == 0)
                {
                    result = new CompileResult
                    {
                        IsSuccess = true,
                        ExecutableFile = new FileInfo(outputName),
                        Message = message,
                        ElapsedTime = compilerProcess.GetRunningTimeMS()
                    };
                }
                else
                {
                    result = new CompileResult
                    {
                        IsSuccess = false,
                        ExecutableFile = null,
                        Message = message,
                        ElapsedTime = 0
                    };
                }
                return result;
            }
            catch (Exception e)
            {
                return new CompileResult
                {
                    IsSuccess = false,
                    Message = $"未能成功编译，原因:{e.Message}"
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