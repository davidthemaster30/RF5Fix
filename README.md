# Rune Factory 5 Fix

This BepInEx plugin for the game Rune Factory 5 features:
- Proper ultrawide and non-16:9 aspect ratio support with pillarbox removal.
- Smoother camera movement with a higher update rate.
- Intro/logos skip.
- Graphical tweaks to increase fidelity.
- Adjusting field of view.
- Vert+ FOV at narrower than 16:9 aspect ratios.
- High quality model previews in menus.
- Ability to disable cross-hatching effect.
- Overriding controller icons/glyphs.
- Overriding mouse sensitivity.

## Installation
- Grab the latest release of RF5Fix from [here.](https://github.com/davidthemaster30/RF5Fix/releases)
- Extract the contents of the release zip in to the game directory.<br />(e.g. "**steamapps\common\Rune Factory 5**" for Steam).
- (Optional) Get the [BepInEx Configuration Manager](https://github.com/BepInEx/BepInEx.ConfigurationManager)
- The first launch may take a little while as BepInEx does its magic.

### Linux
- If you are running Linux (for example with the Steam Deck) then the game needs to have it's launch option changed to load BepInEx.
- You can do this by going to the game properties in Steam and finding "LAUNCH OPTIONS".
- Make sure the launch option is set to: ```WINEDLLOVERRIDES="winhttp=n,b" %command%```

| ![steam launch options](https://raw.githubusercontent.com/davidthemaster30/RF5Fix/25978dd30d3d8aacdf29f4395ee16d2407e15f0e/Media/launchoptions.jpeg) |
|:--:|
| Steam launch options. |

## Configuration
- See the generated config file to adjust various aspects of the plugin.

## Known Issues
Please report any issues you see.

- Run into issues after updating the mod? Try deleting your config file, then booting the game to generate a new one.
- If you get startup issues try disabling "Show launcher at start" in the game launcher as shown in the picture below.

| ![launcher](https://raw.githubusercontent.com/davidthemaster30/RF5Fix/25978dd30d3d8aacdf29f4395ee16d2407e15f0e/Media/launcher.jpg) |
|:--:|
| Thanks to pho on the WSGF Discord. |

## Screenshots
| ![ultrawide](https://raw.githubusercontent.com/davidthemaster30/RF5Fix/25978dd30d3d8aacdf29f4395ee16d2407e15f0e/Media/ultrawide.gif) |
|:--:|
| Ultrawide pillarbox removal. | 

## Credits
- [BepinEx](https://github.com/BepInEx/BepInEx) is licensed under the GNU Lesser General Public License v2.1.
- [@Lyall](https://github.com/Lyall) for the original mod.
- [@KingKrouch](https://github.com/KingKrouch) for various contributions.
