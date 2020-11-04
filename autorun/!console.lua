LuaLoaderLog = LuaLoaderLog or {}
LuaLoaderLog.__log = LuaLoaderLog.__log or {}
LuaLoaderLog.__gui = LuaLoaderLog.__gui or false
LuaLoaderLog.__text = LuaLoaderLog.__text or ''
LuaLoaderLog.__lasttexts = LuaLoaderLog.__lasttexts or {}
LuaLoaderLog.__lasttextc = LuaLoaderLog.__lasttextc or 0
LuaLoaderLog.__errortext = LuaLoaderLog.__errortext or ''
LuaLoaderLog.__errortime = LuaLoaderLog.__errortime or 0

local t = {}

function t:__newindex(k,v)
    rawset(self,k,v)

    if k > math.floor(Screen.height/25) then
        table.remove(self,1)
    end
end

setmetatable(LuaLoaderLog.__log,t)

function LuaLoaderLog.Log(s)
    s = tostring(s)

    if not isstring(s) then return end

    LuaLoaderLog.__log[#LuaLoaderLog.__log+1] = {
        text = '[Log] '..s,
        color = Color.white
    }
end

function LuaLoaderLog.LogColor(s,c)
    s = tostring(s)

    if not isstring(s) then return end

    LuaLoaderLog.__log[#LuaLoaderLog.__log+1] = {
        text = '[Log] '..s,
        color = c
    }
end

function LuaLoaderLog.Error(s)
    s = tostring(s)

    if not isstring(s) then return end

    LuaLoaderLog.__log[#LuaLoaderLog.__log+1] = {
        text = '[Error] '..s,
        color = Color.red
    }

    LuaLoaderLog.__errortext = s
    LuaLoaderLog.__errortime = Time.time+5
end

function LuaLoaderLog.ClearLog()
    LuaLoaderLog.__log = {}
end

local function runcommand()
    LuaLoaderLog.Log(LuaLoaderLog.__text)

    local arg1 = string.split(LuaLoaderLog.__text,' ')
    local name = table.remove(arg1,1)
    local arg2 = table.concat(arg1,' ')

    command.Run(name,arg1,arg2)

    LuaLoaderLog.__lasttexts[#LuaLoaderLog.__lasttexts+1] = LuaLoaderLog.__text
    LuaLoaderLog.__text = ''
end

hook.Add('OnGUI','!!!!!!LuaLoaderLog',function()
    local w = Screen.width/3
    local x,y,h = Screen.width/2-(w/2),20,100

    if LuaLoaderLog.__errortime > Time.time then
        GUI.Box(Rect(x,y,w,h),'Lua Error')

        GUI.color = Color.red
        
        GUI.Label(Rect(x+10,y+20,w-20,h-20),LuaLoaderLog.__errortext)

        GUI.color = Color.white
    end

    if not LuaLoaderLog.__gui then return end

    x,y,w,h = Screen.width/2,25,Screen.width/2,20

    GUI.Box(Rect(x-10,y-20,Screen.width/2,Screen.height/1.5),'Lua Console')

    for i = 1,math.min(#LuaLoaderLog.__log,math.floor(Screen.height/25)) do
        local t = LuaLoaderLog.__log[i]

        GUI.color = t.color

        GUI.Label(Rect(x,y,w,22),t.text)

        GUI.color = Color.white
        y = y+15
    end

    y = Screen.height/1.55
    y = y-20

    GUI.SetNextControlName('LuaLoaderLog.__text')

    LuaLoaderLog.__text = GUI.TextField(Rect(x-5,y,w-10,h),LuaLoaderLog.__text)

    y = y+20

    if GUI.Button(Rect(x-5,y,w-10,h),'Run Command') then
        runcommand()
    end
end)

hook.Add('OnUpdate','LuaLoaderLog',function()
    if InputManager.GetKeyDown(config.Instance.ConsoleKey) then
        LuaLoaderLog.__gui = not LuaLoaderLog.__gui
        LuaLoaderLog.__lasttextc = #LuaLoaderLog.__lasttexts
        Loader.ShowMouse = not Loader.ShowMouse

        if LuaLoaderLog.__gui then
            GUI.FocusControl('LuaLoaderLog.__text')
        else
            LuaLoaderLog.__text = ''
        end
    end

    if LuaLoaderLog.__gui then
        local e,r = pcall(InputManager.GetKeyDown,KeyCode.Return)

        if e and r then
            runcommand()
        end

        -- if Keyboard.current.upArrowKey.wasPressedThisFrame then
        --     -- table.print(LuaLoaderLog.__lasttexts)
        --     -- print(LuaLoaderLog.__lasttextc)

        --     -- if LuaLoaderLog.__lasttextc > 1 then
        --     --     LuaLoaderLog.__lasttextc = LuaLoaderLog.__lasttextc-1
        --     -- end

        --     -- LuaLoaderLog.__text = LuaLoaderLog.__lasttexts[LuaLoaderLog.__lasttextc]
        -- end

        -- if Keyboard.current.downArrowKey.wasPressedThisFrame then
        -- end
    end
end)

hook.Add('OnLuaError','LuaLoaderLog',function(err)
    LuaLoaderLog.Error(err)
end)

command.Add('clear',function()
    LuaLoaderLog.ClearLog()
end)

LuaLoaderLog.__print = LuaLoaderLog.__print or print

function print(...)
    LuaLoaderLog.__print(...)

    local r = {}

    for i = 1,select('#',...) do
        table.insert(r,tostring(select(i,...)))
    end

    if #r == 0 then
        table.insert(r,'nil')
    end

    LuaLoaderLog.Log(table.concat(r,'  '))
end