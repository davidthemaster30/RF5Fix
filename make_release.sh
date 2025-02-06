rm -rf obj
rm -rf bin

dotnet build RF5Fix.csproj -f net6.0 -c Release

cp 'BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.725+e1974e2.zip' 'RF5Fix_v1.0.2.zip'

mkdir -p BepInEx/plugins
mkdir -p BepInEx/config

cp './bin/Release/net6.0/RF5Fix.dll' BepInEx/plugins/RF5Fix.dll
cp './RF5Fix.cfg' BepInEx/config/RF5Fix.cfg

cp './bin/Release/net6.0/RF5Fix.dll' '/data/Steam/steamapps/common/Rune Factory 5/BepInEx/plugins'

zip -r 'RF5Fix_v1.0.2.zip' BepInEx

rm -rf BepInEx