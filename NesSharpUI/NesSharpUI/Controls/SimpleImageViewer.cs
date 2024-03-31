using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace NesSharpUI.Controls;

public class SimpleImageViewer : Control
{
    public static readonly StyledProperty<IImage> SourceProperty = AvaloniaProperty.Register<SimpleImageViewer, IImage>(nameof(Source));

    public IImage Source
    {
        get { return GetValue(SourceProperty); }
        set { SetValue(SourceProperty, value); }
    }

    static SimpleImageViewer()
    {
        AffectsRender<SimpleImageViewer>(SourceProperty);
    }

    public SimpleImageViewer()
    {
    }

    public override void Render(DrawingContext context)
    {
        if(Source == null) {
            return;
        }

        context.DrawImage(
            Source,
            new Rect(0, 0, (int)Source.Size.Width, (int)Source.Size.Height),
            new Rect(0, 0, Bounds.Width, Bounds.Height)
        );
    }
}