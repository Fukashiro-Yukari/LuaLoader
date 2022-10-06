using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaLoader.LuaClass
{
    class ents
    {
        public static object FindObjectOfType(Type type)
        {
#if CPP
            var e = UnityEngine.Object.FindObjectOfType(Il2CppType.From(type));

            return e.Il2CppCast(type);
#else
            return UnityEngine.Object.FindObjectOfType(type);
#endif
        }

        public static object FindObjectsOfType(Type type)
        {
#if CPP
            var a = UnityEngine.Object.FindObjectsOfType(Il2CppType.From(type));
            var ret = new object[a.Length];

            for (var i = 0;i < ret.Length; i++)
            {
                ret[i] = a[i].Il2CppCast(type);
            }

            return ret;
#else
            return UnityEngine.Object.FindObjectsOfType(type);
#endif
        }
    }
}
