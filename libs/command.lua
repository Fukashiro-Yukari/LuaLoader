command = {}

local commands = {}

function command.Add(n,f)
    if not isstring(n) or commands[n] then return end

    commands[n] = f
end

function command.Remove(n)
    if not isstring(n) or not commands[n] then return end

    commands[n] = nil
end

function command.Run(n,a,as)
    if not isstring(n) or not commands[n] then return end

    commands[n](a,as)
end

local function loadlua(s)
    return load(s)()
end

local function clua(a,as)
    local e,r = pcall(loadlua,as)

    if not e then
        if LuaLoaderLog then
            LuaLoaderLog.Error(r)
        else
            error(r)
        end
    end
end

command.Add('lua',clua)
command.Add('lua_run',clua)

command.Add('lua_reload',function(a,as)
    ReloadLua()
end)