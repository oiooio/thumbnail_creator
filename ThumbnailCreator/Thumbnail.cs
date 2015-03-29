using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThumbnailCreator
{
    class Thumbnail
    {
        public enum ThumbnailQuality
        {
            Normal,
            High
        }
        public enum Dimension
        {
            Height,
            Width
        }

        private static Bitmap ResizeImageHQ(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            return (Bitmap)(new Bitmap(image, new Size(width, height)));
        }
        public static Bitmap Create(Image image, int maxSizeInDimension, Thumbnail.Dimension dimension, bool keepRatio, Thumbnail.ThumbnailQuality quality)
        {
            //Hier sollen Thumbnail entstehen, die Bilder sollen also kleiner werden und nicht größer.
            if (image.Height <= maxSizeInDimension && dimension == Dimension.Height) throw new Exception("The image would not get smaller by this resize operation.");
            if (image.Width <= maxSizeInDimension && dimension == Dimension.Width) throw new Exception("The image would not get smaller by this resize operation.");

            Size newSize = new Size(); //Neue Größe des Bildes.

            if (!keepRatio) //Längenverhältnis nicht beibehalten.
            {
                if (dimension == Dimension.Height)
                {
                    newSize.Height = maxSizeInDimension; //Neue Höhe wird die angestrebte größe der gewählten Dimension.
                    newSize.Width = image.Width; //Die Breitebleibt erhalten.
                }
                else if (dimension == Dimension.Width)
                {
                    newSize.Height = image.Height; //Die Höhe bleibt erhaten.
                    newSize.Width = maxSizeInDimension; //Neue Höhe wird die angestrebte größe der gewählten Dimension.
                }
            }
            else if (keepRatio) //Wenn das Seitenverhältnis beibehalten werden soll, so muss das Bild "gezoomt" werden, also alle Größen duren einen Faktor dividiert werden.
            {
                double zoomFactor;
                if (dimension == Dimension.Height)
                {
                    zoomFactor = (double)image.Height / (double)maxSizeInDimension;
                    newSize.Height = maxSizeInDimension;
                    newSize.Width = Convert.ToInt32(image.Width / zoomFactor);
                }
                else if (dimension == Dimension.Width)
                {
                    zoomFactor = (double)image.Width / (double)maxSizeInDimension;
                    newSize.Height = Convert.ToInt32(image.Height / zoomFactor);
                    newSize.Width = maxSizeInDimension;
                }
            }

            if (quality == ThumbnailQuality.Normal)
            {
                return (ResizeImage(image, newSize.Width, newSize.Height));
            }
            else if (quality == ThumbnailQuality.High)
            {
                return (ResizeImageHQ(image, newSize.Width, newSize.Height));
            }
            else { return new Bitmap(0, 0); }

        }
        public static Bitmap Create(Image image, int maxWidth, int maxHeight, bool keepRatio, Thumbnail.ThumbnailQuality quality)
        {
            //Hier sollen Thumbnail entstehen, die Bilder sollen also kleiner werden und nicht größer.
            if (image.Height <= maxHeight && image.Width <= maxWidth) throw new Exception("The image would not get smaller by this resize operation.");

            Size newSize = new Size();

            if (!keepRatio)
            {
                if (image.Width > maxWidth && image.Height <= maxHeight)
                {
                    newSize.Width = maxWidth;
                    newSize.Height = image.Height;
                }
                else if (image.Width <= maxWidth && image.Height > maxHeight)
                {
                    newSize.Width = image.Width;
                    newSize.Height = maxHeight;
                }
                else if (image.Width > maxWidth && image.Height > maxHeight)
                {
                    newSize.Width = maxWidth;
                    newSize.Height = maxHeight;
                }
            }
            else if (keepRatio)
            {
                if (image.Width > maxWidth && image.Height <= maxHeight)
                {
                    return (Create(image, maxWidth, Dimension.Width, true, quality));
                }
                else if (image.Width <= maxWidth && image.Height > maxHeight)
                {
                    return (Create(image, maxHeight, Dimension.Height, true, quality));
                }
                else if (image.Width > maxWidth && image.Height > maxHeight)
                {
                    double zoomFactorWidth, zoomFactorHeight;

                    zoomFactorWidth = (double)image.Width / (double)maxWidth;
                    zoomFactorHeight = (double)image.Height / (double)maxHeight;

                    if (zoomFactorWidth > zoomFactorHeight)
                    {
                        return Create(image, maxWidth, Dimension.Width, true, quality);
                    }
                    else
                    {
                        return Create(image, maxHeight, Dimension.Height, true, quality);
                    }
                }
            }
            if (quality == ThumbnailQuality.Normal)
            {
                return (ResizeImage(image, newSize.Width, newSize.Height));
            }
            else if (quality == ThumbnailQuality.High)
            {
                return (ResizeImageHQ(image, newSize.Width, newSize.Height));
            }
            else { return new Bitmap(0, 0); }
        }
    }
}
