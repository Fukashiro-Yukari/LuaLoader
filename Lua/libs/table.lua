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
		if i == rk then return v,k end
		i = i+1
	end
end

function table.empty(t)
	for k,v in pairs(t) do
		t[k] = nil
	end
end

function table.isempty(t)
	if isarray(t) then
		t = util.ArrayToTable(t)
	end

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
		if istable(v) or isarray(v) then
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

function table.copy(t,lookup_table)
	if t == nil then return nil end

	local copy = {}
	setmetatable(copy,debug.getmetatable(t))

	for i,v in pairs(t) do
		if !istable(v) then
			copy[i] = v
		else
			lookup_table = lookup_table or {}
			lookup_table[t] = copy

			if lookup_table[v] then
				copy[i] = lookup_table[v] -- we already copied this table. reuse the copy.
			else
				copy[i] = table.copy(v,lookup_table) -- not yet copied. copy it.
			end
		end
	end

	return copy
end

function table.merge(dest,source)
	for k, v in pairs(source) do
		if istable(v) and istable(dest[k]) then
			-- don't overwrite one table with another
			-- instead merge them recurisvely
			table.merge(dest[k],v)
		else
			dest[k] = v
		end
	end

	return dest
end

function table.copyfromto( from, to )
	table.empty(to)
	table.merge(to,from)
end

function table.hasvalue(t,val)
	for k,v in pairs(t) do
		if v == val then return true end
	end

	return false
end

function table.add(dest,source)
	-- At least one of them needs to be a table or this whole thing will fall on its ass
	if !istable(source) then return dest end
	if !istable(dest) then dest = {} end

	for k, v in pairs(source) do
		table.insert(dest,v)
	end

	return dest
end

function table.sortdesc(t)
	return table.sort(t,function(a,b) return a > b end)
end

function table.sortbykey(t,desc)
	local temp = {}

	for key,_ in pairs(t) do table.insert(temp,key) end

	if desc then
		table.sort(temp,function(a,b) return t[a] < t[b] end)
	else
		table.sort(temp,function(a,b) return t[a] > t[b] end)
	end

	return temp
end

function table.issequential(t)
	local i = 1

	for key,value in pairs(t) do
		if t[i] == nil then return false end

		i = i + 1
	end

	return true
end

local function MakeTable( t, nice, indent, done )
	local str = ''
	local done = done or {}
	local indent = indent or 0
	local idt = ''
	if nice then idt = string.rep( '\t', indent ) end
	local nl, tab  = '', ''
	if ( nice ) then nl, tab = '\n', '\t' end

	local sequential = table.issequential( t )

	for key, value in pairs( t ) do

		str = str .. idt .. tab .. tab

		if !sequential then
			if ( isnumber( key ) or isbool( key ) ) then
				key = '[' .. tostring( key ) .. ']' .. tab .. '='
			else
				key = tostring( key ) .. tab .. '='
			end
		else
			key = ''
		end

		if istable( value ) && !done[ value ] then
			done[ value ] = true
			str = str .. key .. tab .. '{' .. nl .. MakeTable (value, nice, indent + 1, done)
			str = str .. idt .. tab .. tab ..tab .. tab ..'},'.. nl
		else

			if isstring( value ) then
				value = '"' .. tostring( value ) .. '"'
			else
				value = tostring( value )
			end

			str = str .. key .. tab .. value .. ',' .. nl

		end

	end
	return str
end

function table.tostring(t,n,nice)
	local nl, tab  = '', ''
	if nice then nl, tab = '\n', '\t' end

	local str = ''
	if n then str = n .. tab .. '=' .. tab end
	
	return str .. '{' .. nl .. MakeTable( t, nice ) .. '}'
end

function table.forceinsert(t,v)
	if t == nil then t = {} end

	table.insert(t,v)

	return t
end

function table.sortbymember( Table, MemberName, bAsc )
	local TableMemberSort = function( a, b, MemberName, bReverse )

		--
		-- All this error checking kind of sucks, but really is needed
		--
		if !istable( a ) then return !bReverse end
		if !istable( b ) then return bReverse end
		if !a[ MemberName ] then return !bReverse end
		if !b[ MemberName ] then return bReverse end

		if isstring( a[ MemberName ] ) then

			if ( bReverse ) then
				return a[ MemberName ]:lower() < b[ MemberName ]:lower()
			else
				return a[ MemberName ]:lower() > b[ MemberName ]:lower()
			end

		end

		if bReverse then
			return a[ MemberName ] < b[ MemberName ]
		else
			return a[ MemberName ] > b[ MemberName ]
		end

	end

	table.sort( Table, function( a, b ) return TableMemberSort( a, b, MemberName, bAsc or false ) end )
