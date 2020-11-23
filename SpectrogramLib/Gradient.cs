using System.Drawing;
using System.Drawing.Drawing2D;

namespace Spectrogram
{
    public class Gradient
    {
        public Bitmap ColorMap { get; init; }

        public Gradient()
        {
            ColorMap = new Bitmap(1, 1024);
            // Put the points of a polygon in an array.
            int height = ColorMap.Height / 5;
            Point[] points = {
                new Point(0, -1),
                new Point(0, (int)(0.7* height)),
                new Point(0, (int)(1.6 * height)),
                new Point(0, (int)(2.4 * height)),
                new Point(0, (int)(2.9 * height)),
                new Point(0, (int)(3.0 * height)),
                new Point(0, (int)(5.0 * height)),
                new Point(1, 30),
            };

            // Use the array of points to construct a path.
            GraphicsPath path = new GraphicsPath();
            path.AddLines(points);

            // Use the path to construct a path gradient brush.
            PathGradientBrush pthGrBrush = new PathGradientBrush(path);

            // Set the color at the center of the path to red.
            pthGrBrush.CenterColor = Color.FromArgb(255, 255, 0, 0);

            // Set the colors of the points in the array.
            Color[] colors = {
                Color.White,
                Color.Yellow,
                Color.Red,
                Color.DarkViolet,
                Color.DarkBlue,
                Color.Black,
                Color.Black,
            };

            pthGrBrush.SurroundColors = colors;
            Graphics glue = Graphics.FromImage(ColorMap);
            glue.FillPath(pthGrBrush, path);

            path.Dispose();
            pthGrBrush.Dispose();
            glue.Dispose();
            //map.Save(@"D:\dev\test\gradient.png");
        }
        public Color GetGradientColor(int position)
        {
            return ColorMap.GetPixel(0, position);
        }
    }
}
