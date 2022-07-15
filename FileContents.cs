using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace KRUS_DiffPdf.Content
{
    public class FileContents
    {        
        public string FilePath { get; set; }
        public FileContents()
        {
            Pages = new List<BitmapSource>();
        }
        public byte[] FileBody { get; set; }
        public List<BitmapSource> Pages { get; set; }
        public List<string> CountFiles { get; set; }
    }
}
