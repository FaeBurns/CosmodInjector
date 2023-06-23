# Cosmoteer Mod Injector

## Installation
1. Download the zip file from the [Releases](https://github.com/FaeBurns/CosmoteerModInjector/Releases) tab.
2. Unzip it anywhere except for the game's `Bin` folder.
3. Run `Configurator.exe` or manually edit `game.txt` to point to the game's `Bin` folder.
4. Run `CosmoteerModInjector.exe` and the game will launch with C# mod support.

## What is this?
Cosmoteer Mod Injector (CMI) is a C# mod launcher/injector for the game [Cosmoteer](https://cosmoteer.net/).

### Doesn't this already exist?
Sort of. [EML](https://github.com/C0dingschmuser/EnhancedModLoader) exists but it loads too late to allow for things like custom `PartComponent` objects. <br/>
This program also uses [Harmony](https://github.com/pardeike/Harmony) to skip some checks that would further prevent such objects. <br/>
I haven't tested it but I don't think this loader is compatible with EML. It is however very simple to update any EML mods to work with CMI.

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

## License

[MIT](https://github.com/FaeBurns/CosmoteerModInjector/blob/master/License.txt)