AutoReboot = AutoReboot or {}
AutoReboot.LastTimes = AutoReboot.LastTimes or {}
AutoReboot.Times = AutoReboot.Times or {}
AutoReboot.LoadList = AutoReboot.LoadList or {}
AutoReboot.AutoCreate = AutoReboot.AutoCreate or {}

local function istr(s)
    if !string.find(s,'Mods/LuaLoader/') then
		s = 'Mods/LuaLoader/'..s
    end

    if string.sub(s,#s) == '/' or string.sub(s,#s) == '\\' then
        s = string.sub(s,1,#s-1)
    end

    return s
end

function AutoReboot.AddPath(s)
    s = istr(s)
    
    if !Directory.Exists(s) then return end

    AutoReboot.LoadList[s] = s
end

function AutoReboot.AddAutoCreateTablePath(p,t,f)
    p = istr(p)

    if !Directory.Exists(p) then return end

    AutoReboot.AutoCreate[p] = {
        inittable = t,
        callback = f or function() end
    }
end

AutoReboot.AddPath('autorun')
AutoReboot.AddPath('libs')
AutoReboot.AddPath('modules')

hook.Add('OnUpdate','AutoReboot',function()
    for k,v in pairs(AutoReboot.LoadList) do
        for k,v in pairs(Directory.GetFiles(v)) do
            AutoReboot.Times[v] = File.GetLastWriteTime(v).Ticks
        end

        for k,v in pairs(AutoReboot.Times) do
            local t = AutoReboot.LastTimes[k]

            if t and v ~= t then
                AutoReboot.LastTimes = table.copy(AutoReboot.Times)

                local path = string.getpathfromfilename(k)
                local fpath = k

                if string.sub(path,#path) == '/' or string.sub(path,#path) == '\\' then
                    path = string.sub(path,1,#path-1)
                end

                local auto = AutoReboot.AutoCreate[path]
                local old = {}

                if auto then
                    for k,v in pairs(auto.inittable) do
                        old[k] = _G[k]
                        _G[k] = v
                    end
                end

                if !File.Exists(k) then
                    AutoReboot.Times[k] = nil
                end

                include(k)

                if auto then
                    local get = {}

                    for k,v in pairs(auto.inittable) do
                        get[k] = _G[k]
                        _G[k] = old[k]
                    end

                    auto.callback(fpath,get)
                end
            end
        end

        AutoReboot.LastTimes = table.copy(AutoReboot.Times)
    end
end)