using LabApi.Events.CustomHandlers;
using LabApi.Features;
using LabApi.Features.Console;
using LabApi.Loader.Features.Plugins;
using System.Reflection;

namespace MicroHIDReturn;

public class Plugin : Plugin<Config>
{
    public static Plugin Singleton { get; set; } = null!;
    
    public override string Name => "MicroHIDReturn";
    public override string Description { get; } = "Plugin that allows returning Micro HID to its pedestal and even charge it";
    public override string Author { get; } = "Engi LND";
    public override Version Version { get; } = new(1, 3, 0);
    public override Version RequiredApiVersion { get; } = new(LabApiProperties.CompiledVersion);

    
    private CustomEventsHandler _customEventsHandler;
    
    public override void Enable() {
        Singleton = this;
        
        CustomHandlersManager.RegisterEventsHandler(_customEventsHandler = new EventsHandler());
        
        
        string[] soundNames = ["Start.ogg", "Loop.ogg", "End.ogg", "Beeps.ogg"];
        string basePath = "MicroHIDReturn.Sounds.";

        Assembly assembly = Assembly.GetExecutingAssembly();
        foreach (string value in soundNames) {
            Stream stream = assembly.GetManifestResourceStream(basePath + value);
            if (stream == null){
                Logger.Error($"Sound resource \"{value}\" not found in assembly \"{assembly.FullName}\"");
                continue;
            }

            string tempPath = Path.Combine(Path.GetTempPath(), value);
            using (FileStream fileStream = File.Create(tempPath)) {
                stream.CopyTo(fileStream);
            }

            string clipName = Path.GetFileNameWithoutExtension(value);
            AudioClipStorage.LoadClip(tempPath, clipName);
        }
    }
    
    public override void Disable() {
        CustomHandlersManager.UnregisterEventsHandler(_customEventsHandler);
        _customEventsHandler = null!;
        
        Singleton = null!;
    }
}