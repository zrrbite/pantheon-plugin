rm -r assemblies_old
mv assemblies/ assemblies_old/
rm -r assemblies
scp -r deck@192.168.86.42:'/run/media/mmcblk0p1/users/steamuser/Documents/My Games/Pantheon/App/BepInEx/interop/' assemblies

