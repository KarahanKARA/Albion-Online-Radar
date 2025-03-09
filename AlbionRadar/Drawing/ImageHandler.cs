using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Bitmap = SharpDX.Direct2D1.Bitmap;
using PixelFormat = SharpDX.Direct2D1.PixelFormat;

namespace AlbionRadar.Drawing;

public static class ImageHandler
{
    private static readonly Dictionary<string, Bitmap> _images = new Dictionary<string, Bitmap>();
    private static RenderTarget _currentTarget = null;
    private static int _lastIconSize = 0;
    private static readonly HashSet<string> _missingImageKeys = new HashSet<string>();

    public static void Load(RenderTarget target)
    {
        _currentTarget = target;
        _lastIconSize = Config.Instance.RadarParams.IconSize;
        LoadAllImages();
    }

    public static void Reload()
    {
        if (_currentTarget == null)
            return;

        foreach (var image in _images.Values)
        {
            image.Dispose();
        }
        _images.Clear();

        LoadAllImages();
    }

    private static void LoadAllImages()
    {
        Type resourceType = typeof(Properties.Resources);
        PropertyInfo[] properties = resourceType.GetProperties(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

        HashSet<string> processedKeys = new HashSet<string>();

        foreach (var property in properties)
        {
            if (property.PropertyType == typeof(System.Drawing.Bitmap))
            {
                string key = property.Name.ToLower();

                if (processedKeys.Contains(key))
                {
                    MainForm.Log($"Duplicate resource key found: {key}");
                    continue;
                }

                processedKeys.Add(key);

                using System.Drawing.Bitmap image = (System.Drawing.Bitmap)property.GetValue(null, null);
                if (image == null)
                    continue;

                try
                {
                    _images[key] = LoadBitmap(_currentTarget, image);
                }
                catch (Exception ex)
                {
                    MainForm.Log($"Error loading image {key}: {ex.Message}");
                }
            }
        }
    }

    private static Bitmap LoadBitmap(RenderTarget target, System.Drawing.Bitmap image)
    {
        int iconSize = Math.Max(3, Config.Instance.RadarParams.IconSize);
        int newWidth = Math.Max(1, image.Width / iconSize);
        int newHeight = Math.Max(1, image.Height / iconSize);
        using System.Drawing.Bitmap bmp = new(image, new Size(newWidth, newHeight));

        BitmapData bmpData = bmp.LockBits(new(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
        using DataStream stream = new(bmpData.Scan0, bmpData.Stride * bmpData.Height, true, false);

        PixelFormat pFormat = new(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied);
        BitmapProperties bmpProps = new(pFormat);

        Bitmap result = new(target, new(bmp.Width, bmp.Height), stream, bmpData.Stride, bmpProps);
        bmp.UnlockBits(bmpData);

        return result;
    }

    public static Bitmap GetImage(string name)
    {
        if (_lastIconSize != Config.Instance.RadarParams.IconSize)
        {
            _lastIconSize = Config.Instance.RadarParams.IconSize;
            Reload();
        }

        string key = name.ToLower();
        
        if (_images.TryGetValue(key, out Bitmap image))
            return image;

        if (!_missingImageKeys.Contains(key))
        {
            _missingImageKeys.Add(key);
            MainForm.Log($"Can't find image: {key}");
        }
        
        return null;
    }
}