local tonumber = tonumber
local table = table
local util = util
local unpack = unpack
local pairs = pairs

module('timer', package.seeall)

local simpletimer = {}
local timers = {}
local stoptimers = {}

function Adjust(identifier, delay, repetitions, func, ...)
    if not timers[identifier] and not stoptimers[identifier] then
        return false
    end

    delay = tonumber(delay)
    repetitions = tonumber(repetitions)

    local t = timers[identifier] or stoptimers[identifier]

    t.delay = Time.time + delay
    t.adddelay = delay
    t.repetitions = repetitions or 0
    t.nowrepetitions = repetitions - 1 or 0
    t.func = func
    t.args = {...}

    return true
end

function Create(identifier, delay, repetitions, func, ...)
    delay = tonumber(delay)
    repetitions = tonumber(repetitions)

    Remove(identifier)

    timers[identifier] = {
        delay = Time.time + delay,
        adddelay = delay,
        repetitions = repetitions or 0,
        nowrepetitions = repetitions - 1 or 0,
        func = func,
        args = {...}
    }
end

function Exists(identifier)
    if timers[identifier] or stoptimers[identifier] then return true end

    return false
end

function Pause(identifier)
    if not timers[identifier] then return false end

    local t = timers[identifier]

    stoptimers[identifier] = table.copy(timers[identifier])
    timers[identifier] = nil

    return true
end

function Remove(identifier)
    if not timers[identifier] and not stoptimers[identifier] then return end

    timers[identifier] = nil
    stoptimers[identifier] = nil
end

function RepsLeft(identifier)
    if not timers[identifier] and not stoptimers[identifier] then return end

    local t = timers[identifier] or stoptimers[identifier]

    return t.nowrepetitions
end

function Simple(delay, func, ...)
    delay = tonumber(delay)

    simpletimer[util.GetId()] = {
        delay = Time.time + delay,
        func = func,
        args = {...}
    }
end

function Start(identifier)
    if not timers[identifier] and not stoptimers[identifier] then
        return false
    end
    if not timers[identifier] then
        timers[identifier] = table.copy(stoptimers[identifier])
        stoptimers[identifier] = nil
    end

    local t = timers[identifier]

    t.nowrepetitions = t.repetitions - 1

    return true
end

function Stop(identifier)
    if not timers[identifier] then return false end

    stoptimers[identifier] = table.copy(timers[identifier])
    timers[identifier] = nil

    local t = stoptimers[identifier]

    t.nowrepetitions = t.repetitions - 1

    return true
end

function TimeLeft(identifier)
    if not timers[identifier] and not stoptimers[identifier] then return end

    local t = timers[identifier] or stoptimers[identifier]

    return t.delay - Time.time
end

function Toggle(identifier)
    if not timers[identifier] and not stoptimers[identifier] then
        return false
    end
    if timers[identifier] then
        Pause(identifier)

        return false
    end

    UnPause(identifier)

    return true
end

function UnPause(identifier)
    if not stoptimers[identifier] then return false end

    timers[identifier] = table.copy(stoptimers[identifier])
    stoptimers[identifier] = nil

    return true
end

function OnUpdate()
    for k, v in pairs(simpletimer) do
        if v.delay <= Time.time then
            v.func(unpack(v.args))

            simpletimer[k] = nil
        end
    end

    for k, v in pairs(timers) do
        if v.delay <= Time.time then
            v.func(unpack(v.args))

            local t = timers[k]

            if v.repetitions == 0 or v.nowrepetitions > 0 then
                t.delay = Time.time + t.adddelay

                if v.nowrepetitions > 0 then
                    t.nowrepetitions = t.nowrepetitions - 1
                end
            else
                stoptimers[k] = v
                timers[k] = nil
            end
        end
    end
end

hook.Add('OnUpdate', 'timer', OnUpdate)
