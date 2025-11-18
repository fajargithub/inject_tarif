using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InjectServiceWorker.Services
{
    public static class NetworkPaths
    {
        public const string sourcePath = @"D:\INJECT_TARIF";
        public const string donePath = @"D:\INJECT_TARIF\Done";
        public const string invalidPath = @"D:\INJECT_TARIF\Invalid";
        public const string errorPath = @"D:\INJECT_TARIF\Error";

        //public const string sourcePath2 = @"D:\INJECT_TARIF_NEW";
        //public const string donePath2 = @"D:\INJECT_TARIF_NEW\Done";
        //public const string invalidPath2 = @"D:\INJECT_TARIF_NEW\Invalid";
        //public const string errorPath2 = @"D:\INJECT_TARIF_NEW\Error";

        public const string sourcePath2 = @"\\cluster-nas\FTP\NON FTP\\UPLOAD_TARIF";
        public const string donePath2 = @"\\cluster-nas\FTP\NON FTP\\UPLOAD_TARIF\Done";
        public const string invalidPath2 = @"\\cluster-nas\FTP\NON FTP\\UPLOAD_TARIF\Invalid";
        public const string errorPath2 = @"\\cluster-nas\FTP\NON FTP\\UPLOAD_TARIF\Error";
    }
}
