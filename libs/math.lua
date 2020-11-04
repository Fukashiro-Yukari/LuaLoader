function math.distance( x1, y1, x2, y2 )
	local xd = x2 - x1
	local yd = y2 - y1
	return math.sqrt( xd * xd + yd * yd )
end
math.dist = math.distance -- Backwards compatibility

function math.bintoint( bin )
	return tonumber( bin, 2 )
end

local intbin = {
	["0"] = "000", ["1"] = "001", ["2"] = "010", ["3"] = "011",
	["4"] = "100", ["5"] = "101", ["6"] = "110", ["7"] = "111"
}

function math.inttobin( int )
	local str = string.gsub( string.format( "%o", int ), "(.)", function ( d ) return intbin[ d ] end )
	return str
end

function math.clamp( _in, low, high )
	return math.min( math.max( _in, low ), high )
end

function math.rand( low, high )
	return low + ( high - low ) * math.random()
end

function math.easeinout( fProgress, fEaseIn, fEaseOut )

	if ( fEaseIn == nil ) then fEaseIn = 0 end
	if ( fEaseOut == nil ) then fEaseOut = 1 end

	if ( fProgress == 0 || fProgress == 1 ) then return fProgress end

	local fSumEase = fEaseIn + fEaseOut

	if ( fSumEase == 0 ) then return fProgress end
	if ( fSumEase > 1 ) then
		fEaseIn = fEaseIn / fSumEase
		fEaseOut = fEaseOut / fSumEase
	end

	local fProgressCalc = 1 / ( 2 - fEaseIn - fEaseOut )

	if( fProgress < fEaseIn ) then
		return ( ( fProgressCalc / fEaseIn ) * fProgress * fProgress )
	elseif( fProgress < 1 - fEaseOut ) then
		return ( fProgressCalc * ( 2 * fProgress - fEaseIn ) )
	else
		fProgress = 1 - fProgress
		return ( 1 - ( fProgressCalc / fEaseOut ) * fProgress * fProgress )
	end
end

local function KNOT( i, tinc ) return ( i - 3 ) * tinc end

function math.calcbsplinen( i, k, t, tinc )

	if ( k == 1 ) then

		if ( ( KNOT( i, tinc ) <= t ) && ( t < KNOT( i + 1, tinc ) ) ) then

			return 1

		else

			return 0

		end

	else

		local ft = ( t - KNOT( i, tinc ) ) * math.calcbsplinen( i, k - 1, t, tinc )
		local fb = KNOT( i + k - 1, tinc ) - KNOT( i, tinc )

		local st = ( KNOT( i + k, tinc ) - t ) * math.calcbsplinen( i + 1, k - 1, t, tinc )
		local sb = KNOT( i + k, tinc ) - KNOT( i + 1, tinc )

		local first = 0
		local second = 0

		if ( fb > 0 ) then

			first = ft / fb

		end
		if ( sb > 0 ) then

			second = st / sb

		end

		return first + second

	end

end

function math.bsplinepoint( tDiff, tPoints, tMax )

	local Q = Vector3( 0, 0, 0 )
	local tinc = tMax / ( #tPoints - 3 )

	tDiff = tDiff + tinc

	for idx, pt in pairs( tPoints ) do

		local n = math.calcbsplinen( idx, 4, tDiff, tinc )
		Q = Q + ( n * pt )

	end

	return Q

end

function math.round( num, idp )

	local mult = 10 ^ ( idp or 0 )
	return math.floor( num * mult + 0.5 ) / mult

end

function math.truncate( num, idp )

	local mult = 10 ^ ( idp or 0 )
	local FloorOrCeil = num < 0 and math.ceil or math.floor

	return FloorOrCeil( num * mult ) / mult

end

function math.approach( cur, target, inc )

	inc = math.abs( inc )

	if ( cur < target ) then

		return math.min( cur + inc, target )

	elseif ( cur > target ) then

		return math.max( cur - inc, target )

	end

	return target

end

function math.timefraction( Start, End, Current )
	return ( Current - Start ) / ( End - Start )
end

function math.remap( value, inMin, inMax, outMin, outMax )
	return outMin + ( ( ( value - inMin ) / ( inMax - inMin ) ) * ( outMax - outMin ) )
end