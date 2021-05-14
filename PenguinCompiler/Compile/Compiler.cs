using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace KSGFK
{
    public record CompileResult
    {
        public bool IsSuccess { get; init; }
        [JsonIgnore] public FileInfo ExecutableFile { get; init; }
        public string Message { get; init; }
        public double ElapsedTime { get; init; }
    }

    /// <summary>
    /// 编译器
    /// </summary>
    public abstract class Compiler
    {
        /// <summary>
        /// 源文件扩展名
        /// </summary>
        public abstract string SourceFileExtension { get; }

        /// <summary>
        /// 源文件是否是/r/n换行
        /// </summary>
        public abstract bool IsSourceCRLF { get; }

        /// <summary>
        /// 源文件编码器
        /// </summary>
        public abstract Encoding Encoding { get; }

        /// <summary>
        /// 编译
        /// </summary>
        /// <param name="targetFile">目标源文件</param>
        /// <param name="compileParams">编译参数</param>
        /// <param name="workDirectory">编译工作目录</param>
        /// <param name="token">超时结束</param>
        /// <returns>编译结果</returns>
        public abstract Task<CompileResult> CompileAsync(
            FileInfo targetFile,
            IReadOnlyList<string> compileParams,
            string workDirectory,
            CancellationToken token = default);
    }
}