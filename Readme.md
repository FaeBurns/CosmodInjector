# Cosmoteer Mod Injector

## Installation
1. Download the zip file from the [Releases](https://github.com/FaeBurns/CosmodInjector/Releases) tab.
2. Unzip it anywhere except for the game's `Bin` folder.
3. Run `Configurator.exe` or manually edit `game.txt` to point to the game's `Bin` folder.
4. Run `CosmoteerModInjector.exe` and the game will launch with C# mod support.
5. (Optional) Add `CosmoteerModInjector.exe` as a non-steam game to your library.

## What is this?
Cosmoteer Mod Injector (Cosmod) (CMI) is a C# mod launcher/injector for the game [Cosmoteer](https://cosmoteer.net/).

### Doesn't this already exist?
[EML](https://github.com/C0dingschmuser/EnhancedModLoader) as another C# mod loader for cosmoteer however it loads too late to allow for custom `PartComponent` types to be detected, meaning mods could not make use of them. CMI allows for the use of such components by loading the C# libraries right before the relevant mod is loaded. 

## How do I use mods?
Simply install a mod manually or from the steam workshop and CMI will automatically load any compatible C# components if they are present. <br/>
CMI only loads mods that are enabled inside of Cosmoteer.
any mods inside `Cosmoteer/UserMods` will always be loaded

## How do I go back to playing without C# mods?
Launching Cosmoteer through steam as you would normally will not load any CMI mods. mods that require their C# component to work will cause an error on game launch. Either launch without mods enabled or disable them when prompted to fix this.  

## How can I create mods?
A good place to start is with the template included in this repository. <br/>
Use of an assembly publicizer is required. The template is already set up with [Kraf's Publicizer](https://github.com/krafs/Publicizer) however you may have to modify the paths of the references to `Cosmoteer.dll` and `HalflingCore.dll` at the bottom to point towards your install. <br/>
[DNSpy](https://github.com/dnSpyEx/dnSpy) is recommended when trying to discover how the game operates.

## What mods are there?
Currently, none. I am working on a few mods at the moment and they will be listed here after a bit more testing.

## License

[MIT](https://github.com/FaeBurns/CosmodInjector/blob/main/License.txt)