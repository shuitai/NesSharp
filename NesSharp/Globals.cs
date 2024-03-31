global using Address = ushort;

namespace NesSharp;

public class Constants
{
    // Reset vector address, once CPU reset, CPU will run instruction from the address
    public static readonly ushort ResetVector = 0xfffc;
    
    public static readonly uint ScanlineVisibleDots = 256;
    public static readonly uint VisibleScanlines = 240;

    public static uint NESVideoWidth = ScanlineVisibleDots;
    public static uint NESVideoHeight = VisibleScanlines;
}

