using Avalonia.Controls;
using Avalonia.Input;
using NesSharp;

namespace NesSharpUI.Views;

public partial class MainWindow : Window
{
    private readonly float _screenScale = 2.0f;
    public MainWindow()
    {
        InitializeComponent();
        Width = Constants.NESVideoWidth * _screenScale;
        Height = Constants.NESVideoHeight * _screenScale;
    }
}