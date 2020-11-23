util = {}

-- math.randomseed(os.time())
math.randomseed(tostring(DateTime.Now.Ticks):reverse():sub(1,6))

function istype(o,t)
	return type(o) == t
end

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
	return type(a) == 'table'
end

function isfunction(a)
	return type(a) == 'function'
end

local function getLength(o)
	return o.Length
end

function isarray(t)
	if istype(t,'userdata') then
		local b,r = pcall(getLength,t)

		return b
	end

	return false
end

function str(a)
	return tostring(a)
end

function num(a)
	return tonumber(a)
end

number = num

function bool(a)
	if a == nil or a == false or a == 0 or a == '0' or a == 'false' then return false end

	return false
end

function len(a)
	if istable(a) or isarray(a) then
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

function switch(a,b,c)
    b = b or {}
	c = c or function() end
	setmetatable(b,{__index = function(t,k) return c end})
    b[a]()
end

local clock = os.clock
function sleep(n) -- seconds
	local t0 = clock()

	while clock()-t0 <= n do end
end

function typeof(o)
    if !o or !istype(o,'userdata') then return end

    local s = tostring(o)

    s = string.split(s,':')
    s = s[1]
    s = string.gsub(s,'ProxyType','')
    s = string.gsub(s,'%(','')
    s = string.gsub(s,'%)','')

    return Loader.GetType(s)
end

function xluatypeof(o) -- The result is the same as typeof, it doesn't seem to make any sense
	return unpack(Loader.RunXLuaCode([[
		return typeof(nlua.obj)
	]],{
		obj = o
	}))
end

function cpairs(t)
	assert(isarray(t),'bad argument #1 to \'cpairs\' (c# array expected, got '..type(t)..')')

	local i = 0

	return function()
		if i < t.Length then
			local k = i
			local v = t[i]

			i = i+1

			return k,v
		end
	end,t
end

local old = pairs

function pairs(t,...)
	if isarray(t) then
		return cpairs(t)
	end

	return old(t,...)
end

local old = table.unpack

function unpack(t,...)
	if istype(t,'userdata') then
		local b,r = pcall(getLength,t)

		if b then
			t = util.ArrayToTable(t)
		end
	end

	return old(t,...)
end

table.unpack = unpack

function include(f)
	if !string.find(f,'Mods/LuaLoader/') then
		f = 'Mods/LuaLoader/'..f
	end

	if AutoReboot then
		AutoReboot.AddPath(string.getpathfromfilename(f))
	end

	local ex = string.getextensionfromfilename(f)

	if !ex then
		f = f..'.lua'
	end

	return unpack(LoadLuaFile(f))
end

function lerp( delta, from, to )

	if ( delta > 1 ) then return to end
	if ( delta < 0 ) then return from end

	return from + ( to - from ) * delta

end

function util.ArrayToTable(o,b)
	assert(isarray(o),'bad argument #1 to \'util.ArrayToTable\' (c# array expected, got '..type(t)..')')
	
	local t = {}

	for k,v in cpairs(o) do
		t[(b and k or k+1)] = v
	end

	return t
end

function util.TableToArray(t)
	return Loader.CreateArray(t)
end

function util.GetRandomString(len)
    local str = '1234567890abcdefhijklmnopqrstuvwxyz'
	local ret = ''
	
    for i = 1,len do
        local rchr = math.random(1,string.len(str))
        ret = ret..string.sub(str,rchr,rchr)
	end
	
    return ret
end

function util.GetRandomStringNoNum(len)
    local str = 'abcdefhijklmnopqrstuvwxyz'
	local ret = ''
	
    for i = 1,len do
        local rchr = math.random(1,string.len(str))
        ret = ret..string.sub(str,rchr,rchr)
	end
	
    return ret
end

local idTemp = 0
function util.GetId()
	idTemp = idTemp+1
	
    return idTemp
end

function util.GetClassFullName(o)
    if !o or !istype(o,'userdata') then return end

    local s = tostring(o)

    s = string.split(s,':')
    s = s[1]
    s = string.gsub(s,'ProxyType','')
    s = string.gsub(s,'%(','')
    s = string.gsub(s,'%)','')

    return s
end

-- R.I.P module function and setfenv function

-- function module()
-- end