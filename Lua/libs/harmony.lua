LuaHarmony = {}

local Patchs = {
    regid = {},
    prefix = {},
    postfix = {},
    transpiler = {}
}

function LuaHarmony.GetTable()
    return Patchs
end

local function buildargs(argt)
    if !istable(argt) then return '' end

    local rs = ','

    if table.count(argt) < 1 then return '' end
    
    for k,v in pairs(argt) do
        rs = rs..v..' '..k..','
    end

    rs = string.sub(rs,1,#rs-1)

    return rs
end

local function buildargs2(argt)
    if !istable(argt) then return '' end

    local rs = ','

    if table.count(argt) < 1 then return '' end
    
    for k,v in pairs(argt) do
        rs = rs..k..','
    end

    rs = string.sub(rs,1,#rs-1)

    return rs
end

function LuaHarmony.PatchPrefix(n,mn,id,prefix,argtype,argt,dt)
    local rs = util.GetRandomStringNoNum(6)

    n = util.GetClassFullName(n) or n
    dt = dt or {}
    prefix = prefix or function() end

    local sargt = buildargs(argt)
    local sargt2 = buildargs2(argt)

    assert(n,'Need Class Name')
    assert(isstring(n),'Class Name must be a string')
    assert(isstring(mn),'Method Name must be a string')
    assert(isstring(id),'ID must be a string')
    assert(isstring(argtype),'__result type must be a string')

    Patchs.prefix[id] = prefix
    
    local dlllist = {
        '0Harmony.dll',
        '../../../../../LuaLoader.dll',
        '../../../../../../MelonLoader/MelonLoader.ModHandler.dll'
    }

    for k,v in pairs(dt) do
        dlllist[#dlllist+1] = v
    end

    local cs = Loader.RunCSCode(string.format([[
        using System;
        using LuaLoader;
        using Harmony;
        using MelonLoader;

        namespace %s
        {
            public class Patch
            {
                public void run()
                {
                    try
                    {
                        var harmony = HarmonyInstance.Create("%s");

                        harmony.Patch(typeof(%s).GetMethod("%s"), new HarmonyMethod(typeof(Patch).GetMethod(nameof(Prefix))));
                    }
                    catch (Exception e)
                    {
                        MelonLogger.LogError(e.ToString());
                    }
                }

                public static void Prefix(ref %s __result%s)
                {
                    var hr = LuaLoader.LuaLoader.lua.lua.GetFunction("LuaHarmony.Call").Call("%s", 1, __result%s);

                    if (hr.Length > 0 && hr[0] != null)
                    {
                        __result = (%s)hr[0];
                    }
                }
            }
        }
    ]],rs,id,n,mn,argtype,sargt,id,sargt2,argtype),dlllist)

    if !cs then return end

    local csi = cs:CreateInstance(rs..'.Patch')

    csi:GetType():GetMethod('run'):Invoke(csi,nil)
end

function LuaHarmony.PatchPostfix(n,mn,id,postfix,argtype,argt,dt)
    local rs = util.GetRandomStringNoNum(6)

    n = util.GetClassFullName(n) or n
    dt = dt or {}
    postfix = postfix or function() end

    local sargt = buildargs(argt)
    local sargt2 = buildargs2(argt)

    assert(n,'Need Class Name')
    assert(isstring(n),'Class Name must be a string')
    assert(isstring(mn),'Method Name must be a string')
    assert(isstring(id),'ID must be a string')
    assert(isstring(argtype),'__result type must be a string')

    Patchs.postfix[id] = postfix
    
    local dlllist = {
        '0Harmony.dll',
        '../../../../../LuaLoader.dll',
        '../../../../../../MelonLoader/MelonLoader.ModHandler.dll'
    }

    for k,v in pairs(dt) do
        dlllist[#dlllist+1] = v
    end

    local cs = Loader.RunCSCode(string.format([[
        using System;
        using LuaLoader;
        using Harmony;
        using MelonLoader;

        namespace %s
        {
            public class Patch
            {
                public void run()
                {
                    try
                    {
                        var harmony = HarmonyInstance.Create("%s");

                        harmony.Patch(typeof(%s).GetMethod("%s"), null, new HarmonyMethod(typeof(Patch).GetMethod(nameof(Postfix))));
                    }
                    catch (Exception e)
                    {
                        MelonLogger.LogError(e.ToString());
                    }
                }

                public static void Postfix(ref %s __result%s)
                {
                    var hr = LuaLoader.LuaLoader.lua.lua.GetFunction("LuaHarmony.Call").Call("%s", 2, __result%s);

                    if (hr.Length > 0 && hr[0] != null)
                    {
                        __result = (%s)hr[0];
                    }
                }
            }
        }
    ]],rs,id,n,mn,argtype,sargt,id,sargt2,argtype),dlllist)

    if !cs then return end

    local csi = cs:CreateInstance(rs..'.Patch')

    csi:GetType():GetMethod('run'):Invoke(csi,nil)
end

local typecall = {
    'prefix',
    'postfix',
    'transpiler'
}

function LuaHarmony.Call(id,type,...)
    if !Patchs.prefix[id] and !Patchs.postfix[id] and !Patchs.transpiler[id] then return end

    type = typecall[type]

    assert(type,'Incorrect type')

    return Patchs[type][id](...)
end