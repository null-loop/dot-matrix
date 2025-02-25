using System.Runtime.InteropServices;

namespace dotmatrix.lib.Matrix;

/// <summary>
/// Represents an RGB (red, green, blue) color
/// </summary>
public struct Color
{
    /// <summary>
    /// The red component value of this instance.
    /// </summary>
    public byte R;

    /// <summary>
    /// The green component value of this instance.
    /// </summary>
    public byte G;

    /// <summary>
    /// The blue component value of this instance.
    /// </summary>
    public byte B;

    /// <summary>
    /// Creates a new color from the specified color values (red, green, and blue).
    /// </summary>
    /// <param name="r">The red component value.</param>
    /// <param name="g">The green component value.</param>
    /// <param name="b">The blue component value.</param>
    public Color(int r, int g, int b) : this((byte)r, (byte)g, (byte)b) { }

    /// <summary>
    /// Creates a new color from the specified color values (red, green, and blue).
    /// </summary>
    /// <param name="r">The red component value.</param>
    /// <param name="g">The green component value.</param>
    /// <param name="b">The blue component value.</param>
    public Color(byte r, byte g, byte b) => (R, G, B) = (r, g, b);
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
internal struct InternalRGBLedMatrixOptions
{
    public IntPtr hardware_mapping;
    public int rows;
    public int cols;
    public int chain_length;
    public int parallel;
    public int pwm_bits;
    public int pwm_lsb_nanoseconds;
    public int pwm_dither_bits;
    public int brightness;
    public int scan_mode;
    public int row_address_type;
    public int multiplexing;
    public IntPtr led_rgb_sequence;
    public IntPtr pixel_mapper_config;
    public IntPtr panel_type;
    public byte disable_hardware_pulsing;
    public byte show_refresh_rate;
    public byte inverse_colors;
    public int limit_refresh_rate_hz;

    public InternalRGBLedMatrixOptions(RGBLedMatrixOptions opt)
    {
        chain_length = opt.ChainLength;
        rows = opt.Rows;
        cols = opt.Cols;
        hardware_mapping = Marshal.StringToHGlobalAnsi(opt.HardwareMapping);
        inverse_colors = (byte)(opt.InverseColors ? 1 : 0);
        led_rgb_sequence = Marshal.StringToHGlobalAnsi(opt.LedRgbSequence);
        pixel_mapper_config = Marshal.StringToHGlobalAnsi(opt.PixelMapperConfig);
        panel_type = Marshal.StringToHGlobalAnsi(opt.PanelType);
        parallel = opt.Parallel;
        multiplexing = (int)opt.Multiplexing;
        pwm_bits = opt.PwmBits;
        pwm_lsb_nanoseconds = opt.PwmLsbNanoseconds;
        pwm_dither_bits = opt.PwmDitherBits;
        scan_mode = (int)opt.ScanMode;
        show_refresh_rate = (byte)(opt.ShowRefreshRate ? 1 : 0);
        limit_refresh_rate_hz = opt.LimitRefreshRateHz;
        brightness = opt.Brightness;
        disable_hardware_pulsing = (byte)(opt.DisableHardwarePulsing ? 1 : 0);
        row_address_type = opt.RowAddressType;
    }
}

/*
Some of the extern methods listed below are marked with [SuppressGCTransition].
This disables some GC checks that may take a long time. But such methods should
be fast and trivial, otherwise the managed code may become unstable (see docs).
Keep this in mind when changing the C/C++ side.

https://learn.microsoft.com/dotnet/api/system.runtime.interopservices.suppressgctransitionattribute
*/
internal static class Bindings
{
    private const string Lib = "librgbmatrix.so.1";

    [DllImport(Lib)]
    public static extern IntPtr led_matrix_create(int rows, int chained, int parallel);

    [DllImport(Lib, CharSet = CharSet.Ansi)]
    public static extern IntPtr led_matrix_create_from_options_const_argv(
        ref InternalRGBLedMatrixOptions options,
        int argc,
        string[] argv);

