using HoyoVERSE.ViewModels;

namespace HoyoVERSE.Models
{
    public enum GraphicsApi { Auto, DX11, DX12, Vulkan, OpenGL }
    public enum WindowMode { Default, ExclusiveFullscreen, Borderless, Windowed }
    public enum VsyncMode { Default, Off, On }

    public class GameLaunchSettings : ObservableObject
    {
        GraphicsApi _graphicsApi = GraphicsApi.Auto;
        public GraphicsApi GraphicsApi { get => _graphicsApi; set => Set(ref _graphicsApi, value); }

        WindowMode _windowMode = WindowMode.Default;
        public WindowMode WindowMode { get => _windowMode; set => Set(ref _windowMode, value); }

        int _width;
        public int Width { get => _width; set => Set(ref _width, value); }

        int _height;
        public int Height { get => _height; set => Set(ref _height, value); }

        VsyncMode _vsync = VsyncMode.Default;
        public VsyncMode VSync { get => _vsync; set => Set(ref _vsync, value); }

        // 0 = unset; otherwise Star Rail FPS unlock value (30..120)
        int _targetFps;
        public int TargetFps { get => _targetFps; set => Set(ref _targetFps, value); }

        string _customArgs = string.Empty;
        public string CustomArgs { get => _customArgs; set => Set(ref _customArgs, value ?? string.Empty); }
    }
}
