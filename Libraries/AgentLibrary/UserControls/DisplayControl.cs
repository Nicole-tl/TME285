using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AgentLibrary.UserControls
{
    public partial class DisplayControl : UserControl
    {
        protected Bitmap image;
        protected Boolean fill = false;

        public DisplayControl()
        {
            InitializeComponent();
            Clear();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (SolidBrush backgroundBrush = new SolidBrush(BackColor))
            {
                e.Graphics.FillRectangle(backgroundBrush, 0, 0, this.Width, this.Height);
            }
            if (image != null)
            {
                e.Graphics.DrawImage(image, new PointF(0, 0));
            }
        }

        // Some quality loss here - better not to resize, if it can be avoided.
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (image != null)
            {
                if (fill)
                {
                    Bitmap resizedImage = new Bitmap(this.Width, this.Height);
                    using (Graphics g = Graphics.FromImage(resizedImage))
                    {
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.DrawImage(image, new Rectangle(0, 0, this.Width, this.Height));
                    }
                    AssignImage(resizedImage);
                }
            }
        }

        public void Clear()
        {
            image = null;
            fill = false;
          //  Invalidate();  // Handled (with thread safety) externally instead.
        }

        public void FillImage(Bitmap image)
        {
            fill = true;
            this.image = new Bitmap(image, new Size(this.Width, this.Height));
            //  Invalidate();  // Handled (with thread safety) externally instead.
        }

        public void AssignImage(Bitmap image)
        {
            this.image = new Bitmap(image, new Size(image.Width, image.Height));
            //   Invalidate();  // Handled (with thread safety) externally instead.
        }
    }
}
