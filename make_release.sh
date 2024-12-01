rm -rf obj
rm -rf bin

dotnet build RF5Fix.csproj -f net6.0 -c Release

zip -j 'RF5Fix_v1.0.0.zip' './bin/Release/net6.0/RF5Fix.dll' './RF5Fix.cfg'
