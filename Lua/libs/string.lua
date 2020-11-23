function string.split(str,delimiter)
    local strlist, tmp = {},string.byte(delimiter)

    if delimiter == '' then
        for i = 1, #str do strlist[i] = str:sub(i,i) end
    else
        for substr in string.gmatch(str .. delimiter,'(.-)'..(((tmp > 96 and tmp < 123) or (tmp > 64 and tmp < 91) or (tmp > 47 and tmp < 58)) and delimiter or '%'..delimiter)) do
            table.insert(strlist,substr)
        end
    end
    
    return strlist
end

function string.splittostrip(str,delimiter)
    local strlist, tmp = {},string.byte(delimiter)

    if delimiter == '' then
        for i = 1, #str do strlist[i] = str:sub(i,i) end
    else
        for substr in string.gmatch(str .. delimiter,'(.-)'..(((tmp > 96 and tmp < 123) or (tmp > 64 and tmp < 91) or (tmp > 47 and tmp < 58)) and delimiter or '%'..delimiter)) do
            if substr ~= '' and substr ~= ' ' then
                table.insert(strlist,substr)
            end
        end
    end
    
    return strlist
end

function string.tohex(str,separator)
    return str:gsub('.', function(c)
        return string.format('%02X' ..(separator or ''),string.byte(c))
    end)
end

function string.fromhex(hex)
    local hex = hex:gsub('[%s%p]',''):upper()

    return hex:gsub('%x%x', function(c)
        return string.char(tonumber(c,16))
    end)
end

function string.tovalue(str)
    return string.fromHex(str:gsub('%x','0%1'))
end

function string.utf8len(str)
    local len = #str
    local left = len
    local cnt = 0
    local arr = {0, 0xc0, 0xe0, 0xf0, 0xf8, 0xfc}

    while left ~= 0 do
        local tmp = string.byte(str, -left)
        local i = #arr

        while arr[i] do
            if tmp >= arr[i] then
                left = left - i
                break
            end
            i = i - 1
        end

        cnt = cnt + 1
    end

    return cnt
end

local function urlEncodeChar(c)
    return '%' .. string.format('%02X', string.byte(c))
end

function string.urlencode(str)
    return string.gsub(string.gsub(string.gsub(tostring(str),'\n','\r\n'),'([^%w%.%- ])', urlEncodeChar),' ','+')
end

function string.totable( str )
	local tbl = {}

	for i = 1, string.len( str ) do
		tbl[i] = string.sub( str, i, i )
	end

	return tbl
end

local pattern_escape_replacements = {
	["("] = "%(",
	[")"] = "%)",
	["."] = "%.",
	["%"] = "%%",
	["+"] = "%+",
	["-"] = "%-",
	["*"] = "%*",
	["?"] = "%?",
	["["] = "%[",
	["]"] = "%]",
	["^"] = "%^",
	["$"] = "%$",
	["\0"] = "%z"
}

function string.patternsafe( str )
	return ( str:gsub( ".", pattern_escape_replacements ) )
end

function string.getextensionfromfilename( path )
	return path:match( "%.([^%.]+)$" )
end

function string.stripextension( path )
	local i = path:match( ".+()%.%w+$" )
	if ( i ) then return path:sub( 1, i - 1 ) end
	return path
end

function string.getpathfromfilename( path )
	return path:match( "^(.*[/\\])[^/\\]-$" ) or ""
end

function string.getfilefromfilename( path )
	if ( !path:find( "\\" ) && !path:find( "/" ) ) then return path end 
	return path:match( "[\\/]([^/\\]+)$" ) or ""
end

function string.formattedtime( seconds, format )
	if ( not seconds ) then seconds = 0 end
	local hours = math.floor( seconds / 3600 )
	local minutes = math.floor( ( seconds / 60 ) % 60 )
	local millisecs = ( seconds - math.floor( seconds ) ) * 100
	seconds = math.floor( seconds % 60 )

	if ( format ) then
		return string.format( format, minutes, seconds, millisecs )
	else
		return { h = hours, m = minutes, s = seconds, ms = millisecs }
	end
end

function string.tominutesmecondsmilliseconds( TimeInSeconds ) return string.formattedtime( TimeInSeconds, "%02i:%02i:%02i" ) end
function string.tominutesmeconds( TimeInSeconds ) return string.formattedtime( TimeInSeconds, "%02i:%02i" ) end

local function pluralizeString( str, quantity )
	return str .. ( ( quantity ~= 1 ) and "s" or "" )
end

function string.nicetime( seconds )

	if ( seconds == nil ) then return "a few seconds" end

	if ( seconds < 60 ) then
		local t = math.floor( seconds )
		return t .. pluralizeString( " second", t )
	end

	if ( seconds < 60 * 60 ) then
		local t = math.floor( seconds / 60 )
		return t .. pluralizeString( " minute", t )
	end

	if ( seconds < 60 * 60 * 24 ) then
		local t = math.floor( seconds / (60 * 60) )
		return t .. pluralizeString( " hour", t )
	end

	if ( seconds < 60 * 60 * 24 * 7 ) then
		local t = math.floor( seconds / ( 60 * 60 * 24 ) )
		return t .. pluralizeString( " day", t )
	end

	if ( seconds < 60 * 60 * 24 * 365 ) then
		local t = math.floor( seconds / ( 60 * 60 * 24 * 7 ) )
		return t .. pluralizeString( " week", t )
	end

	local t = math.floor( seconds / ( 60 * 60 * 24 * 365 ) )
	return t .. pluralizeString( " year", t )

end

function string.left( str, num ) return string.sub( str, 1, num ) end
function string.right( str, num ) return string.sub( str, -num ) end

function string.trim( s, char )
	if ( char ) then char = char:patternsafe() else char = "%s" end
	return string.match( s, "^" .. char .. "*(.-)" .. char .. "*$" ) or s
end

function string.trimright( s, char )
	if ( char ) then char = char:patternsafe() else char = "%s" end
	return string.match( s, "^(.-)" .. char .. "*$" ) or s
end

function string.trimleft( s, char )
	if ( char ) then char = char:patternsafe() else char = "%s" end
	return string.match( s, "^" .. char .. "*(.+)$" ) or s
end

function string.nicesize( size )

	size = tonumber( size )

	if ( size <= 0 ) then return "0" end
	if ( size < 1000 ) then return size .. " Bytes" end
	if ( size < 1000 * 1000 ) then return math.round( size / 1000, 2 ) .. " KB" end
	if ( size < 1000 * 1000 * 1000 ) then return math.round( size / ( 1000 * 1000 ), 2 ) .. " MB" end

	return math.round( size / ( 1000 * 1000 * 1000 ), 2 ) .. " GB"

end

function string.setchar( s, k, v )

	local start = s:sub( 0, k - 1 )
	local send = s:sub( k + 1 )

	return start .. v .. send

end

function string.startwith( String, Start )

	return string.sub( String, 1, string.len( Start ) ) == Start

end

function string.endswith( String, End )

	return End == "" or string.sub( String, -string.len( End ) ) == End

end

function string.comma( number )

	if ( isnumber( number ) ) then
		number = string.format( "%f", number )
		number = string.match( number, "^(.-)%.?0*$" ) -- Remove trailing zeros
	end

	local k

	while true do
		number, k = string.gsub( number, "^(-?%d+)(%d%d%d)", "%1,%2" )
		if ( k == 0 ) then break end
	end

	return number

end