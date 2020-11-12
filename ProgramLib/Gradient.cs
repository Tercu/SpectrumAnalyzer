using System.Drawing;
using System.Drawing.Drawing2D;

namespace SpectrumAnalyser
{
    public class Gradient
    {
        private Bitmap map = new Bitmap(1, 257);

        public Gradient()
        {
            // Put the points of a polygon in an array.
            int height = map.Height / 3;
            Point[] points = {
       new Point(0, 0*height),
       new Point(0, (int)(2*height)),
       new Point(0, (int)(2.7*height)),
       new Point(0, 3*height),
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
       Color.Yellow,
       Color.Orange,
       Color.Red,
       Color.DarkViolet,
       Color.Black,
            };

            pthGrBrush.SurroundColors = colors;
            Graphics glue = Graphics.FromImage(map);
            glue.FillPath(pthGrBrush, path);
        }
        public Color GetGradientColor(int position)
        {
            return map.GetPixel(0, position + 1);
        }
    }
}
