include('import.lua')
include('util.lua')

json = require('JSON')

require('command')
require('hook')
require('timer')

include('extensions/math.lua')
include('extensions/string.lua')
include('extensions/table.lua')
include('extensions/util.lua')

local function loadlua(code) return load(code)() end

local function clua(cmd, args, argstr)
    local success, err = pcall(loadlua, argstr)

    if not success then
        if LuaLoaderLog then LuaLoaderLog.Error(err) end

        error(err)
    end
end

command.Add('lua', clua)
command.Add('lua_run', clua)
command.Add('lua_reload', function() Loader.ReloadLua() end)
