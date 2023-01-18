using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;

namespace Kenedia.Modules.Characters
{
    internal static class ContainerExtensions
    {
        public static void ToggleVisibility(this Container c)
        {
            c.Visible = !c.Visible;
        }
    }
    internal static class ControlExtensions
    {
        public static void ToggleVisibility(this Control c)
        {
            c.Visible = !c.Visible;
        }
    }
    internal static class PointExtensions
    {
        public static int Distance2D(this Point p1, Point p2)
        {
            return (int)Math.Sqrt(Math.Pow((p2.X - p1.X), 2) + Math.Pow((p2.Y - p1.Y), 2));
        }
        public static int Distance3D(this Vector3 p1, Vector3 p2)
        {
            float deltaX = p2.X - p1.X;
            float deltaY = p2.Y - p1.Y;
            float deltaZ = p2.Z - p1.Z;

            float distance = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
            return (int)distance;
        }
        public static Point Add(this Point b, Point p)
        {
            return new Point(b.X + p.X, b.Y + p.Y);
        }
        public static Point Scale(this Point p, double factor)
        {
            return new Point((int)(p.X * factor), (int)(p.Y * factor));
        }
        public static string ConvertToString(this Point p)
        {
            return string.Format("X: {0}, Y: {1}", p.X, p.Y);
        }
    }
    internal static class DisposableExtensions
    {
        public static void DisposeAll(this IEnumerable<IDisposable> disposables)
        {
            foreach (var d in disposables)
                d?.Dispose();
        }
    }

    internal static class ListExtension
    {
        public static bool ContainsAny<T>(this IEnumerable<T> sequence, params T[] matches)
        {
            return matches.Any(value => sequence.Contains(value));
        }

        public static bool ContainsAll<T>(this IEnumerable<T> sequence, params T[] matches)
        {
            return matches.All(value => sequence.Contains(value));
        }
    }

    internal static class Texture2DExtension
    {

        static public Texture2D CreateTexture2D(this MemoryStream s)
        {
            Texture2D texture;

            using (var device = Blish_HUD.GameService.Graphics.LendGraphicsDeviceContext())
            {
                texture = Texture2D.FromStream(device.GraphicsDevice, s);
            }

            return texture;
        }

        static public Texture2D ToGrayScaledPalettable(this Texture2D original)
        {
            //make an empty bitmap the same size as original
            Color[] colors = new Color[original.Width * original.Height];
            original.GetData<Color>(colors);
            Color[] destColors = new Color[original.Width * original.Height];
            Texture2D newTexture;

            using (var device = Blish_HUD.GameService.Graphics.LendGraphicsDeviceContext())
            {
                newTexture = new Texture2D(device.GraphicsDevice, original.Width, original.Height);
            }

            for (int i = 0; i < original.Width; i++)
            {
                for (int j = 0; j < original.Height; j++)
                {
                    //get the pixel from the original image
                    int index = i + j * original.Width;
                    Color originalColor = colors[index];

                    //create the grayscale version of the pixel
                    float maxval = .3f + .59f + .11f + .79f;
                    float grayScale = (((originalColor.R / 255f) * .3f) + ((originalColor.G / 255f) * .59f) + ((originalColor.B / 255f) * .11f) + ((originalColor.A / 255f) * .79f));
                    grayScale = grayScale / maxval;

                    destColors[index] = new Color(grayScale, grayScale, grayScale, originalColor.A);
                }
            }
            newTexture.SetData<Color>(destColors);
            return newTexture;
        }
    }
}
