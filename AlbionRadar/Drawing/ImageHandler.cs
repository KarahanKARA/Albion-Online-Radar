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
    private static float _lastIconSize = 0;
    private static readonly HashSet<string> _missingImageKeys = new HashSet<string>();
    private static readonly object _lockObject = new object();
    private static bool _isReloading = false;
    private static DateTime _lastReloadTime = DateTime.MinValue;

    public static void Load(RenderTarget target)
    {
        _currentTarget = target;
        _lastIconSize = Config.Instance.RadarParams.IconSize;
        LoadAllImages();
    }

    public static void Reload()
    {
        if (_currentTarget == null || _currentTarget.IsDisposed)
        {
            MainForm.Log("Cannot reload - RenderTarget is null or disposed");
            return;
        }

        try
        {
            foreach (var image in _images.Values)
            {
                if (image != null && !image.IsDisposed)
                {
                    image.Dispose();
                }
            }
            _images.Clear();

            LoadAllImages();
        }
        catch (Exception ex)
        {
            MainForm.Log($"Error in Reload: {ex.Message}");
        }
    }

    private static void LoadAllImages()
    {
        if (_currentTarget == null || _currentTarget.IsDisposed)
        {
            MainForm.Log("Cannot load images - RenderTarget is null or disposed");
            return;
        }

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
                    var bitmap = LoadBitmap(_currentTarget, image);
                    if (bitmap != null)
                    {
                        _images[key] = bitmap;
                    }
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
        if (target == null || target.IsDisposed)
        {
            MainForm.Log("RenderTarget is null or disposed");
            return null;
        }

        try
        {
            float iconSize = Config.Instance.RadarParams.IconSize;
            iconSize = Math.Max(4.0f, Math.Min(12.0f, iconSize));
            iconSize = (float)(Math.Round(iconSize / 2.0) * 2.0);
            
            float scaleFactor = iconSize / 24.0f;
            
            int minSize = 8;
            int newWidth = Math.Max(minSize, Math.Min(1024, (int)(image.Width * scaleFactor)));
            int newHeight = Math.Max(minSize, Math.Min(1024, (int)(image.Height * scaleFactor)));
            
            using System.Drawing.Bitmap bmp = new(image, new Size(newWidth, newHeight));
            BitmapData bmpData = null;
            Bitmap result = null;

            try
            {
                if (target.IsDisposed)
                {
                    MainForm.Log("RenderTarget was disposed during bitmap creation");
                    return null;
                }

                bmpData = bmp.LockBits(
                    new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                    ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppPArgb
                );

                using DataStream stream = new(bmpData.Scan0, bmpData.Stride * bmpData.Height, true, false);
                var pFormat = new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied);
                var bmpProps = new BitmapProperties(pFormat);

                if (!target.IsDisposed)
                {
                    result = new Bitmap(target, new Size2(bmp.Width, bmp.Height), stream, bmpData.Stride, bmpProps);
                    return result;
                }
                else
                {
                    MainForm.Log("RenderTarget was disposed before bitmap creation");
                    return null;
                }
            }
            catch (Exception ex)
            {
                if (result != null && !result.IsDisposed)
                {
                    result.Dispose();
                }

                if (ex.Message.Contains("D2DERR_RECREATE_TARGET"))
                {
                    MainForm.Log("RenderTarget needs to be recreated");
                }
                else
                {
                    MainForm.Log($"Error creating bitmap: {ex.Message}");
                }
                return null;
            }
            finally
            {
                if (bmpData != null)
                {
                    try
                    {
                        bmp.UnlockBits(bmpData);
                    }
                    catch
                    {
                        // Ignore unlock errors
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MainForm.Log($"Error in LoadBitmap: {ex.Message}");
            return null;
        }
    }

    public static Bitmap GetImage(string name)
    {
        try
        {
            if ((DateTime.Now - _lastReloadTime).TotalMilliseconds < 100)
            {
                return _images.TryGetValue(name.ToLower(), out Bitmap cachedImage) ? cachedImage : null;
            }

            if (_lastIconSize != Config.Instance.RadarParams.IconSize)
            {
                lock (_lockObject)
                {
                    if (!_isReloading)
                    {
                        _isReloading = true;
                        try
                        {
                            _lastIconSize = Config.Instance.RadarParams.IconSize;
                            Reload();
                            _lastReloadTime = DateTime.Now;
                        }
                        finally
                        {
                            _isReloading = false;
                        }
                    }
                }
            }

            string key = name.ToLower();
            if (_images.TryGetValue(key, out Bitmap image))
            {
                if (image != null && !image.IsDisposed)
                {
                    return image;
                }
                else
                {
                    _images.Remove(key);
                }
            }

            if (!_missingImageKeys.Contains(key))
            {
                _missingImageKeys.Add(key);
                MainForm.Log($"Can't find image: {key}");
            }
        }
        catch (Exception ex)
        {
            MainForm.Log($"Error in GetImage: {ex.Message}");
        }
        
        return null;
    }

    
}