local pairs = pairs
local isfunction = isfunction
local isstring = isstring
local isnumber = isnumber
local isbool = isbool
local IsValid = IsValid
local type = type
local error = error

module('hook')

local hooks = {}

--[[---------------------------------------------------------
    Name: GetTable
    Desc: Returns a table of all hooks.
-----------------------------------------------------------]]
function GetTable() return hooks end

--[[---------------------------------------------------------
    Name: Add
    Args: string hookName, any identifier, function func
    Desc: Add a hook to listen to the specified event.
-----------------------------------------------------------]]
function Add(event_name, name, func)
    if not isstring(event_name) then
        error("bad argument #1 to 'Add' (string expected, got " ..
                  type(event_name) .. ")")
        return
    end
    if not isfunction(func) then
        error(
            "bad argument #3 to 'Add' (function expected, got " .. type(func) ..
                ")")
        return
    end

    if hooks[event_name] == nil then hooks[event_name] = {} end

    hooks[event_name][name] = func
end

--[[---------------------------------------------------------
    Name: Remove
    Args: string hookName, identifier
    Desc: Removes the hook with the given indentifier.
-----------------------------------------------------------]]
function Remove(event_name, name)
    if not isstring(event_name) then
        error("bad argument #1 to 'Remove' (string expected, got " ..
                  type(event_name) .. ")")
        return
    end
    if not hooks[event_name] then return end

    hooks[event_name][name] = nil
end

--[[---------------------------------------------------------
    Name: Call
    Args: string hookName, vararg args
    Desc: Calls hooks associated with the hook name.
-----------------------------------------------------------]]
function Call(name, ...)
    local tbl = hooks[name]

    if not tbl then return end

    local a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p, q, r, s, t, u, v, w,
          x, y, z

    for k, v in pairs(tbl) do
        local a2, b2, c2, d2, e2, f2, g2, h2, i2, j2, k2, l2, m2, n2, o2, p2,
              q2, r2, s2, t2, u2, v2, w2, x2, y2, z2 = v(...)

        if a2 ~= nil then
            a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p, q, r, s, t, u, v, w, x, y, z =
                a2, b2, c2, d2, e2, f2, g2, h2, i2, j2, k2, l2, m2, n2, o2, p2,
                q2, r2, s2, t2, u2, v2, w2, x2, y2, z2
        end
    end

    if a ~= nil then
        return a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p, q, r, s, t, u, v,
               w, x, y, z
    end
end
