using System;
using System.Threading.Tasks;

namespace KSGFK
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            OneBotHttpApi.SetServerUri(new Uri("http://127.0.0.1:25565"));
            var compiler = new PenguinCompilerService(@"C:\Users\ksgfk\Desktop\test", 5000, 1000, 2);
            var listener = new OneBotHttpEventDispatcher("http://127.0.0.1:25567/", compiler, 1000, 2);
            compiler.AddCompiler("cpp", new CppMsvcCompiler());
            compiler.AddCompiler("py", new PythonCompiler());
            compiler.AddExecutor(new WindowsExeExecutor());
            compiler.AddExecutor(new PythonExecutor());
            compiler.AddSourceCodeSaver(new CppSourceCodeSaver());

            var listenerTask = listener.StartAsync();
            var compilerTask = compiler.StartAsync();

            Console.CancelKeyPress += (sender, arg) =>
            {
                arg.Cancel = true;
                listener.Stop();
                compiler.Stop();
            };

            try
            {
                await Task.WhenAll(listenerTask, compilerTask);
            }
            catch (TaskCanceledException)
            {
            }
        }
    }
}