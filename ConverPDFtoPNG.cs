using KRUS_DiffPdf.Content;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Threading;

namespace KRUS_DiffPdf.ReadDocuments
{
    public class ConverPDFtoPNG
    {
        private static bool _isQueueInitialized;
        private static bool _isJobQueueInitialized;
        private static pdfforge.PDFCreator.UI.ComWrapper.Queue CreateQueue()
        {
            if (_isQueueInitialized) return new pdfforge.PDFCreator.UI.ComWrapper.Queue();

            var queueType = Type.GetTypeFromProgID("PDFCreator.JobQueue");
            Activator.CreateInstance(queueType);
            _isQueueInitialized = true;
            return new pdfforge.PDFCreator.UI.ComWrapper.Queue();            
        }
        public FileContents ReadFilePdfCreator(string pathFile)
        {            
            var targetPath = $"{Path.GetTempPath()}{Path.GetFileName(pathFile).Replace(".pdf","")}.png";
            Process p = new Process();            
            p.StartInfo = new ProcessStartInfo()
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = false,
                Verb = "print",
                FileName = pathFile //put the correct path here
            };
            p.Start();
            pdfforge.PDFCreator.UI.ComWrapper.Queue queue;
            queue = CreateQueue();
            
            if (!_isJobQueueInitialized)
            {
                queue.Initialize();
                _isJobQueueInitialized = true;
            }
            if (!queue.WaitForJob(10000))
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action) (() =>
                {
                    MessageBox.Show("PDFCreator не был доступен 100 секунд");
                }));
            }
            else
            {
                var convertJob = queue.NextJob;
                convertJob.SetProfileSetting("OutputFormat", "Png");
                convertJob.SetProfileSetting("ShowProgress", "false");
                convertJob.ConvertTo(targetPath);
                KillPng();
                convertJob = null;
            }
            var fileContents = new FileContents();           
            fileContents.FileBody = File.ReadAllBytes(pathFile);                       
            fileContents.FilePath = pathFile;
            fileContents.Pages = new List<BitmapSource>();
            var files = Directory.EnumerateFiles(Path.GetTempPath(), $"{Path.GetFileName(pathFile).Replace(".pdf", "")}*.*", SearchOption.TopDirectoryOnly).ToList();
            fileContents.CountFiles = files;
            foreach (var file in files)
            {
                byte[] bytes = file.Select(x => (byte)(x)).ToArray();
                BitmapImage image = new BitmapImage();
                var imageSource = ToImageSource(image, bytes, file);
                fileContents.Pages.Add(imageSource);
                File.Delete(file);
            }
            return fileContents;
        }
        private static BitmapSource ToImageSource(BitmapImage image, byte[] fileBody, string pathFile)
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
                image.Rotation = Rotation.Rotate90;
                image.EndInit();                
            }
            return image;
        }
        private void KillPng()
        {
            var pp = Process.GetProcessesByName("Microsoft.Photos");
            pp[0].Kill();
        }
    }
}
