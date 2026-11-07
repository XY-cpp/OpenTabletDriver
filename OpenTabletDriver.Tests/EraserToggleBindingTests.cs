using NSubstitute;
using OpenTabletDriver.Desktop.Binding;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;
using Xunit;

namespace OpenTabletDriver.Tests
{
    public class EraserToggleBindingTests
    {
        [Fact]
        public void HoldingAndReleasingButtonOnlySwitchesOnce()
        {
            var pointer = Substitute.For<IEraserToggleHandler>();
            var state = new BindingState { Binding = new EraserToggleBinding { Pointer = pointer } };
            var report = Substitute.For<IDeviceReport>();

            for (int i = 0; i < 100; i++)
                state.Invoke(null!, report, true);
            state.Invoke(null!, report, false);
            state.Invoke(null!, report, false);
            pointer.Received(1).ToggleEraser();

            state.Invoke(null!, report, true);
            state.Invoke(null!, report, false);
            pointer.Received(2).ToggleEraser();
        }

        [Fact]
        public void BothButtonsUseTheSamePointerToolState()
        {
            var pointer = Substitute.For<IEraserToggleHandler>();
            var first = new BindingState { Binding = new EraserToggleBinding { Pointer = pointer } };
            var second = new BindingState { Binding = new EraserToggleBinding { Pointer = pointer } };
            var report = Substitute.For<IDeviceReport>();

            first.Invoke(null!, report, true);
            first.Invoke(null!, report, false);
            second.Invoke(null!, report, true);
            second.Invoke(null!, report, false);
            pointer.Received(2).ToggleEraser();
        }
    }
}
