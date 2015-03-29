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
        /// <summary>
        /// Represents the used algorythm for creating a thumbnail.
        /// </summary>
        public enum ThumbnailQuality
        {
            Normal,
            High
        }
        /// <summary>
        /// The dimension of an image.
        /// </summary>
        public enum Dimension
        {
            Height,
            Width
        }

        /// <summary>
        /// Used to create resized images with a minor loss in quality. Since we just want to get smaller images this is somewhat pintless.
        /// </summary>
        /// <param name="image">The image that should be resized.</param>
        /// <param name="width">Width of the target image.</param>
        /// <param name="height">Height of the target image.</param>
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

        /// <summary>
        /// Used to create resized images.
        /// </summary>
        /// <param name="image">The image that should be resized.</param>
        /// <param name="width">Width of the target image.</param>
        /// <param name="height">Height of the target image.</param>
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            return (Bitmap)(new Bitmap(image, new Size(width, height)));
        }
        /// <summary>
        /// Creates a thumbnail that has a maximum size in one dimension.
        /// </summary>
        /// <param name="image">The image that acts as the source of the thumbnail.</param>
        /// <param name="dimension">The dimension that gives the limit in size.</param>
        /// <param name="maxSizeInDimension">Maximum width/height of the thumbnail.</param>
        /// <param name="keepRatio">If the aspect ratio should be kept.</param>
        /// <param name="quality">The quality of the thumbnail (the better the quality the bigger the file).</param>
        public static Bitmap Create(Image image, Thumbnail.Dimension dimension, int maxSizeInDimension, bool keepRatio, Thumbnail.ThumbnailQuality quality)
        {
            //We jsut want thumbnails, so the image should get smaller.
            if (image.Height <= maxSizeInDimension && dimension == Dimension.Height) throw new Exception("The image would not get smaller by this resize operation.");
            if (image.Width <= maxSizeInDimension && dimension == Dimension.Width) throw new Exception("The image would not get smaller by this resize operation.");

            Size newSize = new Size(); //New site of the image.

            if (!keepRatio) //Do not keep aspect ratio.
            {
                if (dimension == Dimension.Height)
                {
                    newSize.Height = maxSizeInDimension; //The new height is the maximum size in the dimension of height.
                    newSize.Width = image.Width; //The width stays the same because we do not keep the aspect ratio.
                }
                else if (dimension == Dimension.Width)
                {
                    newSize.Height = image.Height; //The height stays the same because we do not keep the aspect ratio.
                    newSize.Width = maxSizeInDimension; //The new width is the maximum size in the dimension of width.
                }
            }
            else if (keepRatio) //If the aspect ratio should be cept we have to zoom the image by dividing width and height it with the zoom factor.
            {
                double zoomFactor;
                if (dimension == Dimension.Height)
                {
                    zoomFactor = (double)image.Height / (double)maxSizeInDimension; //The height gives the zoom factor because it is our current dimension.
                    newSize.Height = maxSizeInDimension;
                    newSize.Width = Convert.ToInt32(image.Width / zoomFactor);
                }
                else if (dimension == Dimension.Width)
                {
                    zoomFactor = (double)image.Width / (double)maxSizeInDimension; //The width gives the zoom factor because it is our current dimension.
                    newSize.Height = Convert.ToInt32(image.Height / zoomFactor);
                    newSize.Width = maxSizeInDimension;
                }
            }

            //Creating the thumbanil with the given quality.
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
        /// <summary>
        /// Creates a thumbnail that has a maximum size in both dimension.
        /// </summary>
        /// <param name="image">The image that acts as the source of the thumbnail.</param>
        /// <param name="maxHeight">The maximum height of the thumbnail.</param>
        /// <param name="maxWidth">The maximum width of the thumbnail.</param>
        /// <param name="keepRatio">If the aspect ratio should be kept.</param>
        /// <param name="quality">The quality of the thumbnail (the better the quality the bigger the file).</param>
        public static Bitmap Create(Image image, int maxWidth, int maxHeight, bool keepRatio, Thumbnail.ThumbnailQuality quality)
        {
            //Again we want a smaller image as result.
            if (image.Height <= maxHeight && image.Width <= maxWidth) throw new Exception("The image would not get smaller by this resize operation.");

            Size newSize = new Size(); //Size of the thumbnail.

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
                    return (Create(image, Dimension.Width, maxWidth, true, quality));
                }
                else if (image.Width <= maxWidth && image.Height > maxHeight)
                {
                    return (Create(image, Dimension.Height, maxHeight, true, quality));
                }
                else if (image.Width > maxWidth && image.Height > maxHeight)
                {
                    double zoomFactorWidth, zoomFactorHeight;

                    zoomFactorWidth = (double)image.Width / (double)maxWidth;
                    zoomFactorHeight = (double)image.Height / (double)maxHeight;

                    if (zoomFactorWidth > zoomFactorHeight)
                    {
                        return Create(image, Dimension.Width,maxWidth, true, quality);
                    }
                    else
                    {
                        return Create(image, Dimension.Height,maxHeight, true, quality);
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
