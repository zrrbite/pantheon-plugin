# Legacy Steam Deck install script. Current dev is Windows; see CLAUDE.md.
# Bumped to BepInEx BE 755 for reference. The +<commit> suffix in the filename
# changes per build — grab the exact zip name from
# https://builds.bepinex.dev/projects/bepinex_be/755 before running.
cd '/run/media/mmcblk0p1/users/steamuser/Documents/My Games/Pantheon/App/'
wget 'https://builds.bepinex.dev/projects/bepinex_be/755/BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.755%2B<COMMIT>.zip'
unzip BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.755+<COMMIT>.zip
# launch Pantheon and see it patch. Close game.
# upload plugin to /Bepinex/plugins folder
# launch again