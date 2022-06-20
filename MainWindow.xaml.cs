using System;
using System.Windows;
using System.IO;
using System.Windows.Media.Imaging;
using Ghostscript.NET;
using Ghostscript.NET.Rasterizer;
using Ghostscript.NET.Viewer;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Linq;
using KRUS_DiffPdf.Content;
using System.Text;
using System.Drawing;
using System.Windows.Media;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Threading;

namespace KRUS_DiffPdf
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static FileContents contentsFileFirst = new FileContents();
        public static FileContents contentsFileSecond = new FileContents();
        public int currentPage = 1;
        public NumberFiles enumFiles;
        public MainWindow()
        {
            InitializeComponent();
        }
        public class FileContents
        {
            public string FilePath { get; set; }          
            public FileContents()
            {

            }
            //public Image picture { get; set; }
            public byte[] FileBody { get; set; }
            public List<byte[]> PagesByte { get; set; }
            public List<BitmapSource> Pages { get; set; }
            public int numberPage { get; set; }
        }
        public FileContents ReadFile(string pathFile)
        {            
            using (GhostscriptRasterizer rasterizer = new GhostscriptRasterizer())
            {
                var fileContents = new FileContents();
                GhostscriptVersionInfo gvi = new GhostscriptVersionInfo(@"C:\Program Files (x86)\gs\gs9.56.1\bin\gsdll32.dll");
                fileContents.FileBody = File.ReadAllBytes(pathFile);
                fileContents.FilePath = pathFile;
                fileContents.Pages = new List<BitmapSource>();
                MemoryStream ms = new MemoryStream(fileContents.FileBody);
                rasterizer.Open(ms, gvi, true);
                GhostscriptPngDevice pngDevice = new GhostscriptPngDevice(GhostscriptPngDeviceType.Png16m);
                fileContents.Pages.Add(null);
                for (int pageNumber = 1; pageNumber <= 5; pageNumber++)
                {
                    pngDevice.GraphicsAlphaBits = GhostscriptImageDeviceAlphaBits.V_4;
                    pngDevice.TextAlphaBits = GhostscriptImageDeviceAlphaBits.V_4;
                    pngDevice.ResolutionXY = new GhostscriptImageDeviceResolution(150, 150);
                    pngDevice.InputFiles.Add(TextBoxPathFileOne.Text);
                    pngDevice.Pdf.FirstPage = pageNumber;
                    pngDevice.Pdf.LastPage = pageNumber;
                    pngDevice.CustomSwitches.Add("-dDOINTERPOLATE");
                    pngDevice.OutputPath = $@"{TextBoxPathFileOne.Text.Replace(".pdf", "")}_{pageNumber}.png";
                    pngDevice.Process();
                    byte[] bytes = pngDevice.OutputPath.Select(x => (byte)(x)).ToArray();                    
                    BitmapImage image = new BitmapImage();
                    var imageSource = ToImageSource(image,bytes, pngDevice.OutputPath);
                    fileContents.numberPage = pageNumber;
                    fileContents.Pages.Add(imageSource);
                    File.Delete(pngDevice.OutputPath);
                    //ImageDoc1.Source = fileContents.Pages[pageNumber];
                }
                return fileContents;
            } 
        }
        public static BitmapSource ToImageSource(BitmapImage image,byte[] fileBody,string pathFile)
        {           
            using (MemoryStream stream = new MemoryStream(fileBody))
            {                               
                // Rewind the stream
                stream.Seek(0, SeekOrigin.Current);
                // Tell the WPF BitmapImage to use this stream
                image.BeginInit();
                image.UriSource = new Uri(pathFile);
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
            }
            return image;        
        }
        private void ButtonTest_Click(object sender, RoutedEventArgs e)
        {
            Errors();
            ReadFile("");
        }
        private void ButtonChooseFileOne_Click(object sender, RoutedEventArgs e)
        {            
            var filePath = ChooseFile(enumFiles = NumberFiles.FileFirst);
            if (filePath == "") 
            {
                TextBoxPathFileOne.Text = "";
                return;
            }
            TextBoxPathFileOne.Text = filePath;
            contentsFileFirst = ReadFile(filePath);
            ImageDoc1.Source = contentsFileFirst.Pages[contentsFileFirst.numberPage];
        }
        private void ButtonChooseFileTwo_Click(object sender, RoutedEventArgs e)
        {
            ChooseFile(enumFiles = NumberFiles.FileSecond);
        }
        private string ChooseFile(NumberFiles numberFiles)
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = false,
                Multiselect = true
            };
            dialog.Filters.Add(new CommonFileDialogFilter("PDF документ", "*.pdf"));
            dialog.Filters.Add(new CommonFileDialogFilter("PDF документ", "*.PDF"));
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                foreach (var file in dialog.FilesAsShellObject)
                {
                    if (numberFiles == NumberFiles.FileFirst)
                    {
                        contentsFileFirst.FilePath = file.ParsingName;
                        TextBoxPathFileOne.Text = contentsFileFirst.FilePath;
                    }
                    if (numberFiles == NumberFiles.FileSecond)
                    {
                        contentsFileSecond.FilePath = file.ParsingName;
                        TextBoxPathFileTwo.Text = contentsFileSecond.FilePath;
                    }
                }
                return dialog.FileName;
            }
            return "";
        }
        private void Errors()
        {
            if (TextBoxPathFileOne.Text == string.Empty || TextBoxPathFileTwo.Text == string.Empty)
            {
                MessageBox.Show("Выберите файл(ы).", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        private void ButtonDown_Click(object sender, RoutedEventArgs e)
        {
            ImageDoc1.Source = contentsFileFirst.Pages[contentsFileFirst.numberPage--];
        }
        private void ButtonUp_Click(object sender, RoutedEventArgs e)
        {
            ImageDoc1.Source = contentsFileFirst.Pages[contentsFileFirst.numberPage++];
        }
    }
}