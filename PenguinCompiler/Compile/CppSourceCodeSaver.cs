using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace KSGFK
{
    public class CppSourceCodeSaver : SourceCodeSaver
    {
        private static readonly string _templateCppCodePart1 = "#include <cstdio>\n" +
                                                               "#include <iostream>\n" +
                                                               "#include <algorithm>\n" +
                                                               "#include <utility>\n" +
                                                               "#include <cmath>\n" +
                                                               "#include <string>\n" +
                                                               "#include <vector>\n" +
                                                               "#include <queue>\n" +
                                                               "#include <map>\n" +
                                                               "#include <set>\n" +
                                                               "#include <stack>\n" +
                                                               "#include <unordered_set>\n" +
                                                               "#include <unordered_map>\n" +
                                                               "#include <random>\n" +
                                                               "#include <chrono>\n" +
                                                               "#include <regex>\n" +
                                                               "#include <limits>\n" +
                                                               "#include <numeric>\n" +
                                                               "#include <functional>\n" +
                                                               "using namespace std;\n" +
                                                               "int main() {\n";

        private static readonly string _templateCppCodePart2 = "  return 0;\n" +
                                                               "}";

        public const string SpecialCompileParamOI = "oi";

        public override string TargetFileExtension => ".cpp";

        public override async ValueTask SaveAsync(
            string source,
            List<string> compileParam,
            FileInfo targetFile,
            Encoding encoding)
        {
            var dir = targetFile.Directory;
            if (dir == null)
            {
                throw new ArgumentException(nameof(targetFile));
            }
            if (!dir.Exists)
            {
                dir.Create();
            }

            await using var fileStream = targetFile.Open(FileMode.Create);
            await using var writer = new StreamWriter(fileStream, encoding);
            var oiIndex = compileParam.FindIndex(para => para == SpecialCompileParamOI);
            if (oiIndex < 0) //原样输出
            {
                await writer.WriteAsync(source);
            }
            else //有OI参数的话，写入模板代码
            {
                await writer.WriteAsync(_templateCppCodePart1);
                await writer.WriteAsync(source);
                await writer.WriteLineAsync();
                await writer.WriteAsync(_templateCppCodePart2);
                compileParam.RemoveAt(oiIndex);
            }
        }
    }
}