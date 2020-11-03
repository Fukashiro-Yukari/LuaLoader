function table.count(t)
	local i = 0
	for k in pairs(t) do
		i = i+1
	end

	return i
end

function table.random(t)
	local rk = math.random(1,table.count(t))
	local i = 1
	
	for k,v in pairs(t) do
		if i == rk then return v, k end
		i = i+1
	end
end

function table.empty(t)
	for k,v in pairs(t) do
		t[k] = nil
	end
end

function table.isempty(t)
	return next(t) == nil
end

function table.getkeys(a)
	local key = {}

	for k,v in pairs(a) do
		key[#key+1] = k
	end

	return key
end

function table.print(...)
    local t = {...}

	for k,v in pairs(t) do
		if istable(v) then
			local keys = table.getkeys(v)
			
			for i=1,#keys do
				local key = keys[i]
				local val = v[key]

				if type(val) == 'table' then
					print('['..tostring(key)..']'..':\n')
					table.print(val)
				else
					print('['..tostring(key)..']'..' = '..tostring(val))
				end
			end
		else
			print(v)
		end
	end
end

function table.getmax(a)
    local max = 0
    
	for k,v in pairs(a) do
		if v > max then
			max = v
		end
	end
	
	return max
end

function table.getmin(a)
	local min = math.huge
	for k,v in pairs(a) do
		if v < min then
			min = v
		end
	end

	return min
end

function table.copy(t)
	local tbl = {}
	for k,v in pairs(t) do
		tbl[k] = v
	end
	
	return tbl
end

function table.hasvalue(t,val)
	for k,v in pairs(t) do
		if v == val then return true end
	end

	return false
end