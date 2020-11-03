function string.split(str,delimiter)
    local strlist, tmp = {},string.byte(delimiter)

    if delimiter == "" then
        for i = 1, #str do strlist[i] = str:sub(i,i) end
    else
        for substr in string.gmatch(str .. delimiter,"(.-)"..(((tmp > 96 and tmp < 123) or (tmp > 64 and tmp < 91) or (tmp > 47 and tmp < 58)) and delimiter or "%"..delimiter)) do
            table.insert(strlist,substr)
        end
    end
    
    return strlist
end

function string.splittostrip(str,delimiter)
    local strlist, tmp = {},string.byte(delimiter)

    if delimiter == "" then
        for i = 1, #str do strlist[i] = str:sub(i,i) end
    else
        for substr in string.gmatch(str .. delimiter,"(.-)"..(((tmp > 96 and tmp < 123) or (tmp > 64 and tmp < 91) or (tmp > 47 and tmp < 58)) and delimiter or "%"..delimiter)) do
            if substr ~= '' and substr ~= ' ' then
                table.insert(strlist,substr)
            end
        end
    end
    
    return strlist
end