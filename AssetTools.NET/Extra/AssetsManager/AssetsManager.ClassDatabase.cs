using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssetsTools.NET.Extra
{
    public partial class AssetsManager
    {
        public ClassDatabaseFile LoadClassDatabase(Stream stream)
        {
            classDatabase = new ClassDatabaseFile();
            classDatabase.Read(new AssetsFileReader(stream));
            return classDatabase;
        }

        public ClassDatabaseFile LoadClassDatabase(string path)
        {
            return LoadClassDatabase(File.OpenRead(path));
        }

        public ClassDatabaseFile LoadClassDatabaseFromPackage(UnityVersion version)
        {
            return classDatabase = classPackage.GetClassDatabase(version);
        }

        public ClassDatabaseFile LoadClassDatabaseFromPackage(string version)
        {
            return classDatabase = classPackage.GetClassDatabase(version);
        }

        public ClassPackageFile LoadClassPackage(Stream stream)
        {
            classPackage = new ClassPackageFile();
            classPackage.Read(new AssetsFileReader(stream));
            return classPackage;
        }

        public ClassPackageFile LoadClassPackage(string path)
        {
            return LoadClassPackage(File.OpenRead(path));
        }
    }
}