    [DllImport(Lib)]
    public static extern void led_matrix_delete(IntPtr matrix);

    [DllImport(Lib)]
    public static extern IntPtr led_matrix_create_offscreen_canvas(IntPtr matrix);

    [DllImport(Lib)]
    public static extern IntPtr led_matrix_swap_on_vsync(IntPtr matrix, IntPtr canvas);

    [DllImport(Lib)]
    public static extern IntPtr led_matrix_get_canvas(IntPtr matrix);

    [DllImport(Lib)]
    [SuppressGCTransition]
    public static extern byte led_matrix_get_brightness(IntPtr matrix);

    [DllImport(Lib)]
    [SuppressGCTransition]
    public static extern void led_matrix_set_brightness(IntPtr matrix, byte brightness);

    [DllImport(Lib, CharSet = CharSet.Ansi)]
    public static extern IntPtr load_font(string bdf_font_file);

    [DllImport(Lib, CharSet = CharSet.Ansi)]
    public static extern int draw_text(IntPtr canvas, IntPtr font, int x, int y, byte r, byte g, byte b,
                                       string utf8_text, int extra_spacing);

    [DllImport(Lib, CharSet = CharSet.Ansi)]
    public static extern int vertical_draw_text(IntPtr canvas, IntPtr font, int x, int y, byte r, byte g, byte b,
                                                string utf8_text, int kerning_offset);

    [DllImport(Lib, CharSet = CharSet.Ansi)]
    public static extern void delete_font(IntPtr font);

    [DllImport(Lib)]
    [SuppressGCTransition]
    public static extern void led_canvas_get_size(IntPtr canvas, out int width, out int height);

    [DllImport(Lib)]
    [SuppressGCTransition]
    public static extern void led_canvas_set_pixel(IntPtr canvas, int x, int y, byte r, byte g, byte b);

    [DllImport(Lib)]
    public static extern void led_canvas_set_pixels(IntPtr canvas, int x, int y, int width, int height,
                                                    ref Color colors);

    [DllImport(Lib)]
    public static extern void led_canvas_clear(IntPtr canvas);

    [DllImport(Lib)]
    public static extern void led_canvas_fill(IntPtr canvas, byte r, byte g, byte b);

    [DllImport(Lib)]
    public static extern void draw_circle(IntPtr canvas, int xx, int y, int radius, byte r, byte g, byte b);

    [DllImport(Lib)]
    public static extern void draw_line(IntPtr canvas, int x0, int y0, int x1, int y1, byte r, byte g, byte b);
}

/// <summary>
/// Type of multiplexing.
/// </summary>
public enum Multiplexing : int
{
    Direct = 0,
    Stripe = 1,
    Checker = 2
}

/// <summary>
/// Represents a canvas whose pixels can be manipulated.
/// </summary>
public class RGBLedCanvas
{
    // This is a wrapper for canvas no need to implement IDisposable here 
    // because RGBLedMatrix has ownership and takes care of disposing canvases
    internal IntPtr _canvas;

    // this is not called directly by the consumer code,
    // consumer uses factory methods in RGBLedMatrix
    internal RGBLedCanvas(IntPtr canvas)
    {
        _canvas = canvas;
        Bindings.led_canvas_get_size(_canvas, out var width, out var height);
        Width = width;
        Height = height;
    }

    /// <summary>
    /// The width of the canvas in pixels.
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// The height of the canvas in pixels.
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// Sets the color of a specific pixel.
    /// </summary>
    /// <param name="x">The X coordinate of the pixel.</param>
    /// <param name="y">The Y coordinate of the pixel.</param>
    /// <param name="color">New pixel color.</param>
    public void SetPixel(int x, int y, Color color) => Bindings.led_canvas_set_pixel(_canvas, x, y, color.R, color.G, color.B);

