local string = string
local print = print

module('command', package.seeall)

local commandlist = {}
local completelist = {}
local helplist = {}

--[[---------------------------------------------------------
   Name: command.GetTable( )
   Desc: Returns the table of console commands and auto complete
-----------------------------------------------------------]]
function GetTable() return commandlist, completelist end

--[[---------------------------------------------------------
   Name: command.Add( name, func, completefunc, help)
   Desc: Register a new console command
-----------------------------------------------------------]]
function Add(name, func, completefunc, help)
    local lowername = string.lower(name)
    commandlist[lowername] = func
    completelist[lowername] = completefunc
    helplist[lowername] = help
end

--[[---------------------------------------------------------
   Name: command.Remove( name )
   Desc: Removes a console command
-----------------------------------------------------------]]
function Remove(name)
    local lowername = string.lower(name)
    commandlist[lowername] = nil
    completelist[lowername] = nil
    helplist[lowername] = nil
end

--[[---------------------------------------------------------
   Name: command.Run( command, arguments, argstring )
   Desc: Called by the engine when an unknown console command is run
-----------------------------------------------------------]]
function Run(command, arguments, argstring)
    local lowercommand = string.lower(command)

    if commandlist[lowercommand] ~= nil then
        commandlist[lowercommand](command, arguments, argstring)
        return true
    end

    if LuaLoaderLog then
        LuaLoaderLog.Log('Unknown command: ' .. command .. '\n')
    end

    print('Unknown command: ' .. command .. '\n')

    return false
end

--[[---------------------------------------------------------
   Name: command.AutoComplete( command, arguments )
   Desc: Returns a table for the autocompletion
-----------------------------------------------------------]]
function AutoComplete(command, arguments)
    local lowercommand = string.lower(command)

    if completelist[lowercommand] ~= nil then
        return completelist[lowercommand](command, arguments)
    end
end
