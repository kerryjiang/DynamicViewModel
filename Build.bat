@echo off

set fdir=%WINDIR%\Microsoft.NET\Framework
set msbuild=%fdir%\v4.0.30319\msbuild.exe

%msbuild% DynamicViewModel\DynamicViewModel.csproj /p:Configuration=Debug /t:Rebuild /p:OutputPath=..\bin\Net40
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

%msbuild% DynamicViewModel.SL\DynamicViewModel.SL.csproj /p:Configuration=Debug /t:Rebuild /p:OutputPath=..\bin\SL
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

pause