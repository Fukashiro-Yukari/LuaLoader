--[[
Example:

import('c# dll name','c# namespace')

You can put the c# dll in the same directory as the game exe
]]

import('Assembly-CSharp')
import('UnityEngine')
import('UnityEngine.UI')

if CPP then
    import('UnhollowerBaseLib')
    import('UnhollowerRuntimeLib')
    import('UnhollowerBaseLib', 'UnhollowerRuntimeLib')
end

import('System')
import('System', 'System.Type')
import('System.IO')
import('System.Runtime', 'System.Reflection')
