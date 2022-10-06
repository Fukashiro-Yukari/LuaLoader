util = util or {}

function util.ArrayToTable(array, luastyle)
    assert(isarray(array),
           'bad argument #1 to \'util.ArrayToTable\' (c# array expected, got ' ..
               type(array) .. ')')

    local t = {}

    for k, v in cpairs(array) do t[(luastyle and k or k + 1)] = v end

    return t
end

function util.TableToArray(tbl) return Loader.CreateArray(tbl) end

function util.GetRandomString(len)
    local str = '1234567890abcdefhijklmnopqrstuvwxyz'
    local ret = ''

    for i = 1, len do
        local rchr = math.random(1, string.len(str))
        ret = ret .. string.sub(str, rchr, rchr)
    end

    return ret
end

function util.GetRandomStringNoNum(len)
    local str = 'abcdefhijklmnopqrstuvwxyz'
    local ret = ''

    for i = 1, len do
        local rchr = math.random(1, string.len(str))
        ret = ret .. string.sub(str, rchr, rchr)
    end

    return ret
end

local idTemp = 0
function util.GetId()
    idTemp = idTemp + 1

    return idTemp
end

function util.GetAllAssembliesPath()
    local rt = {}

    for k, v in pairs(AppDomain.CurrentDomain:GetAssemblies()) do
        if v.Location ~= '' and v.Location ~= nil then
            rt[#rt + 1] = v.Location
        end
    end

    return rt
end

--[[---------------------------------------------------------
	Returns year, month, day and hour, minute, second in a formatted string.
-----------------------------------------------------------]]
function util.DateStamp()

    local t = os.date('*t')
    return t.year .. "-" .. t.month .. "-" .. t.day .. " " ..
               Format("%02i-%02i-%02i", t.hour, t.min, t.sec)

end

--
-- Formats a float by stripping off extra 0's and .'s
--
--	0.00	->		0
--	0.10	->		0.1
--	1.00	->		1
--	1.49	->		1.49
--	5.90	->		5.9
--
function util.NiceFloat(f)

    local str = string.format("%f", f)

    str = str:TrimRight("0")
    str = str:TrimRight(".")

    return str

end
