using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using KRUS_DiffPdf.Content;
using System.IO;
using System.Linq;
using System;
using MahApps.Metro.Controls;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace KRUS_DiffPdf
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static FileContents contentsFileOne = new FileContents();
        public static FileContents contentsFileTwo = new FileContents();        
        public int numberPage { get; set; } = 1;
        public NumberFiles enumFiles;
        public MainWindow()
        {
            InitializeComponent();
        }        
        private void ButtonTest_Click(object sender, RoutedEventArgs e)
        {
            Errors();
        }
        private void ButtonChooseFileOne_Click(object sender, RoutedEventArgs e)
        {
            var convert = new ReadDocuments.ConverPDFtoPNG();
            var filePath = ChooseFile();
            TextBoxPathFileOne.Text = filePath;
            ShowProgressVisbility();
            Thread thread;
            thread = new Thread(()=> 
            {
                try
                {
                    contentsFileOne = convert.ReadFilePdfCreator(filePath);                    
                }
                finally
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        ImageWaitingOne.Visibility = Visibility.Hidden;
                        ImageDoc1.Source = contentsFileOne.Pages[numberPage];                       
                        TextBoxNumber();
                    }));
                }
            });
            thread.Start();
        }
        private void ButtonChooseFileTwo_Click(object sender, RoutedEventArgs e)
        {
            var convert = new ReadDocuments.ConverPDFtoPNG();
            var filePath = ChooseFile();
            TextBoxPathFileTwo.Text = filePath;
            contentsFileTwo = convert.ReadFilePdfCreator(filePath);
            ImageDoc2.Source = contentsFileTwo.Pages[numberPage];
            TextBoxNumber();
        }
        private string ChooseFile()
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
                return dialog.FileName;
            }
            return "";
        }
        private void ButtonDown_Click(object sender, RoutedEventArgs e)
        {
            numberPage--;
            if (numberPage < 1)
            {
                numberPage = 1;
            }
            if (numberPage <= contentsFileOne.Pages.Count - 1)
            {
                ImageDoc1.Source = contentsFileOne.Pages[numberPage];
            }
            if (numberPage <= contentsFileTwo.Pages.Count - 1)
            {
                ImageDoc2.Source = contentsFileTwo.Pages[numberPage];
            }
            TextBoxNumber();
        }
        private void ButtonUp_Click(object sender, RoutedEventArgs e)
        {
            numberPage++;            
            var maxCountPage = contentsFileOne.Pages.Count > contentsFileTwo.Pages.Count ? contentsFileOne.Pages.Count : contentsFileTwo.Pages.Count;
            if (numberPage >= maxCountPage)
            {
                numberPage = maxCountPage - 1;
            }
            if (numberPage <= contentsFileOne.Pages.Count - 1)
            {
                ImageDoc1.Source = contentsFileOne.Pages[numberPage];
            }
            if (numberPage <= contentsFileTwo.Pages.Count - 1)
            {
                ImageDoc2.Source = contentsFileTwo.Pages[numberPage];
            }
            TextBoxNumber();
        }
        private void ScrollChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
        {
            if (sender == ScrollViewerOne)
            {
                ScrollViewerTwo.ScrollToVerticalOffset(e.VerticalOffset);
                ScrollViewerTwo.ScrollToHorizontalOffset(e.HorizontalOffset);
            }
            else
            {
                ScrollViewerOne.ScrollToVerticalOffset(e.VerticalOffset);
                ScrollViewerOne.ScrollToHorizontalOffset(e.HorizontalOffset);
            }
        }
        private void ShowProgressVisbility()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                ImageWaitingOne.Visibility = Visibility.Visible;
            }));
            Thread.Sleep(1000);
        }
        private void TextBoxNumber()
        {
            TextBoxPageNumber.Text = numberPage.ToString();
        }
        private void Errors()
        {
            if (TextBoxPathFileOne.Text == string.Empty || TextBoxPathFileTwo.Text == string.Empty)
            {
                MessageBox.Show("Выберите файл(ы).", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }       
    }
}