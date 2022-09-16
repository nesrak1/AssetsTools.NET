using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssetsTools.NET.Cpp2IL
{
    public static class FindCpp2IlFiles
    {
        public static FindCpp2IlFilesResult Find(string fileDir)
        {
            // search windows/linux
            string desktopMetaPath = Path.Combine(fileDir, "il2cpp_data", "Metadata", "global-metadata.dat");
            string windowsAsmPath = Path.Combine(fileDir, "..", "GameAssembly.dll");
            string linuxAsmPath = Path.Combine(fileDir, "..", "GameAssembly.so");

            // android
            string androidMetaPath = Path.Combine(fileDir, "Managed", "Metadata", "global-metadata.dat");
            string androidAsmDir = Path.Combine(fileDir, "..", "..", "..", "lib");
            string androidAsmPathX86 = Path.Combine(androidAsmDir, "x86", "libil2cpp.so");
            string androidAsmPathX8664 = Path.Combine(androidAsmDir, "x86_64", "libil2cpp.so");
            string androidAsmPathArm64 = Path.Combine(androidAsmDir, "arm64-v8a", "libil2cpp.so");
            string androidAsmPathArm32 = Path.Combine(androidAsmDir, "armeabi-v7a", "libil2cpp.so");

            if (File.Exists(desktopMetaPath))
            {
                if (File.Exists(windowsAsmPath))
                {
                    return new FindCpp2IlFilesResult(desktopMetaPath, windowsAsmPath);
                }
                else if (File.Exists(linuxAsmPath))
                {
                    return new FindCpp2IlFilesResult(desktopMetaPath, linuxAsmPath);
                }
            }
            else if (File.Exists(androidMetaPath))
            {
                if (File.Exists(androidAsmPathX86))
                {
                    return new FindCpp2IlFilesResult(androidMetaPath, androidAsmPathX86);
                }
                else if (File.Exists(androidAsmPathX8664))
                {
                    return new FindCpp2IlFilesResult(androidMetaPath, androidAsmPathX8664);
                }
                else if (File.Exists(androidAsmPathArm64))
                {
                    return new FindCpp2IlFilesResult(androidMetaPath, androidAsmPathArm64);
                }
                else if (File.Exists(androidAsmPathArm32))
                {
                    return new FindCpp2IlFilesResult(androidMetaPath, androidAsmPathArm32);
                }
            }

            return new FindCpp2IlFilesResult(false);
        }
    }

    public struct FindCpp2IlFilesResult
    {
        public string metaPath;
        public string asmPath;
        public bool success;

        public FindCpp2IlFilesResult(bool success)
        {
            metaPath = null;
            asmPath = null;
            this.success = success;
        }

        public FindCpp2IlFilesResult(string metaPath, string asmPath)
        {
            this.metaPath = metaPath;
            this.asmPath = asmPath;
            success = true;
        }
    }
}
