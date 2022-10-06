using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using NLua;

namespace LuaLoader.LuaClass
{
    public class LuaHarmony
    {
        private readonly string id;
        private HarmonyLib.Harmony harmony;
        private TypeBuilder typeBuilder;
        private Type type;
        private int methodId;

        public LuaHarmony(string id)
        {
            this.id = id;

            methodId = 0;
            harmony = new HarmonyLib.Harmony(id);

            var assName = new AssemblyName("LuaHarmony_" + id);
            var assBuilder = AssemblyBuilder.DefineDynamicAssembly(assName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assBuilder.DefineDynamicModule(assName.Name);

            typeBuilder = moduleBuilder.DefineType("LuaHarmony_" + id, TypeAttributes.Public);
        }

        public void CreateMethod(LuaTable table, LuaFunction func)
        {
            var methodBuilder = typeBuilder.DefineMethod("LuaHarmonyMethod_" + methodId, MethodAttributes.Public | MethodAttributes.Static, null, new Type[] { typeof(int) });

            methodId++;

            // https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.typebuilder?view=net-6.0
            // https://github.com/pardeike/Harmony/blob/56ac45a7e79bf0575298ab7cbbc20ded5418d796/Harmony/Internal/MethodPatcher.cs

            ILGenerator methIL = methodBuilder.GetILGenerator();

            //emitter.Emit(OpCodes.Ldnull)
            // To retrieve the private instance field, load the instance it
            // belongs to (argument zero). After loading the field, load the
            // argument one and then multiply. Return from the method with
            // the return value (the product of the two numbers) on the
            // execution stack.
            //methIL.Emit(OpCodes.Ldarg_0);
            //methIL.Emit(OpCodes.Ldfld, fbNumber);
            //methIL.Emit(OpCodes.Ldarg_1);
            //methIL.Emit(OpCodes.Mul);
            //methIL.Emit(OpCodes.Ret);


        }

        public Type CreateType()
        {
            type = typeBuilder.CreateType();

            return type;
        }

        public void Patch(LuaTable table)
        {
            //int priority = -1, string[] before = null, string[] after = null, bool? debug = null
            //this.priority = priority;
            //this.before = before;
            //this.after = after;
            //this.debug = debug;

            //PatchProcessor patchProcessor = harmony.CreateProcessor(original);
            ////patchProcessor.AddPrefix(prefix);
            ////patchProcessor.AddPostfix(postfix);
            ////patchProcessor.AddTranspiler(transpiler);
            ////patchProcessor.AddFinalizer(finalizer);
            ////patchProcessor.AddILManipulator(ilmanipulator);
            ////return patchProcessor.Patch();
        }
    }
}
