using System.IO;

namespace AssetsTools.NET.Extra
{
    public partial class AssetsManager
    {
        public ClassDatabaseFile LoadClassDatabase(Stream stream)
        {
            ClassDatabase = new ClassDatabaseFile();
            ClassDatabase.Read(new AssetsFileReader(stream));
            return ClassDatabase;
        }

        public ClassDatabaseFile LoadClassDatabase(string path)
        {
            return LoadClassDatabase(File.OpenRead(path));
        }

        public ClassDatabaseFile LoadClassDatabaseFromPackage(UnityVersion version)
        {
            return ClassDatabase = ClassPackage.GetClassDatabase(version);
        }

        public ClassDatabaseFile LoadClassDatabaseFromPackage(string version)
        {
            return ClassDatabase = ClassPackage.GetClassDatabase(version);
        }

        public ClassPackageFile LoadClassPackage(Stream stream)
        {
            ClassPackage = new ClassPackageFile();
            ClassPackage.Read(new AssetsFileReader(stream));
            return ClassPackage;
        }

        public ClassPackageFile LoadClassPackage(string path)
        {
            return LoadClassPackage(File.OpenRead(path));
        }

        public void UnloadClassDatabase()
        {
            ClassDatabase = null;
        }

        public void UnloadClassPackage()
        {
            ClassPackage = null;
        }
    }
}
