using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;

namespace Spectrogram
{
    public class BitmapGenerator : IBitmapGenerator
    {
        public string PathToFile { get; private set; }
        public Bitmap Image { get; private set; }
        public Boolean IsSingleChannel { get; init; }
        private int currentRow = 0;
        private readonly Gradient gradient = new Gradient();

        public BitmapGenerator(string path, int width, int height, Boolean isSingleChannel = false)
        {
            PathToFile = path;
            IsSingleChannel = isSingleChannel;
            Image = new Bitmap(width, height);
        }

        public void EditRow(int row, Histogram histogram)
        {
            int index = Image.Height - 1;
            foreach (var h in histogram.Data)
            {
                Color color = gradient.GetGradientColor((int)h.Value);
                Image.SetPixel(row, index, color);
                --index;
            }
            ++currentRow;
        }

        public void SaveImage()
        {
            RemoveEmptyRows();
            char separator = Path.DirectorySeparatorChar;
            string name = @$"{ Path.GetDirectoryName(PathToFile)}{separator}{ Path.GetFileNameWithoutExtension(PathToFile) }.bmp";
            if (IsSingleChannel)
            {
                SaveSingleChannel(name);
            }
            else
            {
                SaveMultiChannel(name);
            }
        }

        private void SaveMultiChannel(string name)
        {
            Rectangle channel1rect = new Rectangle(0, 0, Image.Width, Image.Height / 2);
            Bitmap channel1 = Image.Clone(channel1rect, Image.PixelFormat);
            channel1.RotateFlip(RotateFlipType.RotateNoneFlipY);

            Rectangle channel2rect = new Rectangle(0, Image.Height / 2, Image.Width, Image.Height / 2);
            Bitmap channel2 = Image.Clone(channel2rect, Image.PixelFormat);


            int outline = 40;
            Bitmap outputImage = new Bitmap(Image.Width + 100, Image.Height + 100);
            Rectangle channel1Position = new Rectangle(0, 0, Image.Width + outline, (Image.Height / 2) + outline);
            Rectangle channel2Position = new Rectangle(0, (Image.Height / 2) + outline, Image.Width + outline, (Image.Height / 2) + outline);

            using (Graphics graphics = Graphics.FromImage(outputImage))
            {
                graphics.FillRectangle(Brushes.Black, new Rectangle(0, 0, outputImage.Width, outputImage.Height));

                graphics.DrawImage(DrawScales(channel1, channel1Position, outline / 2), channel1Position);
                graphics.DrawImage(DrawScales(channel2, channel2Position, outline / 2), channel2Position);

                Rectangle gradientRect = new Rectangle(outputImage.Width - 50, outputImage.Height * 1 / 5, 40, 680);
                graphics.DrawImage(DrawGradient(gradientRect), gradientRect);

            }
            outputImage.Save($"{name}");
        }

        private Bitmap DrawGradient(Rectangle grad)
        {
            Bitmap output = new Bitmap(grad.Width, grad.Height);
            using (Graphics graphics = Graphics.FromImage(output))
            {
                SetHighQualityGraphics(graphics);
                for (int i = 0; i < 650; ++i)
                {
                    graphics.DrawLine(new Pen(gradient.GetGradientColor(i)), 0, i, 10, i);
                    if (i % (1000 / 20) == 0)
                    {
                        graphics.DrawString($"-{i / (1000 / 20) * 10}", new Font("Monospace", 8), Brushes.LightGray, 10, i);
                    }
                }
                graphics.DrawString($"dBFS", new Font("Monospace", 8), Brushes.LightGray, 0, output.Height - 20);
            }
            return output;
        }

        private Bitmap DrawScales(Bitmap channelImage, Rectangle position, int outline)
        {
            Bitmap output = new Bitmap(position.Width, position.Height);
            using (Graphics graphics = Graphics.FromImage(output))
            {

                SetHighQualityGraphics(graphics);

                int plotHeigth = position.Height - outline;
                int plotWidth = position.Width - outline;

                Rectangle verticalScale = new Rectangle(0, 0, outline, plotHeigth);
                Rectangle verticalScalePosition = new Rectangle(plotWidth, 0, outline, plotHeigth);
                Rectangle horizontalScale = new Rectangle(0, 0, plotWidth, outline);
                Rectangle horizontalScalePosition = new Rectangle(0, plotHeigth, plotWidth, outline);

                Rectangle channelPosition = new Rectangle(outline, outline, plotWidth - outline, plotHeigth - outline);

                graphics.DrawImage(channelImage, channelPosition);

                graphics.DrawImage(DrawWerticalScale(verticalScale, false), verticalScale);
                graphics.DrawImage(DrawWerticalScale(verticalScale, true), verticalScalePosition);
                graphics.DrawImage(DrawHorizontalScale(horizontalScale, false, new TimeSpan(0, 4, 40)), horizontalScale);
                graphics.DrawImage(DrawHorizontalScale(horizontalScale, true, new TimeSpan(0, 4, 40)), horizontalScalePosition);

                graphics.Flush();
            }
            return output;
        }

        private static void SetHighQualityGraphics(Graphics graphics)
        {
            graphics.InterpolationMode = InterpolationMode.High;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
        }

        private Bitmap DrawHorizontalScale(Rectangle scale, Boolean onTop, TimeSpan time)
        {
            Bitmap output = new Bitmap(scale.Width, scale.Height);
            using (Graphics graphics = Graphics.FromImage(output))
            {
                if (onTop)
                {
                    graphics.DrawLine(Pens.LightGray, new Point(scale.X + 20, scale.Y), new Point(scale.Width, scale.Y));
                    for (int i = 1; i <= 21; ++i)
                    {
                        graphics.DrawString($"{(int)(time.TotalSeconds / 22 * i)}", new Font("Monospace", 8), Brushes.LightGray, (scale.Width / 22) * i, 5);
                    }
                }
                else
                {
                    graphics.DrawLine(Pens.LightGray, new Point(scale.X + 20, scale.Height - 1), new Point(scale.Width, scale.Height - 1));
                }
                graphics.Flush();
            }
            return output;
        }

        private Bitmap DrawWerticalScale(Rectangle scale, Boolean lineOnLeft)
        {
            Bitmap output = new Bitmap(scale.Width, scale.Height);
            Graphics graphics = Graphics.FromImage(output);
            if (lineOnLeft)
            {
                graphics.DrawLine(Pens.LightGray, new Point(scale.X, scale.Y + 20), new Point(scale.X, scale.Height));
            }
            else
            {
                graphics.DrawLine(Pens.LightGray, new Point(scale.Width - 1, scale.Y + 20), new Point(scale.Width - 1, scale.Height));
            }
            for (int i = 1; i <= 22; ++i)
            {
                graphics.DrawString($"{i}", new Font("Monospace", 8), Brushes.LightGray, 0, scale.Height - (scale.Height / 22) * i);
            }
            graphics.Flush();
            graphics.Dispose();
            return output;
        }

        private void SaveSingleChannel(string name)
        {
            Image.Save($"{name}");
        }

        private void RemoveEmptyRows()
        {
            Image = Image.Clone(new Rectangle(0, 0, currentRow, Image.Height), Image.PixelFormat);
        }

        public void Dispose()
        {
            Image.Dispose();
        }
    }
}
