# PenguinCompiler
这是一个使用[OneBot](https://github.com/howmanybots/onebot)标准QQ聊天机器人，可以以聊天的形式编译运行各种语言的代码并回复结果，使用.Net 5和C# 9
## Features
* 没有第三方库依赖（这是能算特性吗
* 内置可以编译运行的语言
  * C++ MSVC
  * Python
* 只实现了HTTP与OneBot通信
* 只实现了OneBot的消息事件
* 可用的OneBot API只有`send_private_msg`和`send_group_msg`
* 特化了C++ MSVC的参数，当参数包含`oi`的时候使用内置模板简化代码
* 大部分API都是异步的（练习async/await留下的233
## Use
* 启动参数都硬编码在PenguinCompiler/Program.cs里面
  * `OneBotHttpApi.SetServerUri`设置OneBot的HTTP uri
  * `PenguinCompilerService`类的构造方法依次填入：源码缓存路径、程序最长运行时间（单位ms）、任务队列最大长度、最多并行程序执行数量
  * `OneBotHttpEventDispatcher`类的构造方法依次填入：接收OneBot的HTTP消息的uri、`PenguinCompilerService`实例
  * 可以继承`Compiler`、`Executor`、`SourceCodeSaver`并将实例存入`PenguinCompilerService`来扩展可用语言
* 由于MSVC的特性，使用`cl.exe`前要用VS的工具设置编译环境，打开开始菜单-Visual Studio-Tools Command Prompt差不多这种字样的批处理，等它配好了之后，直接在批处理里启动程序就能找到`cl.exe`了
* 没写`GCC`编译C++的实现，不会真的有人在Windows下用`MingW`吧（是我自己啊，那没事了
## Structure
* PenguinCompiler 源码根目录
  * Compile 编译相关目录
    * Compiler.cs 编译器基类
    * Executor.cs 执行源码文件基类
    * SourceCodeSaver.cs 储存源码时行为的基类
  * OneBot OneBot相关目录
    * Event OneBot事件目录
    * Message OneBot消息（指的是[这个](https://github.com/howmanybots/onebot/blob/master/v11/specs/message/README.md)消息）
    * Net OneBot网络连接目录
  * OneBotHttpEventDispatcher.cs 分发来自OneBot的事件
  * PenguinCompilerService.cs 编译服务
  * Program.cs 程序入口
  * Service.cs 服务的基类
* 需要注意的是，并没有像一般做法一样，将来自OneBot的事件从Json转化成实例类，而是直接解析成`System.Text.Json.JsonDocument`并用`KSGFK.ChatMessageEventArgs`简单包装了一下（目前只接受聊天事件，所以基类就这个233），包装类里也没多少方法，所以要获取某些 类里没有实现读取 的数据，要手动根据Json结构去获取（常用的Json PropertyName名字都放在`KSGFK.OneBotMessage`和`KSGFK.OneBotEvent`里面了）
* OneBot的API调用都放在`KSGFK.OneBotHttpApi`里面
## Problem
* 没有鉴权，接收任何来自可能是OneBot的HTTP请求
* 没有经过检查就执行代码，很容易被恶意代码攻击
* OneBot的事件支持快速操作，实际上是一个HTTP response，但是代码中直接把那些包装事件的实例（就是`KSGFK.ChatMessageEventArgs`）传递给其他服务，response可能会失效，导致调用`KSGFK.ChatMessageEventArgs.Reply`会抛异常
* 一大堆hard code（233
* 代码挺乱的，没整理过（又不是不能用
## License
MIT