using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using NesSharp;
using NesSharpUI.Controls;
using NesSharpUI.ViewModels;
using Console = NesSharp.Console;

namespace NesSharpUI.Views;

public partial class MainView : UserControl
{
    private SimpleImageViewer _frameViewer;

    private MainViewModel _model = new();

    private PixelSize _pixelSize = new PixelSize(256, 240);

    private Console _console;

    private Thread _nesThread;

    private static readonly Dictionary<byte, Color> ColorDict;

    static MainView()
    {
        ColorDict = new Dictionary<byte, Color>()
        {
            { 0x0, Color.FromArgb(84, 84, 84) },
            { 0x1, Color.FromArgb(0, 30, 116) },
            { 0x2, Color.FromArgb(8, 16, 144) },
            { 0x3, Color.FromArgb(48, 0, 136) },
            { 0x4, Color.FromArgb(68, 0, 100) },
            { 0x5, Color.FromArgb(92, 0, 48) },
            { 0x6, Color.FromArgb(84, 4, 0) },
            { 0x7, Color.FromArgb(60, 24, 0) },
            { 0x8, Color.FromArgb(32, 42, 0) },
            { 0x9, Color.FromArgb(8, 58, 0) },
            { 0xa, Color.FromArgb(0, 64, 0) },
            { 0xb, Color.FromArgb(0, 60, 0) },
            { 0xc, Color.FromArgb(0, 50, 60) },
            { 0xd, Color.FromArgb(0, 0, 0) },
            { 0xe, Color.FromArgb(0, 0, 0) },
            { 0xf, Color.FromArgb(0, 0, 0) },
            { 0x10, Color.FromArgb(152, 150, 152) },
            { 0x11, Color.FromArgb(8, 76, 196) },
            { 0x12, Color.FromArgb(48, 50, 236) },
            { 0x13, Color.FromArgb(92, 30, 228) },
            { 0x14, Color.FromArgb(136, 20, 176) },
            { 0x15, Color.FromArgb(160, 20, 100) },
            { 0x16, Color.FromArgb(152, 34, 32) },
            { 0x17, Color.FromArgb(120, 60, 0) },
            { 0x18, Color.FromArgb(84, 90, 0) },
            { 0x19, Color.FromArgb(40, 114, 0) },
            { 0x1a, Color.FromArgb(8, 124, 0) },
            { 0x1b, Color.FromArgb(0, 118, 40) },
            { 0x1c, Color.FromArgb(0, 102, 120) },
            { 0x1d, Color.FromArgb(0, 0, 0) },
            { 0x1e, Color.FromArgb(0, 0, 0) },
            { 0x1f, Color.FromArgb(0, 0, 0) },
            { 0x20, Color.FromArgb(236, 238, 236) },
            { 0x21, Color.FromArgb(76, 154, 236) },
            { 0x22, Color.FromArgb(120, 124, 236) },
            { 0x23, Color.FromArgb(176, 98, 236) },
            { 0x24, Color.FromArgb(228, 84, 236) },
            { 0x25, Color.FromArgb(236, 88, 180) },
            { 0x26, Color.FromArgb(236, 106, 100) },
            { 0x27, Color.FromArgb(212, 136, 32) },
            { 0x28, Color.FromArgb(160, 170, 0) },
            { 0x29, Color.FromArgb(116, 196, 0) },
            { 0x2a, Color.FromArgb(76, 208, 32) },
            { 0x2b, Color.FromArgb(56, 204, 108) },
            { 0x2c, Color.FromArgb(56, 180, 204) },
            { 0x2d, Color.FromArgb(60, 60, 60) },
            { 0x2e, Color.FromArgb(0, 0, 0) },
            { 0x2f, Color.FromArgb(0, 0, 0) },
            { 0x30, Color.FromArgb(236, 238, 236) },
            { 0x31, Color.FromArgb(168, 204, 236) },
            { 0x32, Color.FromArgb(188, 188, 236) },
            { 0x33, Color.FromArgb(212, 178, 236) },
            { 0x34, Color.FromArgb(236, 174, 236) },
            { 0x35, Color.FromArgb(236, 174, 212) },
            { 0x36, Color.FromArgb(236, 180, 176) },
            { 0x37, Color.FromArgb(228, 196, 144) },
            { 0x38, Color.FromArgb(204, 210, 120) },
            { 0x39, Color.FromArgb(180, 222, 120) },
            { 0x3a, Color.FromArgb(168, 226, 144) },
            { 0x3b, Color.FromArgb(152, 226, 180) },
            { 0x3c, Color.FromArgb(160, 214, 228) },
            { 0x3d, Color.FromArgb(160, 162, 160) },
            { 0x3e, Color.FromArgb(0, 0, 0) },
            { 0x3f, Color.FromArgb(0, 0, 0) },
        };
    }