    /// <summary>
    /// Copies the colors from the specified buffer to a rectangle on the canvas.
    /// </summary>
    /// <param name="x">The X coordinate of the top-left pixel of the rectangle.</param>
    /// <param name="y">The Y coordinate of the top-left pixel of the rectangle.</param>
    /// <param name="width">Width of the rectangle.</param>
    /// <param name="height">Height of the rectangle.</param>
    /// <param name="colors">Buffer containing the colors to copy.</param>
    public void SetPixels(int x, int y, int width, int height, Span<Color> colors)
    {
        if (colors.Length < width * height)
            throw new ArgumentOutOfRangeException(nameof(colors));
        Bindings.led_canvas_set_pixels(_canvas, x, y, width, height, ref colors[0]);
    }

    /// <summary>
    /// Sets the color of the entire canvas.
    /// </summary>
    /// <param name="color">New canvas color.</param>
    public void Fill(Color color) => Bindings.led_canvas_fill(_canvas, color.R, color.G, color.B);

    /// <summary>
    /// Cleans the entire canvas.
    /// </summary>
    public void Clear() => Bindings.led_canvas_clear(_canvas);

    /// <summary>
    /// Draws a circle of the specified color.
    /// </summary>
    /// <param name="x">The X coordinate of the center.</param>
    /// <param name="y">The Y coordinate of the center.</param>
    /// <param name="radius">The radius of the circle, in pixels.</param>
    /// <param name="color">The color of the circle.</param>
    public void DrawCircle(int x, int y, int radius, Color color) =>
        Bindings.draw_circle(_canvas, x, y, radius, color.R, color.G, color.B);

    /// <summary>
    /// Draws a line of the specified color.
    /// </summary>
    /// <param name="x0">The X coordinate of the first point.</param>
    /// <param name="y0">The Y coordinate of the first point.</param>
    /// <param name="x1">The X coordinate of the second point.</param>
    /// <param name="y1">The Y coordinate of the second point.</param>
    /// <param name="color">The color of the line.</param>
    public void DrawLine(int x0, int y0, int x1, int y1, Color color) =>
        Bindings.draw_line(_canvas, x0, y0, x1, y1, color.R, color.G, color.B);

    /// <summary>
    /// Draws the text with the specified color.
    /// </summary>
    /// <param name="font">Font to draw text with.</param>
    /// <param name="x">The X coordinate of the starting point.</param>
    /// <param name="y">The Y coordinate of the starting point.</param>
    /// <param name="color">The color of the text.</param>
    /// <param name="text">Text to draw.</param>
    /// <param name="spacing">Additional spacing between characters.</param>
    /// <param name="vertical">Whether to draw the text vertically.</param>
    /// <returns>How many pixels was advanced on the screen.</returns>
    public int DrawText(RGBLedFont font, int x, int y, Color color, string text, int spacing = 0, bool vertical = false) =>
        font.DrawText(_canvas, x, y, color, text, spacing, vertical);
}

/// <summary>
/// Represents a <c>.BDF</c> font.
/// </summary>
public class RGBLedFont : IDisposable
{
    internal IntPtr _font;
    private bool disposedValue = false;

    /// <summary>
    /// Loads the BDF font from the specified file.
    /// </summary>
    /// <param name="bdfFontPath">The path to the BDF file to load.</param>
    public RGBLedFont(string bdfFontPath) => _font = Bindings.load_font(bdfFontPath);

    internal int DrawText(IntPtr canvas, int x, int y, Color color, string text, int spacing = 0, bool vertical = false)
    {
        if (!vertical)
            return Bindings.draw_text(canvas, _font, x, y, color.R, color.G, color.B, text, spacing);
        else
            return Bindings.vertical_draw_text(canvas, _font, x, y, color.R, color.G, color.B, text, spacing);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposedValue) return;
        Bindings.delete_font(_font);
        disposedValue = true;
    }

    ~RGBLedFont() => Dispose(false);

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Represents a RGB matrix.
/// </summary>
public class RGBLedMatrix : IDisposable
{
    private IntPtr matrix;
    private bool disposedValue = false;

