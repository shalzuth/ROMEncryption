-- CameraController.singletonInstance.zoomMin = 0.1;
-- UnityEngine.Application.targetFrameRate = 60.0;
-- RenderSettings.fog = false;

if DebugLog == nil then
	DebugLog = {};
	DebugLog.File = io.open("/data/local/tmp/script/rom.log", "a");
    DebugLog.Write = function(data)
        DebugLog.File:write(os.date("%x %X") .. ' ' .. data .. "\n");
        DebugLog.File:flush();
    end;
end;
if Game~=nil and Game.me~=nil then
    DebugLog.Write(tostring(Game.me));
end;

function SetFrameRate(fps)
    UnityEngine.Application.targetFrameRate = fps;
end;

function ZoomHack(apply)
    if apply then
        CameraController.singletonInstance.zoomMin = 0.1;
    else
        CameraController.singletonInstance.zoomMin = 0.7;     
    end
end;

function RemoveFog()
    RenderSettings.fog = false;
end;

ZoomHack(true);
RemoveFog();
SetFrameRate(60.0);


DebugLog.Write("loaded");

