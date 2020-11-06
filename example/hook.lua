
-- GUI rendering hook

hook.Add('OnGUI','Example',function()
    -- GUI functions can be found on https://docs.unity3d.com/ScriptReference/GUI.html

    GUI.Label(Rect(10,10,100,20),'Hello World!')

    GUI.color = Color.blue -- Change the color

    GUI.Label(Rect(10,30,100,20),'Hello World!')

    GUI.color = Color.white

    if GUI.Button(Rect(10,50,100,20),'Hello World!') then
        print('Do something')
    end
end)

-- Update hook

hook.Add('OnUpdate','Example',function()
    -- Do something
end)

-- Call when the level is loaded

hook.Add('OnLevelWasLoaded','Example',function(level)
    print('level is '..level)
end)