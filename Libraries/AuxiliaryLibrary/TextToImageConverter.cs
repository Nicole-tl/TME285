using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuxiliaryLibrary
{
    public class TextToImageConverter
    {
        private string fontFamily = "Calibri";
        private int fontSize = 12;
        private Color textColor;
        private Font font;
        private Bitmap tmpBitmap;

        public TextToImageConverter()
        {
            font = new Font(fontFamily, fontSize);
            textColor = Color.White;
            tmpBitmap = new Bitmap(1, 1);
        }

        public Bitmap Convert(string text)
        {
            Bitmap textAsBitmap = null;
            List<string> textLineList = text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            using (Graphics tmpGraphics = Graphics.FromImage(tmpBitmap))
            {
                float height = 0;
                float width = 0;
                foreach (string textLine in textLineList)
                {
                    SizeF size = tmpGraphics.MeasureString(textLine, font);
                    if (size.Width > width) { width = size.Width; }
                    height += size.Height;
                }
                float top = 0;
                textAsBitmap = new Bitmap((int)width, (int)height);
                using (Graphics graphics = Graphics.FromImage(textAsBitmap))
                {
                    using (SolidBrush brush = new SolidBrush(textColor))
                    {
                        foreach (string textLine in textLineList)
                        {
                            graphics.DrawString(textLine, font, brush, new PointF(0, top));
                            SizeF size = tmpGraphics.MeasureString(textLine, font);
                            top += size.Height;
                        }
                    }
                }
            }
            return textAsBitmap;
        }

        public string FontFamily
        {
            get { return fontFamily; }
            set { fontFamily = value; }
        }

        public int FontSize
        {
            get { return fontSize; }
            set { fontSize = value; }
        }

        public Color TextColor
        {
            get { return textColor; }
            set { textColor = value; }
        }
    }
}
