--
-- Seed the rand!
--
math.randomseed(tostring(DateTime.Now.Ticks):reverse():sub(1, 6))

--
-- Alias string.Format to global Format
--
Format = string.format

function istype(obj, is) return type(obj) == is end

function isctype(obj, is) return ctype(obj) == is end

function isnumber(obj) return type(obj) == 'number' end

function isstring(obj) return type(obj) == 'string' end

function isbool(obj) return type(obj) == 'boolean' end

function istable(obj) return type(obj) == 'table' end

function isfunction(obj) return type(obj) == 'function' end

function isarray(obj) return
    type(obj) == 'userdata' and obj.GetEnumerator ~= nil end

function cpairs(t)
    assert(isarray(t),
           'bad argument #1 to \'cpairs\' (c# array expected, got ' .. type(t) ..
               ')')

    local i = 0
    local e = t:GetEnumerator()

    return function()
        if e:MoveNext() then
            local k = i
            local v = e.Current

            if ctype(v):StartWith('KeyValuePair') then
                return v.Key, v.Value
            end

            i = i + 1

            return k, v
        end
    end, t
end

local old = pairs

function pairs(t, ...)
    if isarray(t) then return cpairs(t) end

    return old(t, ...)
end

--[[---------------------------------------------------------
	Prints a table to the console
-----------------------------------------------------------]]
function PrintTable(t, indent, done)
    local print = print

    done = done or {}
    indent = indent or 0
    local keys = table.GetKeys(t)

    table.sort(keys, function(a, b)
        if (isnumber(a) and isnumber(b)) then return a < b end
        return tostring(a) < tostring(b)
    end)

    done[t] = true

    for i = 1, #keys do
        local key = keys[i]
        local value = t[key]
        print(string.rep("\t", indent))

        if (istable(value) and not done[value]) then
            done[value] = true
            print(key, ":\n")
            PrintTable(value, indent + 2, done)
            done[value] = nil
        else

            print(key, "\t=\t", value, "\n")

        end

    end

end

--[[---------------------------------------------------------
	Simple lerp
-----------------------------------------------------------]]
function lerp(delta, from, to)

    if (delta > 1) then return to end
    if (delta < 0) then return from end

    return from + (to - from) * delta

end

--[[---------------------------------------------------------
	Convert Var to Bool
-----------------------------------------------------------]]
function tobool(val)
    if val == nil or val == false or val == 0 or val == "0" or val == "false" then
        return false
    end

    return true
end
