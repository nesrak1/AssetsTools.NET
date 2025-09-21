using AssetsTools.NET.Extra;
using AssetsTools.NET.Texture;
using NUnit.Framework;
using System;
using System.IO;

namespace AssetsTools.NET.Tests
{
    public class TextureTests
    {
        private static TextureFormat[] formats = {
            TextureFormat.Alpha8, // OK
            TextureFormat.ARGB4444, // OK
            TextureFormat.RGB24, // OK
            TextureFormat.RGBA32, // OK
            TextureFormat.ARGB32, // BAD: flipped R/B
            TextureFormat.ARGBFloat, // ERROR
            TextureFormat.RGB565, // OK
            TextureFormat.BGR24, // ERROR (uabea doesn't support these)
            TextureFormat.R16, // OK
            TextureFormat.DXT1, // OK
            TextureFormat.DXT3, // ERROR (uabea doesn't support these)
            TextureFormat.DXT5, // OK
            TextureFormat.RGBA4444, // OK
            TextureFormat.BGRA32, // OK
            TextureFormat.RHalf, // OK: all R
            TextureFormat.RGHalf, // OK: all RG
            TextureFormat.RGBAHalf, // OK
            TextureFormat.RFloat, // OK: all R
            TextureFormat.RGFloat, // OK: all RG
            TextureFormat.RGBAFloat, // OK
            TextureFormat.RGB9e5Float, // OK
            TextureFormat.RGBFloat, // ERROR (uabea doesn't support these)
            TextureFormat.BC6H, // OK
            TextureFormat.BC7, // OK
            TextureFormat.BC4, // OK: all R
            TextureFormat.BC5, // OK: all RG
            TextureFormat.DXT1Crunched, // not supported
            TextureFormat.DXT5Crunched, // not supported
            TextureFormat.PVRTC_RGB2, // OK
            TextureFormat.PVRTC_RGBA2, // OK
            TextureFormat.PVRTC_RGB4, // OK
            TextureFormat.PVRTC_RGBA4, // OK
            TextureFormat.ETC_RGB4, // OK
            TextureFormat.BGRA32Old, // ERROR (uabea doesn't support these)
            TextureFormat.EAC_R, // OK: all R
            TextureFormat.EAC_R_SIGNED, // OK (uabea is wrong)
            TextureFormat.EAC_RG, // OK: all RG
            TextureFormat.EAC_RG_SIGNED, // OK (uabea is wrong)
            TextureFormat.ETC2_RGB4, // OK
            TextureFormat.ETC2_RGBA1, // OK
            TextureFormat.ETC2_RGBA8, // OK
            TextureFormat.ASTC_RGB_4x4, // OK
            TextureFormat.ASTC_RGB_5x5, // OK
            TextureFormat.ASTC_RGB_6x6, // OK
            TextureFormat.ASTC_RGB_8x8, // OK
            TextureFormat.ASTC_RGB_10x10, // OK
            TextureFormat.ASTC_RGB_12x12, // OK
            TextureFormat.ASTC_RGBA_4x4, // OK
            TextureFormat.ASTC_RGBA_5x5, // OK
            TextureFormat.ASTC_RGBA_6x6, // OK
            TextureFormat.ASTC_RGBA_8x8, // OK
            TextureFormat.ASTC_RGBA_10x10, // OK
            TextureFormat.ASTC_RGBA_12x12, // OK
            TextureFormat.ETC_RGB4_3DS, // ERROR (uabea doesn't support these)
            TextureFormat.ETC_RGBA8_3DS, // ERROR (uabea doesn't support these)
            TextureFormat.RG16, // OK: all R
            TextureFormat.R8, // OK: all R
            TextureFormat.ETC_RGB4Crunched, // not supported
            TextureFormat.ETC2_RGBA8Crunched, // not supported
            TextureFormat.ASTC_HDR_4x4, // ERROR
            TextureFormat.ASTC_HDR_5x5, // ERROR
            TextureFormat.ASTC_HDR_6x6, // ERROR
            TextureFormat.ASTC_HDR_8x8, // ERROR
            TextureFormat.ASTC_HDR_10x10, // ERROR
            TextureFormat.ASTC_HDR_12x12, // ERROR
            TextureFormat.RG32, // ERROR
            TextureFormat.RGB48, // ERROR
            TextureFormat.RGBA64, // OK
        };

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestNativeWrite()
        {
            var level = 3;
            foreach (var format in formats)
            {
                try
                {
                    Console.WriteLine($"encoding {format} on level {level}...");

                    var am = new AssetsManager();
                    var bunInst = am.LoadBundleFile("texture2d_template.unity3d", true);
                    var fileInst = am.LoadAssetsFileFromBundle(bunInst, "CAB-e04b523b4a0e5464085a3a5edb214dd2");
                    var bun = bunInst.file;
                    var file = fileInst.file;

                    var texInf = file.GetAssetsOfType(AssetClassID.Texture2D)[0];
                    var texBf = am.GetBaseField(fileInst, texInf);
                    var tex = TextureFile.ReadTextureFile(texBf);
                    tex.m_TextureFormat = (int)format;
                    tex.EncodeTextureImage("test4096.png", level);
                    tex.WriteTo(texBf);
                    texInf.SetNewData(texBf);

                    bun.BlockAndDirInfo.DirectoryInfos[0].SetNewData(file);
                    bun.BlockAndDirInfo.DirectoryInfos[0].Name = $"CAB-texture-{format}";

                    Console.WriteLine($"compressing bundle for {format}...");
                    var writer = new AssetsFileWriter($"newimg_{format}_.unity3d");
                    bun.Write(writer);
                    writer.Close();

                    am.UnloadAll();

                    var newWriter = new AssetsFileWriter($"newimg_{format}.unity3d");
                    var newBun = am.LoadBundleFile($"newimg_{format}_.unity3d");
                    newBun.file.Pack(newWriter, AssetBundleCompressionType.LZ4);
                    newWriter.Close();
                    am.UnloadAll();
                    Console.WriteLine($"successfully bundled.");

                    File.Delete($"newimg_{format}_.unity3d");
                }
                catch (Exception e)
                {
                    Assert.Fail($"failed to encode {format}: {e.Message}");
                }
            }

            Assert.Pass();
        }
    }
}
