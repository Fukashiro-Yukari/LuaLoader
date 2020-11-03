util = {}

function isnum(a)
    return type(a) == 'number'
end

isnumber = isnum

function isstr(a)
    return type(a) == 'string'
end

isstring = isstr

function isbool(v)
	return type(v) == 'boolean'
end

function istable(a)
	local f,e = pcall(pairs,a)
	return bool(f)
end

function isfunction(a)
	return type(a) == 'function'
end

function istype(o,t)
	return type(o) == t
end

function str(a)
	return tostring(a)
end

function num(a)
	return tonumber(a)
end

number = num

function bool(a)
	if a then
		return true
	end

	return false
end

function len(a)
	if istable(a) then
		return table.count(a)
	elseif isstr(a) then
		return string.len(a)
	elseif str(a) then
		return len(str(a))
	end
end

function pass(a)
    if not isnum(a) then return end

    local pass = ''

    for i = 0,a do
        pass = pass..' '
    end

    return pass
end

function tablefunc(a,b,c)
    b = b or {}
	c = c or function() end
	setmetatable(b,{__index = function(t,k) return c end})
    b[a]()
end

local clock = os.clock
function sleep(n)  -- seconds
	local t0 = clock()
	while clock() - t0 <= n do end
end