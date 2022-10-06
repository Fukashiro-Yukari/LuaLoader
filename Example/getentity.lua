-- The VRCPlayer is the player entity in VRChat

local e = Object.FindObjectOfType(Il2CppType.From(typeof(VRCPlayer)))

print(e:Il2CppCast(typeof(VRCPlayer)))

-- Or

local e = ents.FindObjectOfType(typeof(VRCPlayer))

print(e)