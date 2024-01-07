﻿using Microsoft.Win32;
using ScottPlot.Control;
using System.Windows.Media.Imaging;
using System.Windows;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace ScottPlot.WPF;

public class Menu
{
    public string DefaultSaveImageFilename { get; set; } = "Plot.png";
    public List<ContextMenuItem> ContextMenuItems { get; set; } = new();
    readonly WpfPlotBase ThisControl;

    public Menu(WpfPlotBase control)
    {
        ThisControl = control;
        ContextMenuItems.AddRange(GetDefaultContextMenuItems());
    }

    public ContextMenuItem[] GetDefaultContextMenuItems()
    {
        ContextMenuItem saveImage = new()
        {
            Label = "Save Image",
            OnInvoke = OpenSaveImageDialog
        };

        ContextMenuItem copyImage = new()
        {
            Label = "Copy to Clipboard",
            OnInvoke = CopyImageToClipboard
        };

        return new ContextMenuItem[] { saveImage, copyImage };
    }

    public ContextMenu GetContextMenu()
    {
        ContextMenu menu = new();

        foreach (ContextMenuItem curr in ContextMenuItems)
        {
            MenuItem menuItem = new() { Header = curr.Label };
            menuItem.Click += (s, e) => curr.OnInvoke(ThisControl);
            menu.Items.Add(menuItem);
        }

        return menu;
    }

    public void OpenSaveImageDialog(IPlotControl plotControl)
    {
        SaveFileDialog dialog = new()
        {
            FileName = DefaultSaveImageFilename,
            Filter = "PNG Files (*.png)|*.png" +
                     "|JPEG Files (*.jpg, *.jpeg)|*.jpg;*.jpeg" +
                     "|BMP Files (*.bmp)|*.bmp" +
                     "|WebP Files (*.webp)|*.webp" +
                     "|SVG Files (*.svg)|*.svg" +
                     "|All files (*.*)|*.*"
        };

        if (dialog.ShowDialog() is true)
        {
            if (string.IsNullOrEmpty(dialog.FileName))
                return;

            ImageFormat format;

            try
            {
                format = ImageFormatLookup.FromFilePath(dialog.FileName);
            }
            catch (ArgumentException)
            {
                MessageBox.Show("Unsupported image file format", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                PixelSize lastRenderSize = plotControl.Plot.RenderManager.LastRender.FigureRect.Size;
                plotControl.Plot.Save(dialog.FileName, (int)lastRenderSize.Width, (int)lastRenderSize.Height, format);
            }
            catch (Exception)
            {
                MessageBox.Show("Image save failed", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
    }

    public static void CopyImageToClipboard(IPlotControl plotControl)
    {
        PixelSize lastRenderSize = plotControl.Plot.RenderManager.LastRender.FigureRect.Size;
        BitmapImage bmp = plotControl.Plot.GetBitmapImage((int)lastRenderSize.Width, (int)lastRenderSize.Height);
        Clipboard.SetImage(bmp);
    }
}
