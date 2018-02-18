echo %1

set targetDir=%1

set copyTargetDir=%targetDir%\bin\Debug\net461

cd ..\..\
mkdir "%copyTargetDir%\SpecFlow\Tools"
mkdir "%copyTargetDir%\SpecFlow\lib"

mkdir "%copyTargetDir%\NUnit3\"
mkdir "%copyTargetDir%\NUnit3-Runner\"
mkdir "%copyTargetDir%\NUnit\"
mkdir "%copyTargetDir%\NUnit-Runner\"
mkdir "%copyTargetDir%\xUnit2\"
mkdir "%copyTargetDir%\xUnit\"
mkdir "%copyTargetDir%\xunit.runner.console\"
mkdir "%copyTargetDir%\FSharp\"

xcopy .\SpecFlow.Build.Tasks\build\* "%copyTargetDir%\SpecFlow.Build.Tasks\build\" /s /y
xcopy .\SpecFlow.Build.Tasks\buildMultiTargeting\* "%copyTargetDir%\SpecFlow.Build.Tasks\buildMultiTargeting\" /s /y
xcopy .\SpecFlow.Build.Tasks\bin\Debug\net461\* "%copyTargetDir%\SpecFlow.Build.Tasks\tools\net461\" /s /y

copy .\TechTalk.SpecFlow.Tools\bin\Debug\net461\SpecFlow.* "%copyTargetDir%\SpecFlow\Tools"
copy .\TechTalk.SpecFlow\bin\Debug\net461\*.* "%copyTargetDir%\SpecFlow\Tools"
copy .\TechTalk.SpecFlow.Utils\bin\Debug\net461\*.* "%copyTargetDir%\SpecFlow\Tools"
copy .\TechTalk.SpecFlow.Reporting\bin\Debug\net461\*.* "%copyTargetDir%\SpecFlow\Tools"
copy .\TechTalk.SpecFlow.Parser\bin\Debug\net461\*.* "%copyTargetDir%\SpecFlow\Tools"
copy .\TechTalk.SpecFlow.Generator\bin\Debug\net461\*.* "%copyTargetDir%\SpecFlow\Tools"
copy "%targetDir%\bin\Debug\net461\Gherkin.dll" "%copyTargetDir%\SpecFlow\Tools"
copy "%targetDir%\bin\Debug\net461\Gherkin.dll" "%copyTargetDir%\SpecFlow\Tools"

copy .\TechTalk.SpecFlow.Tools\bin\Debug\net461\SpecFlow.* "%copyTargetDir%\SpecFlow\lib"
copy .\TechTalk.SpecFlow\bin\Debug\net461\*.* "%copyTargetDir%\SpecFlow\lib"
copy .\TechTalk.SpecFlow.Utils\bin\Debug\net461\*.* "%copyTargetDir%\SpecFlow\lib"
copy .\TechTalk.SpecFlow.Reporting\bin\Debug\net461\*.* "%copyTargetDir%\SpecFlow\lib"
copy .\TechTalk.SpecFlow.Parser\bin\Debug\net461\*.* "%copyTargetDir%\SpecFlow\lib"
copy .\TechTalk.SpecFlow.Generator\bin\Debug\net461\*.* "%copyTargetDir%\SpecFlow\lib"
copy "%targetDir%\bin\Debug\net461\Gherkin.dll" "%copyTargetDir%\SpecFlow\lib"
copy "%targetDir%\bin\Debug\net461\Gherkin.dll" "%copyTargetDir%\SpecFlow\lib"

rem xcopy "%USERPROFILE%\.nuget\packages\NUnit\3.2.1\*" "%copyTargetDir%\NUnit3\" /s /y
rem xcopy ".\NuGet\custom\NUnit3-Runner\*" "%copyTargetDir%\NUnit3-Runner\" /s /y
xcopy ".\lib\xunit.2.0.0\*.*" "%copyTargetDir%\xUnit2\" /s /y
xcopy ".\lib\Microsoft F#\*.*" "%copyTargetDir%\FSharp\" /s /y

xcopy "%USERPROFILE%\.nuget\packages\mstest.testadapter\1.2.0\*" "%copyTargetDir%\packages\mstest.testadapter\" /s /y
xcopy "%USERPROFILE%\.nuget\packages\mstest.testframework\1.2.0\*" "%copyTargetDir%\packages\mstest.testframework\" /s /y

xcopy "%USERPROFILE%\.nuget\packages\NUnit\2.6.4\*" "%copyTargetDir%\NUnit\" /s /y
xcopy "%USERPROFILE%\.nuget\packages\NUnit.Runners\2.6.4\*" "%copyTargetDir%\NUnit.Runners\" /s /y

xcopy "%USERPROFILE%\.nuget\packages\NUnit.Extension.NUnitProjectLoader\3.6.0\*" "%copyTargetDir%\NUnit3-Runner\" /s /y
xcopy "%USERPROFILE%\.nuget\packages\NUnit.Extension.VSProjectLoader\3.6.0\*" "%copyTargetDir%\NUnit3-Runner\" /s /y
xcopy "%USERPROFILE%\.nuget\packages\NUnit.Extension.NUnitV2ResultWriter\3.7.0\*" "%copyTargetDir%\NUnit3-Runner\" /s /y
xcopy "%USERPROFILE%\.nuget\packages\NUnit.Extension.NUnitV2Driver\3.7.0\*" "%copyTargetDir%\NUnit3-Runner\" /s /y
rem xcopy "%USERPROFILE%\.nuget\packages\NUnit.Extension.TeamCityEventListener\1.0.2\*" "%copyTargetDir%\NUnit3-Runner\" /s /y
xcopy "%USERPROFILE%\.nuget\packages\NUnit.ConsoleRunner\3.7.0\*" "%copyTargetDir%\NUnit3-Runner\" /s /y
xcopy "%USERPROFILE%\.nuget\packages\NUnit\3.8.1\*" "%copyTargetDir%\NUnit3\" /s /y


echo nunit-v2-result-writer.dll > "%copyTargetDir%\NUnit3-Runner\tools\.addins"
echo vs-project-loader.dll >> "%copyTargetDir%\NUnit3-Runner\tools\.addins"
echo nunit.v2.driver.dll >> "%copyTargetDir%\NUnit3-Runner\tools\.addins"
echo nunit-project-loader.dll >> "%copyTargetDir%\NUnit3-Runner\tools\.addins"

xcopy "%USERPROFILE%\.nuget\packages\xunit\1.9.2\*.*" "%copyTargetDir%\xUnit\" /s /y
xcopy "%USERPROFILE%\.nuget\packages\xunit.extensions\1.9.2\*.*" "%copyTargetDir%\xUnit\" /s /y


xcopy "%USERPROFILE%\.nuget\packages\xunit.runner.console\2.2.0\*.*" "%copyTargetDir%\xunit.runner.console\" /s /y

rem xcopy "%USERPROFILE%\.nuget\packages\SpecFlow\1.9.0\lib\net35\*.*" "%copyTargetDir%\" /s /y

xcopy ".\lib\mbunit.3.3.442.0\*.*" "%copyTargetDir%\mbUnit3\" /s /y