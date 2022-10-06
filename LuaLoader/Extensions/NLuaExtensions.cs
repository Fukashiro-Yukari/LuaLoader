using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLua;

namespace LuaLoader
{
    public static class NLuaExtensions
    {
        public static object[] TryCall(this LuaFunction function, params object[] args)
        {
            try
            {
                return function.Call(args);
            }
            catch (Exception e)
            {
                LuaLoader.LuaError(e);
            }

            return null;
        }
    }
}
