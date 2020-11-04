using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace LuaLoader.LuaTask
{
    /// <summary>
    /// lua虚拟机对象
    /// 自带luat task框架接口
    /// </summary>
    public class LuaEnv : IDisposable
    {
        //lua虚拟机
        public NLua.Lua lua;
        //报错的回调
        public event EventHandler<string> ErrorEvent;

        private bool stop = false;//是否停止运行
        private ConcurrentDictionary<int, CancellationTokenSource> timerPool =
                new ConcurrentDictionary<int, CancellationTokenSource>();//timer取消标志池子
        private static ConcurrentBag<LuaTaskData> toRun = new ConcurrentBag<LuaTaskData>();//待运行的池子
        private readonly object taskLock = new object();
        private int asyncId = 0;//回调函数编号

        /// <summary>
        /// 发出报错信息
        /// </summary>
        /// <param name="msg">报错信息</param>
        private void error(string msg)
        {
            ErrorEvent?.Invoke(lua, msg);
        }

        /// <summary>
        /// 添加一个回调触发
        /// </summary>
        /// <param name="type">触发类型</param>
        /// <param name="data">传递数据</param>
        public void addTigger(string type, object data) => addTask(-1, type, data);

        /// <summary>
        /// 添加一个
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <param name="data"></param>
        private void addTask(int id, string type, object data)
        {
            if (!stop)
            {
                toRun.Add(new LuaTaskData { id = id, type = type, data = data });
                if (Monitor.TryEnter(taskLock))
                {
                    Monitor.Exit(taskLock);
                    runTask();
                }
            }
        }

        /// <summary>
        /// 跑一遍任务池子里的任务
        /// </summary>
        private void runTask()
        {
            lock (taskLock)
                while (toRun.Count > 0)
                {
                    try
                    {
                        LuaTaskData task;
                        toRun.TryTake(out task);//取出来一个任务
                        lua.GetFunction("sys.tiggerCB").Call(task.id, task.type, task.data);//跑
                    }
                    catch (Exception e)
                    {
                        MelonLoader.MelonLogger.LogError(e.Message);
                        //ErrorEvent?.Invoke(lua, e.Message);
                    }
                    if (stop)//任务停了
                        return;
                }
        }

        /// <summary>
        /// 新建定时器
        /// </summary>
        /// <param name="id">编号</param>
        /// <param name="time">时间(ms)</param>
        public int StartTimer(int id, int time)
        {
            CancellationTokenSource timerToken = new CancellationTokenSource();
            if (timerPool.ContainsKey(id))//如果已经有一个一样的定时器了？
                StopTimer(id);
            timerPool.TryAdd(id, timerToken);//加到池子里
            var timer = new System.Timers.Timer(time);
            timer.Elapsed += (sender, e) =>
            {
                if (timerToken == null || timerToken.IsCancellationRequested)
                    return;
                if (stop)
                    return;
                timerPool.TryRemove(id, out _);
                addTask(id, "timer", null);
                ((System.Timers.Timer)sender).Dispose();
            };
            timer.AutoReset = false;
            timer.Start();
            return 1;
        }

        /// <summary>
        /// 停止定时器
        /// </summary>
        /// <param name="id">编号</param>
        public void StopTimer(int id)
        {
            if (timerPool.ContainsKey(id))
            {
                try
                {
                    CancellationTokenSource tc;
                    timerPool.TryRemove(id, out tc);
                    tc.Cancel();
                }
                catch { }
            }
        }

        /// <summary>
        /// 跑代码
        /// </summary>
        /// <param name="s">代码</param>
        /// <returns>返回的结果</returns>
        public object[] DoString(string s)
        {
            try
            {
                return lua.DoString(s);
            }
            catch (Exception e)
            {
                ErrorEvent?.Invoke(lua, e.Message);
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// 跑文件
        /// </summary>
        /// <param name="s">文件路径</param>
        /// <returns>返回的结果</returns>
        public object[] DoFile(string s)
        {
            try
            {
                return lua.DoFile(s);
            }
            catch (Exception e)
            {
                ErrorEvent?.Invoke(lua, e.Message);
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// 异步执行C#函数，完成后回调
        /// </summary>
        /// <param name="ass">程序集</param>
        /// <param name="type">类.方法</param>
        /// <param name="data">传入数据</param>
        /// <returns>回调的编号</returns>
        public int AsyncRun(string ass,string type,params object[] data)
        {
            int id = asyncId++;
            if (asyncId > int.MaxValue - 100)
                asyncId = 0;
            try
            {
                System.Reflection.Assembly asm = System.Reflection.Assembly.Load(ass);
                string className = type.Substring(0, type.LastIndexOf("."));
                Type t = asm.GetType(className);
                (new Thread(() =>
                {
                    try
                    {
                        string method = type.Substring(type.LastIndexOf(".") + 1,
                            type.Length - type.LastIndexOf(".") - 1);
                        List<Type> ft = new List<Type>();
                        for (int i = 0; i < data.Length; i++)
                            ft.Add(data[i].GetType());
                        object r = t.GetMethod(method, ft.ToArray()).Invoke(null, data);
                        addTask(id, "async", r);
                    }
                    catch (Exception e)
                    {
                        ErrorEvent?.Invoke(this, e.Message);
                        addTask(id, "asyncFail", e.Message);
                    }
                })).Start();
            }
            catch (Exception e)
            {
                
                ErrorEvent?.Invoke(this, e.Message);
                return -1;
            }

            return id;
        }

        /// <summary>
        /// 初始化，加载全部接口
        /// </summary>
        /// <param name="input">输入值，一般为匿名类</param>
        public LuaEnv(object input = null)
        {
            lua = new NLua.Lua();
            lua.State.Encoding = Encoding.UTF8;
            lua.LoadCLRPackage();
            if (input != null)
                lua["input"] = input;//传递输入值
            //lua.DoString(sysCode);
            lua["__luataskthis"] = this;//自己传给自己
        }

        /// <summary>
        /// 销毁
        /// </summary>
        public void Dispose()
        {
            stop = true;//停止待运行任务
            lua.Dispose();//释放lua虚拟机
            foreach (var v in timerPool)
            {
                v.Value.Cancel();//取消所有timer
            }
            timerPool.Clear();//清空timer信息池子
            while (toRun.TryTake(out _)) ;//清空待运行池子
        }


        //private static string sysCode = @"";
    }

    class LuaTaskData
    {
        public int id { get; set; }
        public string type { get; set; }
        public object data { get; set; }
    }
}
