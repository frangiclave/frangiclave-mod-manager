using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Frangiclave
{
    /**
     * Source: https://answers.unity.com/questions/683772/export-sprite-sheets.html
     */
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    internal static class Texture2DExtensions
    {
        public static Texture2D CropTexture(this Texture2D source, int left, int top, int width, int height)
        {
            if (left < 0)
            {
                width += left;
                left = 0;
            }
            if (top < 0)
            {
                height += top;
                top = 0;
            }
            if (left + width > source.width)
            {
                width = source.width - left;
            }
            if (top + height > source.height)
            {
                height = source.height - top;
            }

            if (width <= 0 || height <= 0)
            {
                return null;
            }

            var sourceColor = source.GetPixels(0);
            var croppedTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);

            var area = width * height;
            var pixels = new Color[area];

            var i = 0;
            for (var y = 0; y < height; y++)
            {
                var sourceIndex = (y + top) * source.width + left;
                for (var x = 0; x < width; x++)
                {
                    pixels[i++] = sourceColor[sourceIndex++];
                }
            }

            croppedTexture.SetPixels(pixels);
            croppedTexture.Apply();

            return croppedTexture;
        }
    }
}
