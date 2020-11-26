using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;

namespace Spectrogram
{
    public class BitmapGenerator : IBitmapGenerator
    {
        public Bitmap Image { get; private set; }
        private AudioData AudioInfo { get; init; }
        private int currentRow = 0;
        private readonly Gradient gradient = new Gradient();
        private readonly int outline = 40;

        public BitmapGenerator(AudioData audioData, int width, int height)
        {
            AudioInfo = audioData;
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
            string name = @$"{ Path.GetDirectoryName(AudioInfo.FilePath)}{separator}{ Path.GetFileNameWithoutExtension(AudioInfo.FilePath) }.png";
            if (AudioInfo.Channels == 1)
            {
                SaveSingleChannel(name);
            }
            else
            {
                SaveMultiChannel(name);
            }
        }

        private void SaveSingleChannel(string name)
        {
            Rectangle channel1rect = new Rectangle(0, 0, Image.Width, Image.Height);
            Bitmap channel1 = Image.Clone(channel1rect, Image.PixelFormat);
            channel1 = DrawSingleChannelWithScales(channel1);
            channel1.Save(name);
            channel1.Dispose();
        }

        private void SaveMultiChannel(string name)
        {
            Rectangle channel1rect = new Rectangle(0, 0, Image.Width, Image.Height / 2);
            Bitmap channel1 = Image.Clone(channel1rect, Image.PixelFormat);
            channel1.RotateFlip(RotateFlipType.RotateNoneFlipY);
            channel1 = DrawSingleChannelWithScales(channel1);

            Rectangle channel2rect = new Rectangle(0, Image.Height / 2, Image.Width, Image.Height / 2);
            Bitmap channel2 = Image.Clone(channel2rect, Image.PixelFormat);
            channel2 = DrawSingleChannelWithScales(channel2);

            int gradientWidth = 50;
            using Bitmap outputImage = new Bitmap(channel1.Width + gradientWidth, channel1.Height + channel2.Height);
            using Graphics graphics = Graphics.FromImage(outputImage);
            SetHighQualityGraphics(graphics);
            graphics.FillRectangle(Brushes.Black, new Rectangle(channel1.Width, 0, gradientWidth, outputImage.Height));

            Rectangle channel1Position = new Rectangle(0, 0, channel1.Width, channel1.Height);
            Rectangle channel2Position = new Rectangle(0, channel1.Height, channel2.Width, channel2.Height);

            graphics.DrawImage(channel1, channel1Position);
            graphics.DrawImage(channel2, channel2Position);

            channel1.Dispose();
            channel2.Dispose();

            Rectangle gradientRect = new Rectangle(outputImage.Width - gradientWidth, outputImage.Height * 1 / 5, 40, 680);

            graphics.DrawImage(DrawGradient(gradientRect), gradientRect);
            graphics.Flush();

            outputImage.Save($"{name}");
        }

        private Bitmap DrawSingleChannelWithScales(Bitmap channel1)
        {
            Bitmap output = new Bitmap(channel1.Width + outline, channel1.Height + outline);
            using Graphics graphics = Graphics.FromImage(output);
            SetHighQualityGraphics(graphics);
            Rectangle fullImage = new Rectangle(0, 0, output.Width, output.Height);
            graphics.FillRectangle(Brushes.Black, fullImage);
            graphics.DrawImage(DrawScales(channel1, fullImage, 20), fullImage);
            graphics.Flush();
            return output;
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
                graphics.Flush();
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
                Rectangle verticalScaleLeftLine = new Rectangle(plotWidth, 0, outline, plotHeigth);
                Rectangle horizontalScale = new Rectangle(0, 0, plotWidth + 20, outline);
                Rectangle horizontalScaleTopLine = new Rectangle(0, plotHeigth, plotWidth + 20, outline);

                Rectangle channelPosition = new Rectangle(outline, outline, plotWidth - outline, plotHeigth - outline);

                graphics.DrawImage(channelImage, channelPosition);

                graphics.DrawImage(DrawWerticalScale(verticalScale, false), verticalScale);
                graphics.DrawImage(DrawWerticalScale(verticalScale, true), verticalScaleLeftLine);
                graphics.DrawImage(DrawHorizontalScale(horizontalScale, false), horizontalScale);
                graphics.DrawImage(DrawHorizontalScale(horizontalScale, true), horizontalScaleTopLine);

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

        private Bitmap DrawHorizontalScale(Rectangle scale, Boolean onTop)
        {
            Bitmap output = new Bitmap(scale.Width, scale.Height);
            using (Graphics graphics = Graphics.FromImage(output))
            {
                if (onTop)
                {
                    int timeStepCount = 2;
                    if ((int)AudioInfo.Duration.TotalSeconds > 100)
                    {
                        timeStepCount = (int)AudioInfo.Duration.TotalSeconds / 50;
                    }
                    int timeStep = (int)AudioInfo.Duration.TotalSeconds / timeStepCount;
                    graphics.DrawLine(Pens.LightGray, new Point(scale.X + 20, scale.Y), new Point(scale.Width - 20, scale.Y));
                    for (int i = 1; i <= timeStepCount; ++i)
                    {
                        graphics.DrawString($"{(timeStep * i)}", new Font("Monospace", 8), Brushes.LightGray, ((scale.Width / timeStepCount) * i) - 20, 5);
                    }
                }
                else
                {
                    graphics.DrawLine(Pens.LightGray, new Point(scale.X + 20, scale.Height - 1), new Point(scale.Width - 20, scale.Height - 1));
                }
                graphics.Flush();
            }
            return output;
        }

        private Bitmap DrawWerticalScale(Rectangle scale, Boolean lineOnLeft)
        {
            Bitmap output = new Bitmap(scale.Width, scale.Height);
            using Graphics graphics = Graphics.FromImage(output);
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
            return output;
        }

        private void RemoveEmptyRows()
        {
            Image = Image.Clone(new Rectangle(0, 0, currentRow, Image.Height), Image.PixelFormat);
        }

        public void Dispose()
        {
            Image.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
