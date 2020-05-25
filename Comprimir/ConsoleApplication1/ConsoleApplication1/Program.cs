using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.IO;
//using System.IO.Compression;
using Ionic.Zip;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            //string startPath = @"D:\Com";
            //string zipPath = @"D:\Com\result.zip";
            //string extractPath = @"D:\Com\extract";
            //ZipFile.CreateFromDirectory(startPath, zipPath);
            //ZipFile.ExtractToDirectory(zipPath, extractPath);

            //using (ZipFile loanZip = new ZipFile())
            //{
            //    loanZip.AddFiles(documentPaths, false, zipDestinationPath);
            //    loanZip.Save(string.Format("{0}{1}.zip", zipDestinationPath, documentIdentifier.ToString()));
            //    zipped = true;
            //}

            ZipFile zip = new ZipFile();
            zip.AddFile(@"D:\Com\Imagen2.png", "");
            zip.Save(@"D:\Com\archivo.zip");
        }
    
         
        public void comprimir()
        {
            
        }

    }
}