    public MainView()
    {
        InitializeComponent();
        _frameViewer = this.GetControl<SimpleImageViewer>("Frame");
        _console = new Console();
        _console.DrawAction = Draw;
        this.KeyDown += HandleKeyDown;
        this.KeyUp += HandleKeyUp;
    }
    
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if(DataContext is MainViewModel model) {
            _model = model;
        }
    }

    public async void OpenCommand(object sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        List<FilePickerFileType> filter = new List<FilePickerFileType>();
        filter.Add(new FilePickerFileType("Rom files") { Patterns = new List<string>() { "*.nes" } });
        // Start async operation to open the dialog.
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Rom File",
            AllowMultiple = false,
            FileTypeFilter = filter
        });

        if (files.Count > 0)
        {
            Debug.Print($"OpenCommand ->>>>>> {files[0].Path.LocalPath}");
            _console.LoadCartridge(files[0].Path.LocalPath);
            StartConsole();
        }
    }

    private unsafe void UpdateSurface(byte[] frame, WriteableBitmap? surface)
    {
        if (surface == null)
        {
            return;
        }

        var size = _pixelSize.Width * _pixelSize.Height;
        using (var bitmapLock = surface.Lock())
        {
            var ptr = (int*)bitmapLock.Address;

            for (var x = 0; x < bitmapLock.Size.Width; x++)
            for (var y = 0; y < bitmapLock.Size.Height; y++)
            {
                ptr[y * bitmapLock.RowBytes / 4 + x] = ColorDict[frame[y * bitmapLock.Size.Width + x]].ToArgb();
            }
        }

        Dispatcher.UIThread.Post(() => { _frameViewer.InvalidateVisual(); }, DispatcherPriority.MaxValue);
    }

    void StartConsole()
    {
        _nesThread = new Thread(new ThreadStart(StartNes));
        _nesThread.IsBackground = true;
        _nesThread.Start();
    }

    void StartNes()
    {
        _console.Start();
    }

    private void Draw(byte[] screen)
    {
        if (_model.FrameSurface == null)
        {
            _model.FrameSurface =
                new WriteableBitmap(_pixelSize, new Vector(96, 96), PixelFormat.Bgra8888);
        }
        UpdateSurface(screen, _model.FrameSurface);
    }

    void SetControllerButton(bool state, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Z:
                _console.Controller.setButtonState(Controller.Button.A, state);
                break;
            case Key.X:
                _console.Controller.setButtonState(Controller.Button.B, state);
                break;
            case Key.Left:
                _console.Controller.setButtonState(Controller.Button.Left, state);
                break;
            case Key.Right:
                _console.Controller.setButtonState(Controller.Button.Right, state);
                break;
            case Key.Up:
                _console.Controller.setButtonState(Controller.Button.Up, state);
                break;
            case Key.Down:
                _console.Controller.setButtonState(Controller.Button.Down, state);
                break;
            case Key.Q:
                _console.Controller.setButtonState(Controller.Button.Start, state);
                break;
            case Key.W:
                _console.Controller.setButtonState(Controller.Button.Select, state);
                break;
        }
    }

    protected void HandleKeyDown(object? sender, KeyEventArgs e)
    {
        SetControllerButton(true, e);
    }

    protected void HandleKeyUp(object? sender, KeyEventArgs e)
    {
        SetControllerButton(false, e);
    }
}