    /// <summary>
    /// Initializes a new matrix.
    /// </summary>
    /// <param name="rows">Size of a single module. Can be 32, 16 or 8.</param>
    /// <param name="chained">How many modules are connected in a chain.</param>
    /// <param name="parallel">How many modules are connected in a parallel.</param>
    public RGBLedMatrix(int rows, int chained, int parallel)
    {
        matrix = Bindings.led_matrix_create(rows, chained, parallel);
        if (matrix == (IntPtr)0)
            throw new ArgumentException("Could not initialize a new matrix");
    }

    /// <summary>
    /// Initializes a new matrix.
    /// </summary>
    /// <param name="options">A configuration of a matrix.</param>
    public RGBLedMatrix(RGBLedMatrixOptions options)
    {
        InternalRGBLedMatrixOptions opt = default;
        try
        {
            opt = new(options);
            var args = Environment.GetCommandLineArgs();

            // Because gpio-slowdown is not provided in the options struct,
            // we manually add it.
            // Let's add it first to the command-line we pass to the
            // matrix constructor, so that it can be overridden with the
            // users' commandline.
            // As always, as the _very_ first, we need to provide the
            // program name argv[0].
            var argv = new string[args.Length + 1];
            argv[0] = args[0];
            argv[1] = $"--led-slowdown-gpio={options.GpioSlowdown}";
            Array.Copy(args, 1, argv, 2, args.Length - 1);

            matrix = Bindings.led_matrix_create_from_options_const_argv(ref opt, argv.Length, argv);
            if (matrix == (IntPtr)0)
                throw new ArgumentException("Could not initialize a new matrix");
        }
        finally
        {
            if(options.HardwareMapping is not null) Marshal.FreeHGlobal(opt.hardware_mapping);
            if(options.LedRgbSequence is not null) Marshal.FreeHGlobal(opt.led_rgb_sequence);
            if(options.PixelMapperConfig is not null) Marshal.FreeHGlobal(opt.pixel_mapper_config);
            if(options.PanelType is not null) Marshal.FreeHGlobal(opt.panel_type);
        }
    }

    /// <summary>
    /// Creates a new backbuffer canvas for drawing on.
    /// </summary>
    /// <returns>An instance of <see cref="RGBLedCanvas"/> representing the canvas.</returns>
    public RGBLedCanvas CreateOffscreenCanvas() => new(Bindings.led_matrix_create_offscreen_canvas(matrix));

    /// <summary>
    /// Returns a canvas representing the current frame buffer.
    /// </summary>
    /// <returns>An instance of <see cref="RGBLedCanvas"/> representing the canvas.</returns>
    /// <remarks>Consider using <see cref="CreateOffscreenCanvas"/> instead.</remarks>
    public RGBLedCanvas GetCanvas() => new(Bindings.led_matrix_get_canvas(matrix));

    /// <summary>
    /// Swaps this canvas with the currently active canvas. The active canvas
    /// becomes a backbuffer and is mapped to <paramref name="canvas"/> instance.
    /// <br/>
    /// This operation guarantees vertical synchronization.
    /// </summary>
    /// <param name="canvas">Backbuffer canvas to swap.</param>
    public void SwapOnVsync(RGBLedCanvas canvas) =>
        canvas._canvas = Bindings.led_matrix_swap_on_vsync(matrix, canvas._canvas);

    /// <summary>
    /// The general brightness of the matrix.
    /// </summary>
    public byte Brightness
    {
        get => Bindings.led_matrix_get_brightness(matrix);
        set => Bindings.led_matrix_set_brightness(matrix, value);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposedValue) return;

        Bindings.led_matrix_delete(matrix);
        disposedValue = true;
    }

    ~RGBLedMatrix() => Dispose(false);

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

public struct RGBLedMatrixOptions
{
    /// <summary>
    /// Name of the hardware mapping used. If passed
    /// <see langword="null"/> here, the default is used.
    /// </summary>
    public string? HardwareMapping = null;

