using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace KSGFK
{
    public class PenguinCompilerService : Service<ChatMessageEventArgs>
    {
        public record CompileData
        {
            public bool IsHelp { get; set; }
            public string Compiler { get; init; }
            public List<string> CompileParams { get; init; }
            public int SourceStartPos { get; init; }
            public int SourceEndPos { get; init; }
            public bool IsCRLF { get; init; }
            public string Input { get; init; }
        }

        public const string CompileCmd = "cl";
        public const string InputCmd = "in";
        public const string SourceFileName = "temp";

        private readonly OneBotMessageSegment[] _invalidCompileCmd = new OneBotMessageSegment[1];
        private readonly OneBotMessageSegment[] _invalidCompilerName = new OneBotMessageSegment[1];
        private readonly OneBotMessageSegment[] _unknownError = new OneBotMessageSegment[1];
        private readonly OneBotMessageSegment[][] _help;

        private readonly Dictionary<string, Compiler> _compilers;
        private readonly Dictionary<string, string> _compilerNickname;
        private readonly Dictionary<string, SourceCodeSaver> _codeSavers;
        private readonly Dictionary<string, Executor> _executors;

        public string SourceCachePath { get; }
        public int MaxExecuteTime { get; }

        public PenguinCompilerService(string sourceCachePath, int maxExecuteTime, int maxRequest, int maxTask)
            : base(maxRequest,
                maxTask)
        {
            SourceCachePath = sourceCachePath;
            MaxExecuteTime = maxExecuteTime;
            _compilers = new Dictionary<string, Compiler>();
            _compilerNickname = new Dictionary<string, string>();
            _executors = new Dictionary<string, Executor>();
            _codeSavers = new Dictionary<string, SourceCodeSaver>();
            _help = new[] {new OneBotMessageSegment[1], new OneBotMessageSegment[1], new OneBotMessageSegment[1]};
        }

        public void AddCompiler(string name, Compiler compiler)
        {
            _compilers.Add(name, compiler);
            AddCompilerNickname(name, name);
        }

        public void AddExecutor(Executor executor) { _executors.Add(executor.TargetFileExtensionName, executor); }

        /// <summary>
        /// ?????? C??? -> cpp???C++ -> cpp ??????????????????
        /// </summary>
        /// <param name="targetCompiler">?????????????????????</param>
        /// <param name="nickname">????????????</param>
        public void AddCompilerNickname(string targetCompiler, string nickname)
        {
            if (!_compilers.ContainsKey(targetCompiler))
            {
                throw new ArgumentException($"????????????????????????{targetCompiler}");
            }
            _compilerNickname.Add(nickname, targetCompiler);
        }

        public void AddSourceCodeSaver(SourceCodeSaver saver) { _codeSavers.Add(saver.TargetFileExtension, saver); }

        protected override void Start()
        {
            SetConstantValue();
            Console.WriteLine("start compile service...");
        }

        private void SetConstantValue()
        {
            var sb = new StringBuilder("?????????????????????\n");
            foreach (var nickname in _compilerNickname.Keys)
            {
                sb.Append(nickname).Append('\n');
            }
            var available = sb.ToString();

            _invalidCompileCmd[0] = new OneBotMessageSegmentText
            {
                Text = $"???????????????????????????cl help"
            };
            _invalidCompilerName[0] = new OneBotMessageSegmentText
            {
                Text = available
            };
            _unknownError[0] = new OneBotMessageSegmentText
            {
                Text = "??????????????????????????????????????????????????????"
            };
            _help[0][0] = new OneBotMessageSegmentText
            {
                Text = $"????????????({{}}???????????????[]????????????)???\n{CompileCmd} {{??????}} [??????]{{??????}}\n{{??????}}[??????]\n[in]\n[????????????]\n" +
                       "--1/3(??????:cl help [??????])--"
            };
            _help[1][0] = new OneBotMessageSegmentText
            {
                Text = available + "--2/3(??????:cl help [??????])--"
            };
            _help[2][0] = new OneBotMessageSegmentText
            {
                Text = "C++?????????????????????????????????????????? oi ?????????????????????????????????\n" +
                       "?????????\n#include ...//????????????????????????\nint main() {\n  ##???????????????????????????\n  return 0;\n}\n" +
                       "?????????\n" +
                       "cl cpp oi\nprintf(\"HELLO!\");\n" +
                       "???????????????\n#include ...//??????????????????\nint main() {\n  printf(\"HELLO!\");\n  return 0;\n}\n" +
                       "--3/3(??????:cl help [??????])--"
            };
        }

        protected override async Task RunTaskAsync(ChatMessageEventArgs request)
        {
            try
            {
                if (!IsCompileCmd(request, out var msg))
                {
                    return;
                }
                if (!ParseCompileCmd(msg, out var data))
                {
                    await SendReplyMessageAsync(request, _invalidCompileCmd);
                    return;
                }
                if (data.IsHelp)
                {
                    int page;
                    if (data.CompileParams.Count == 0)
                    {
                        page = 1;
                    }
                    else
                    {
                        var p = int.Parse(data.CompileParams[0]);
                        page = p is <= 0 or > 3 ? 1 : p;
                    }
                    await SendReplyMessageAsync(request, _help[page - 1]);
                    return;
                }

                if (!_compilerNickname.TryGetValue(data.Compiler, out var targetCompiler))
                {
                    await SendReplyMessageAsync(request, _invalidCompilerName);
                    return;
                }

                var compiler = _compilers[targetCompiler];
                var id = GetGuid();
                var dir = GetCacheDirectory(SourceCachePath, request.GetUserId(), id);
                var fileName = $"{SourceFileName}{compiler.SourceFileExtension}";
                var fileInfo = new FileInfo(Path.Combine(dir, fileName));
                var source = msg[data.SourceStartPos..data.SourceEndPos];
                if (_codeSavers.TryGetValue(fileInfo.Extension, out var saver)) //?????????????????????
                {
                    await saver.SaveAsync(source, data.CompileParams, fileInfo, compiler.Encoding);
                }
                else //???????????????????????????????????????????????????
                {
                    CreateDirectory(dir);
                    await SaveSourceToHardDisk(source, fileInfo, compiler.Encoding);
                }

                var compileResult = await compiler.CompileAsync(fileInfo, data.CompileParams, dir, CancelSource.Token);
                await SaveCompileResultToHardDisk(compileResult, dir);
                if (!compileResult.IsSuccess)
                {
                    await SendReplyMessageAsync(request, CreateSimpleMessage(compileResult.Message));
                    return;
                }

                var executor = _executors[compileResult.ExecutableFile.Extension];
                var token = new CancellationTokenSource(MaxExecuteTime);
                var executeResult = await executor.ExecuteAsync(compileResult.ExecutableFile,
                    data.Input,
                    dir,
                    token.Token);
                await SaveExecuteResultToHardDisk(executeResult, dir);
                await SendReplyMessageAsync(request, CreateSimpleMessage(executeResult.GetReplyString()));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await SendReplyMessageAsync(request, _unknownError);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            Console.WriteLine("stop compile service...");
        }

        /// <summary>
        /// ???????????????????????????
        /// </summary>
        /// <param name="request">??????</param>
        /// <param name="msg">?????????????????????</param>
        public static bool IsCompileCmd(ChatMessageEventArgs request, out string msg)
        {
            var segment = request.GetMessageSegments();
            msg = null;
            if (segment.Length < 1)
            {
                return false;
            }
            var head = segment[0];
            if (head.Type != OneBotMessage.Text)
            {
                return false;
            }
            var headMsg = ChatMessageEventArgs.GetSimpleString(head);
            var cmdLen = CompileCmd.Length;
            var isCmd = headMsg != null &&
                        headMsg.StartsWith(CompileCmd) &&
                        headMsg.Length > cmdLen + 1 && //??????????????????????????????????????????
                        headMsg[cmdLen] == ' ';
            if (isCmd)
            {
                msg = request.GetStringMessage();
            }
            return isCmd;
        }

        /// <summary>
        /// ??????????????????
        /// </summary>
        /// <param name="msg">?????????</param>
        /// <param name="result">????????????</param>
        /// <returns>??????????????????</returns>
        public static bool ParseCompileCmd(string msg, out CompileData result) //TODO:?????????????????????????????????
        {
            result = null;
            var cmdLen = CompileCmd.Length;
            var wrap = msg.IndexOf('\n', cmdLen); //??????
            if (wrap <= 0) //????????????????????????????????????
            {
                var split = msg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (split.Length < 2)
                {
                    return false;
                }
                if (split[1] == "help")
                {
                    result = new CompileData
                    {
                        IsHelp = true,
                        CompileParams = split.Skip(2).ToList()
                    };
                    return true;
                }
                return false;
            }
            var isCRLF = msg[wrap - 1] == '\r'; //???????????????Win???CRLF??????
            if (wrap >= msg.Length - 1) //???????????????????????????????????????
            {
                return false;
            }
            var cmdParamLen = isCRLF ? wrap - 1 : wrap;
            if (cmdParamLen <= 0) //????????????????????????
            {
                return false;
            }
            var cmdStr = msg[cmdLen..cmdParamLen];
            var cmdParam = cmdStr.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (cmdParam.Length < 1) //?????????????????????
            {
                return false;
            }
            var compiler = cmdParam[0];
            var compileParam = cmdParam.Skip(1).ToList(); //???????????????
            var sourceStart = wrap + 1; //????????????
            var pattern = isCRLF ? $"\r\n{InputCmd} *\r\n" : $"\n{InputCmd} *\n";
            var match = Regex.Matches(msg, pattern);
            var sourceEnd = msg.Length;
            var input = string.Empty;
            if (match.Count > 0)
            {
                sourceEnd = match[0].Index;
                input = msg[(sourceEnd + match[0].Length)..];
            }
            result = new CompileData
            {
                Compiler = compiler,
                CompileParams = compileParam,
                SourceStartPos = sourceStart,
                SourceEndPos = sourceEnd,
                IsCRLF = isCRLF,
                Input = input,
                IsHelp = false
            };
            return true;
        }

        public static async Task<JsonDocument> SendReplyMessageAsync(
            ChatMessageEventArgs args,
            IReadOnlyList<OneBotMessageSegment> msg)
        {
            switch (args)
            {
                case PrivateMessageEventArgs:
                    return await OneBotHttpApi.SendPrivateMsgAsync(args.GetUserId(), msg);
                case GroupMessageEventArgs group:
                {
                    var msgs = new List<OneBotMessageSegment>();
                    msgs.Add(new OneBotMessageSegmentReply
                    {
                        Id = args.GetMessageId().ToString()
                    });
                    msgs.AddRange(msg);
                    return await OneBotHttpApi.SendGroupMsgAsync(group.GetGroupId(), msgs);
                }
                default:
                    throw new NotSupportedException();
            }
        }

        public static Guid GetGuid() { return Guid.NewGuid(); }

        public static string GetCacheDirectory(string root, long qq, Guid taskId)
        {
            return Path.Combine(root, $"{qq}_{taskId}");
        }

        public static void CreateDirectory(string path) { Directory.CreateDirectory(path); }

        public static async ValueTask SaveSourceToHardDisk(string source, FileInfo sourcePath, Encoding encoding)
        {
            await using var fileStream = sourcePath.Open(FileMode.Create);
            await using var writer = new StreamWriter(fileStream, encoding);
            await writer.WriteAsync(source);
        }

        public static async ValueTask SaveCompileResultToHardDisk(CompileResult result, string root)
        {
            var file = new FileInfo(Path.Combine(root, "compile_result.json"));
            await using var fileStream = file.Open(FileMode.Create);
            await JsonSerializer.SerializeAsync(fileStream, result);
        }

        public static async ValueTask SaveExecuteResultToHardDisk(ExecuteResult result, string root)
        {
            var file = new FileInfo(Path.Combine(root, "execute_result.json"));
            await using var fileStream = file.Open(FileMode.Create);
            await JsonSerializer.SerializeAsync(fileStream, result);
        }

        public static OneBotMessageSegment[] CreateSimpleMessage(string msg)
        {
            return new[]
            {
                new OneBotMessageSegmentText
                {
                    Text = msg
                }
            };
        }
    }
}