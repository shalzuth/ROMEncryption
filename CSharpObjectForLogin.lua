CSharpObjectForLogin = class('CSharpObjectForLogin')

CSharpObjectForLogin.ins = nil
function CSharpObjectForLogin:Ins()
	if CSharpObjectForLogin.ins == nil then
		CSharpObjectForLogin.ins = CSharpObjectForLogin.new()
	end
	return CSharpObjectForLogin
end

local goGameRoleReadyForLogin = nil
local transCamera = nil
local cameraController = nil

function CSharpObjectForLogin:Initialize(completeCallback)
	self:GetObjects()

	if ObjectsIsGetted then
		if completeCallback ~= nil then
			completeCallback()
		end
	else
		if self.tick == nil then
			self.tick = TimeTickManager.Me():CreateTick(0, 500, self.OnTick, self, 1)
		end
		self.completeCallback = completeCallback
	end
end

function CSharpObjectForLogin:GetCameraController()
	return cameraController
end

function CSharpObjectForLogin:GetTransCamera()
	return transCamera
end

function CSharpObjectForLogin:Release()
	transCamera = nil
	cameraController = nil

	GameObject.Destroy(goGameRoleReadyForLogin)
	goGameRoleReadyForLogin = nil
end

function CSharpObjectForLogin:Reset()
	self:Release()
end

function CSharpObjectForLogin:OnTick()
	self:GetObjects()

	if self:ObjectsIsGetted() then
		TimeTickManager.Me():ClearTick(self, 1)
		self.tick = nil

		if self.completeCallback ~= nil then
			self.completeCallback()
		end
	end
end

function CSharpObjectForLogin:GetObjects()
	goGameRoleReadyForLogin = GameObject.Find('GameRoleReadyForLogin(Clone)')
	if goGameRoleReadyForLogin ~= nil then
		transCamera = goGameRoleReadyForLogin.transform:Find('Camera')
		local transCameraController = goGameRoleReadyForLogin.transform:Find('CameraController')
		cameraController = transCameraController:GetComponent('CameraControllerForLoginScene')
	end
end

function CSharpObjectForLogin:ObjectsIsGetted()
	return goGameRoleReadyForLogin
end

function resetForLoginUnity()
	
end

autoImport("Table_MainViewButton")
Table_MainViewButton[#Table_MainViewButton + 1] = {
  id = #Table_MainViewButton + 1,
  name = "Shalzuth",
  icon = "setup",
  panelid = 630,
  redtiptype = _EmptyTable,
  Enterhide = _EmptyTable
}

TEXT_COMMANDS = TEXT_COMMANDS or {}
TEXT_COMMANDS["@test"] = function(params)
    local f = io.open("/data/local/tmp/script/rom.lua")
    local s = f:read("*a")
    f:close()
	local f2 = loadstring(s);
    if f2~=nil then
        local status, err =  pcall(f2);
        if status == false then
            if UIUtil ~= nil then
                UIUtil.FloatMsgByText(err);		
            end;
            return false;
        else
            if UIUtil ~= nil then
                UIUtil.FloatMsgByText("Success");		
            end;
            return true;
        end;
    else
        if UIUtil ~= nil then
            UIUtil.FloatMsgByText("File failed to load");		
        end;
		return false;
    end;
    return    
end

autoImport("ChatSystemManager")
function ChatSystemManager:CheckChatContent_Hook(msg)
    if msg == "" then
        return
    end
    local is_command = false
    local commands = StringUtil.Split(msg, ";")
    for k, v in pairs(commands) do
        local first_char = string.sub(v, 1, 1)
        if first_char == "@" or first_char == "#" or first_char == "/" then
            v = string.lower(v)
            local params = StringUtil.Split(v, " ")
            local cmd = string.gsub(params[1], "[#|/]", "@", 1)
            local status, error = pcall(TEXT_COMMANDS[cmd], params)
            if not status then
                local err = string.format("ERROR EXECUTING CMD: %s\n\nERROR CODE: %s", cmd, error)
                TipsView.Me():ShowGeneralHelp(err, "Information")
            else
                pcall(SETTINGS_SAVE)
            end
            is_command = true
        end
    end
    return is_command and true or self:CheckChatContent_Orig(msg)
end
ChatSystemManager.CheckChatContent_Orig = ChatSystemManager.CheckChatContent_Orig or ChatSystemManager.CheckChatContent
ChatSystemManager.CheckChatContent = ChatSystemManager.CheckChatContent_Hook
