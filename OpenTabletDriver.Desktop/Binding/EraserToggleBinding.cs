using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    [PluginName("Pen / Eraser Toggle"), SupportedPlatform(PluginPlatform.MacOS)]
    public class EraserToggleBinding : IStateBinding
    {
        [Resolved]
        public IEraserToggleHandler? Pointer { get; set; }

        [OnDependencyLoad]
        public void VerifyInitialization()
        {
            if (Pointer == null)
                Log.Write(nameof(EraserToggleBinding), "The output mode does not support native eraser switching", LogLevel.Error);
        }

        public void Press(TabletReference tablet, IDeviceReport report) => Pointer?.ToggleEraser();

        // Keep the selected tool after releasing the side button or lifting the pen.
        public void Release(TabletReference tablet, IDeviceReport report) { }

        public override string ToString() => "Pen / Eraser Toggle";
    }
}
