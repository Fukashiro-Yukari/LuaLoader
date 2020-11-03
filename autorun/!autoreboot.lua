AutoReboot = AutoReboot or {}
AutoReboot.LastTimes = AutoReboot.LastTimes or {}
AutoReboot.Times = AutoReboot.Times or {}

local loadlist = {
    'Mods/LuaLoader/autorun',
    'Mods/LuaLoader/libs'
}

hook.Add('OnUpdate','AutoReboot',function()
    for k,v in pairs(loadlist) do
        local t = Directory.GetFiles(v)

        for i = 0,t.Length-1 do
            AutoReboot.Times[t[i]] = File.GetLastWriteTime(t[i]).Ticks
        end

        for k,v in pairs(AutoReboot.Times) do
            local t = AutoReboot.LastTimes[k]

            if t and v ~= t then
                ReloadLua()
            end
        end

        AutoReboot.LastTimes = table.copy(AutoReboot.Times)
    end
end)