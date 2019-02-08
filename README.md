# ROMEncryption

The Unity dll's are XOR'd, and then the main one has some weird encryption on the .text section. I didn't care to figure it out, but you can hardcode the pointer to correct it, and then decompile it with a .NET decompiler.

The Lua scripts are within the .unity3d files - you will need a .unity3d extractor to dump the raw blobs. The binary blobs can then be decrypted with ROM's custom DES cipher as seen in ROMDesCipher.cs

All the Lua scripts were also dumped to https://github.com/shalzuth/rom_files

This can be used to decrypt/re-encrypt the main login Lua script at login/CSharpObjectForLogin.lua, then re-encrypt with the same function, push it back into the unity3d file, then voila! You have Lua scripting within ROM.

I also included an example modified [CSharpObjectForLogin.lua](https://github.com/shalzuth/ROMEncryption/blob/master/CSharpObjectForLogin.lua#L90-L144) that will load whatever Lua script is at "/data/local/tmp/script/rom.lua" and execute it. Additionally, I've included a [rom.lua](https://github.com/shalzuth/ROMEncryption/blob/master/rom.lua) to show basic functionality of zoom hack, removing fog, and setting the FPS.
