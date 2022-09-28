using NUnit.Framework;
using System.IO;

namespace AssetsTools.NET.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestTpkWriteRead()
        {
            ClassPackageFile tpk = new ClassPackageFile();
            tpk.Read("tests.tpk");

            var cldb = tpk.GetClassDatabase("2019.4.30f1"); // some random version

            var cldbWriteStream1 = new MemoryStream();
            var cldbWriteStream2 = new MemoryStream();
            cldb.Write(new AssetsFileWriter(cldbWriteStream1), ClassFileCompressionType.Lz4);

            cldb = new ClassDatabaseFile();
            cldbWriteStream1.Position = 0;
            cldb.Read(new AssetsFileReader(cldbWriteStream1));
            cldb.Write(new AssetsFileWriter(cldbWriteStream2), ClassFileCompressionType.Lz4);

            string cldb1Sha = Util.GetHashSHA1(cldbWriteStream1.ToArray());
            string cldb2Sha = Util.GetHashSHA1(cldbWriteStream2.ToArray());
            Assert.IsTrue(cldb1Sha == cldb2Sha);

            Assert.Pass();
        }
    }
}