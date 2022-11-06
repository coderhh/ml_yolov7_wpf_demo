using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Yolov5Net.Scorer;
using Yolov5Net.Scorer.Models;

namespace ml_yolov7_wpf_demo;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public string ImageFilePath = "";
    public YoloScorer<YoloCocoP5Model> Scorer;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void btnLoad_Click(object sender, RoutedEventArgs e)
    {
        var op = new OpenFileDialog();
        op.Title = "Select an image";
        op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
                    "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                    "Portable Network Graphic (*.png)|*.png";
        if (op.ShowDialog() == true)
        {
            ImageFilePath = op.FileName;
            imgPhoto.Source = new BitmapImage(new Uri(op.FileName));
        }
    }

    private void btnProcess_Click(object sender, RoutedEventArgs e)
    {
        var image = Image.FromFile(ImageFilePath);
        List<YoloPrediction> predictions = Scorer.Predict(image);

        using var graphics = Graphics.FromImage(image);
        foreach (var prediction in predictions)
        {
            var score = Math.Round(prediction.Score, 2);

            graphics.DrawRectangles(new Pen(prediction.Label.Color, 3),
                new[] { prediction.Rectangle });

            var (x, y) = (prediction.Rectangle.X - 3, prediction.Rectangle.Y - 23);

            graphics.DrawString($"{prediction.Label.Name} ({score})",
                new Font("Consolas", 16, GraphicsUnit.Pixel), new SolidBrush(prediction.Label.Color),
                new PointF(x, y));
        }

        var bitmap = new BitmapImage();

        using (var stream = new MemoryStream())
        {
            image.Save(stream, ImageFormat.Jpeg);

            stream.Seek(0, SeekOrigin.Begin);

            bitmap.BeginInit();
            bitmap.StreamSource = stream;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
        }

        imgPhoto.Source = bitmap;
    }

    private void btnLoadModel_Click(object sender, RoutedEventArgs e)
    {
        var op = new OpenFileDialog();
        op.Title = "Select an ONNX model file";
        op.Filter = "Onnx mode file|*.onnx;";
        if (op.ShowDialog() == true)
        {
            var modelFilePath = op.FileName;
            Scorer = new YoloScorer<YoloCocoP5Model>(modelFilePath);
        }
    }
}