    /// <summary>
    /// The "rows" are the number of rows supported by the display, so 32 or 16.
    /// Default: 32.
    /// </summary>
    public int Rows = 32;

    /// <summary>
    /// The "cols" are the number of columns per panel. Typically something
    /// like 32, but also 64 is possible. Sometimes even 40.
    /// <c>cols * chain_length</c> is the total length of the display, so you can
    /// represent a 64 wide display as cols=32, chain=2 or cols=64, chain=1;
    /// same thing, but more convenient to think of.
    /// </summary>
    public int Cols = 32;

    /// <summary>
    /// The chain_length is the number of displays daisy-chained together
    /// (output of one connected to input of next). Default: 1
    /// </summary>
    public int ChainLength = 1;

    /// <summary>
    /// The number of parallel chains connected to the Pi; in old Pis with 26
    /// GPIO pins, that is 1, in newer Pis with 40 interfaces pins, that can also
    /// be 2 or 3. The effective number of pixels in vertical direction is then
    /// thus <c>rows * parallel</c>. Default: 1
    /// </summary>
    public int Parallel = 1;

    /// <summary>
    /// Set PWM bits used for output. Default is 11, but if you only deal with limited
    /// comic-colors, 1 might be sufficient. Lower require less CPU and increases refresh-rate.
    /// </summary>
    public int PwmBits = 11;

    /// <summary>
    /// Change the base time-unit for the on-time in the lowest significant bit in
    /// nanoseconds. Higher numbers provide better quality (more accurate color, less
    /// ghosting), but have a negative impact on the frame rate.
    /// </summary>
    public int PwmLsbNanoseconds = 130;

    /// <summary>
    /// The lower bits can be time-dithered for higher refresh rate.
    /// </summary>
    public int PwmDitherBits = 0;

    /// <summary>
    /// The initial brightness of the panel in percent. Valid range is 1..100
    /// </summary>
    public int Brightness = 100;

    /// <summary>
    /// Scan mode.
    /// </summary>
    public ScanModes ScanMode = ScanModes.Progressive;

    /// <summary>
    /// Default row address type is 0, corresponding to direct setting of the
    /// row, while row address type 1 is used for panels that only have A/B,
    /// typically some 64x64 panels
    /// </summary>
    public int RowAddressType = 0;

    /// <summary>
    /// Type of multiplexing.
    /// </summary>
    public Multiplexing Multiplexing = Multiplexing.Direct;

    /// <summary>
    /// In case the internal sequence of mapping is not <c>"RGB"</c>, this
    /// contains the real mapping. Some panels mix up these colors.
    /// </summary>
    public string? LedRgbSequence = null;

    /// <summary>
    /// A string describing a sequence of pixel mappers that should be applied
    /// to this matrix. A semicolon-separated list of pixel-mappers with optional
    /// parameter.
    public string? PixelMapperConfig = null;

    /// <summary>
    /// Panel type. Typically just empty, but certain panels (FM6126)
    /// requie an initialization sequence
    /// </summary>
    public string? PanelType = null;

    /// <summary>
    /// Allow to use the hardware subsystem to create pulses. This won't do
    /// anything if output enable is not connected to GPIO 18.
    /// </summary>
    public bool DisableHardwarePulsing = false;
    public bool ShowRefreshRate = false;
    public bool InverseColors = false;

    /// <summary>
    /// Limit refresh rate of LED panel. This will help on a loaded system
    /// to keep a constant refresh rate. &lt;= 0 for no limit.
    /// </summary>
    public int LimitRefreshRateHz = 0;

    /// <summary>
    /// Slowdown GPIO. Needed for faster Pis/slower panels.
    /// </summary>
    public int GpioSlowdown = 1;

    /// <summary>
    /// Creates default matrix settings.
    /// </summary>
    public RGBLedMatrixOptions() { }
}

/// <summary>
/// Scan modes.
/// </summary>
public enum ScanModes
{
    Progressive = 0,
    Interlaced = 1
}