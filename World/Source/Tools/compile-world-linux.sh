mcs -optimize+ -unsafe -t:exe -out:WorldLinux.exe -win32icon:../System/icon.ico -nowarn:219,414 -d:NEWTIMERS -d:NEWPARENT -d:MONO -recurse:../System/*.cs -main:Server.Core
rm ../../WorldLinux.exe
mv ./WorldLinux.exe ../../