end

function table.lowerkeynames(Table)

	local OutTable = {}

	for k, v in pairs(Table) do

		-- Recurse
		if istable(v) then
			v = table.lowerkeynames( v )
		end

		OutTable[ k ] = v

		if isstring( k ) then

			OutTable[ k ]  = nil
			OutTable[ string.lower( k ) ] = v

		end

	end

	return OutTable

end

function table.clearkeys( Table, bSaveKey )

	local OutTable = {}

	for k, v in pairs( Table ) do
		if ( bSaveKey ) then
			v.__key = k
		end
		table.insert( OutTable, v )
	end

	return OutTable

end

local function keyValuePairs( state )

	state.Index = state.Index + 1

	local keyValue = state.KeyValues[ state.Index ]
	if ( !keyValue ) then return end

	return keyValue.key, keyValue.val

end

local function toKeyValues( tbl )

	local result = {}

	for k,v in pairs( tbl ) do
		table.insert( result, { key = k, val = v } )
	end

	return result

end

function sortedpairs( pTable, Desc )

	local sortedTbl = toKeyValues( pTable )

	if ( Desc ) then
		table.sort( sortedTbl, function( a, b ) return a.key > b.key end )
	else
		table.sort( sortedTbl, function( a, b ) return a.key < b.key end )
	end

	return keyValuePairs, { Index = 0, KeyValues = sortedTbl }

end

function sortedpairsbyvalue( pTable, Desc )

	local sortedTbl = toKeyValues( pTable )

	if ( Desc ) then
		table.sort( sortedTbl, function( a, b ) return a.val > b.val end )
	else
		table.sort( sortedTbl, function( a, b ) return a.val < b.val end )
	end

	return keyValuePairs, { Index = 0, KeyValues = sortedTbl }

end

function sortedpairsbymembervalue( pTable, pValueName, Desc )

	local sortedTbl = toKeyValues( pTable )

	for k,v in pairs( sortedTbl ) do
		v.member = v.val[ pValueName ]
	end

	table.sortbymember( sortedTbl, "member", !Desc )

	return keyValuePairs, { Index = 0, KeyValues = sortedTbl }

end

function randompairs( pTable, Desc )

	local sortedTbl = toKeyValues( pTable )

	for k,v in pairs( sortedTbl ) do
		v.rand = math.random( 1, 1000000 )
	end

	-- descending/ascending for a random order, really?
	if ( Desc ) then
		table.sort( sortedTbl, function(a,b) return a.rand > b.rand end )
	else
		table.sort( sortedTbl, function(a,b) return a.rand < b.rand end )
	end

	return keyValuePairs, { Index = 0, KeyValues = sortedTbl }

end

function table.getfirstkey( t )
	local k, v = next( t )
	return k
end

function table.getfirstvalue( t )
	local k, v = next( t )
	return v
end

function table.getlastkey( t )
	local k, v = next( t, table.Count( t ) - 1 )
	return k
end

function table.getlastvalue( t )
	local k, v = next( t, table.Count( t ) - 1 )
	return v
end

function table.findnext( tab, val )
	local bfound = false
	for k, v in pairs( tab ) do
		if ( bfound ) then return v end
		if ( val == v ) then bfound = true end
	end

	return table.getfirstvalue( tab )
end

function table.findprev( tab, val )

	local last = table.GetLastValue( tab )
	for k, v in pairs( tab ) do
		if ( val == v ) then return last end
		last = v
	end

	return last

end

function table.keyfromvalue( tbl, val )
	for key, value in pairs( tbl ) do
		if ( value == val ) then return key end
	end
end

function table.removebyvalue( tbl, val )

	local key = table.keyfromvalue( tbl, val )
	if ( !key ) then return false end

	table.remove( tbl, key )
	return key

end

function table.keysfromvalue( tbl, val )
	local res = {}
	for key, value in pairs( tbl ) do
		if ( value == val ) then res[ #res + 1 ] = key end
	end
	return res
end

function table.reverse( tbl )

	local len = #tbl
	local ret = {}

	for i = len, 1, -1 do
		ret[ len - i + 1 ] = tbl[ i ]
	end

	return ret

end

function table.foreach( tab, funcname )

	for k, v in pairs( tab ) do
		funcname( k, v )
	end

end