using dotmatrix.lib.Matrix;

using var matrix = new RGBLedMatrix(new RGBLedMatrixOptions
{
    Rows = 64, 
    Cols = 64, 
    ChainLength = 4,
    Parallel = 1,
    HardwareMapping = "adafruit-hat",
    GpioSlowdown = 2,
    LimitRefreshRateHz = 120,
    Brightness = 50,
    DisableHardwarePulsing = true
});
var canvas = matrix.CreateOffscreenCanvas();

var maxBrightness = matrix.Brightness;
var rnd = new Random();
var color = new Color(rnd.Next(0, 256), rnd.Next(0, 256), rnd.Next(0, 256));
while (true)
{
    if (matrix.Brightness < 1)
    {
        matrix.Brightness = maxBrightness;
        color = new Color(rnd.Next(0, 256), rnd.Next(0, 256), rnd.Next(0, 256));
    }
    else
    {
        matrix.Brightness--;
    }

    canvas.Fill(color);
    matrix.SwapOnVsync(canvas);
    Thread.Sleep(20);
}