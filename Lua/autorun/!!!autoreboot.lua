AutoReboot = AutoReboot or {}
AutoReboot.LastTimes = AutoReboot.LastTimes or {}
AutoReboot.Times = AutoReboot.Times or {}
AutoReboot.LoadList = AutoReboot.LoadList or {}
AutoReboot.AutoCreate = AutoReboot.AutoCreate or {}
AutoReboot.UseOldVersion = false

local function checkingPath(s)
    if not string.StartWith(s, 'Lua/') then s = 'Lua/' .. s end

    if s[#s] == '/' or s[#s] == '\\' then s = string.sub(s, 1, #s - 1) end

    return s
end

function AutoReboot.AddPath(path)
    path = checkingPath(path)

    if not Directory.Exists(path) or AutoReboot.LoadList[path] then return end

    AutoReboot.LoadList[path] = path

    local function reloadlua(sender, args)
        local auto = AutoReboot.AutoCreate[path]
        local old = {}

        if auto then
            for k, v in pairs(table.Copy(auto.inittable)) do
                old[k] = _G[k]
                _G[k] = v
            end
        end

        include(Path.Combine(path, Path.GetFileName(args.Name)))

        if auto then
            local get = {}

            for k, v in pairs(auto.inittable) do
                get[k] = _G[k]
                _G[k] = old[k]
            end

            auto.callback(Path.GetFileName(args.Name), get)
        end
    end

    Loader.CreateFileSystemWatcher(path,
                                   {Changed = reloadlua, Renamed = reloadlua})
end

function AutoReboot.AddAutoCreateTablePath(path, table, callback)
    path = checkingPath(path)

    if not Directory.Exists(path) then return end

    AutoReboot.AutoCreate[path] = {
        inittable = table,
        callback = callback or function() end
    }
end

if AutoReboot.UseOldVersion then
    function AutoReboot.AddPath(path)
        path = checkingPath(path)

        if not Directory.Exists(path) or AutoReboot.LoadList[path] then
            return
        end

        AutoReboot.LoadList[path] = path
    end

    hook.Add('OnUpdate', 'AutoReboot', function()
        for k, v in pairs(AutoReboot.LoadList) do
            for k, v in pairs(Directory.GetFiles(v)) do
                AutoReboot.Times[v] = File.GetLastWriteTime(v).Ticks
            end

            for k, v in pairs(AutoReboot.Times) do
                local t = AutoReboot.LastTimes[k]

                if t and v ~= t then
                    AutoReboot.LastTimes = table.Copy(AutoReboot.Times)

                    local path = string.GetPathFromFilename(k)
                    local fpath = k

                    if string.sub(path, #path) == '/' or string.sub(path, #path) ==
                        '\\' then
                        path = string.sub(path, 1, #path - 1)
                    end

                    local auto = AutoReboot.AutoCreate[path]
                    local old = {}

                    if auto then
                        for k, v in pairs(table.Copy(auto.inittable)) do
                            old[k] = _G[k]
                            _G[k] = v
                        end
                    end

                    if not File.Exists(k) then
                        AutoReboot.Times[k] = nil
                    end

                    include(k)

                    if auto then
                        local get = {}

                        for k, v in pairs(auto.inittable) do
                            get[k] = _G[k]
                            _G[k] = old[k]
                        end

                        auto.callback(fpath, get)
                    end
                end
            end

            AutoReboot.LastTimes = table.Copy(AutoReboot.Times)
        end
    end)
end
