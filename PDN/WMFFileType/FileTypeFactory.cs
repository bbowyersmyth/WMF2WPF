using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PaintDotNet;

namespace WMFFileType
{
    public class FileTypeFactory : IFileTypeFactory
    {
        public FileType[] GetFileTypeInstances()
        {
            return new FileType[] 
            { 
                new WMFFileType()
            };
        }
    }
}
