using Avalonia.Media.Imaging;
using ReactiveUI.Fody.Helpers;

namespace NesSharpUI.ViewModels;

public class MainViewModel : ViewModelBase
{
    [Reactive] public WriteableBitmap? FrameSurface { get; set; }
    
}