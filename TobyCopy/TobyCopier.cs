using AssetsTools.NET;
using AssetsTools.NET.Extra;

namespace TobyCopy
{
    public class TobyCopier
    {
        private readonly AssetsManager _dstMan;
        private readonly AssetsManager _srcMan;

        public TobyCopier(AssetsManager dstMan, AssetsManager srcMan)
        {
            _dstMan = dstMan;
            _srcMan = srcMan;
        }

        public void Copy(
            AssetsFileInstance srcFile, AssetFileInfo srcInfo,
            AssetsFileInstance dstFile)
        {
            Copy(srcFile, new List<AssetFileInfo> { srcInfo }, _ => dstFile);
        }

        public void Copy(
            AssetsFileInstance srcFile, AssetFileInfo srcInfo,
            Func<AssetFileInfo, AssetsFileInstance> dstSelector)
        {
            Copy(srcFile, new List<AssetFileInfo> { srcInfo }, dstSelector);
        }

        public void Copy(
            AssetsFileInstance srcFile, List<AssetFileInfo> srcInfos,
            AssetsFileInstance dstFile)
        {
            Copy(srcFile, srcInfos, _ => dstFile);
        }

        public void Copy(
            AssetsFileInstance srcFile, List<AssetFileInfo> srcInfos,
            Func<AssetFileInfo, AssetsFileInstance> dstSelector)
        {
            var crawler = new DependencyCrawler(_srcMan, srcFile, false);
            foreach (var inf in srcInfos)
            {
                crawler.Crawl(inf);
            }

            var orderedItems = crawler.PathIdMap.OrderBy(p => p.Value);
            Console.WriteLine("mappings: ");
            foreach (var item in orderedItems)
            {
                Console.WriteLine($"  {item.Key.FilePath}/{item.Key.PathId} -> {item.Value}");
                var itemFile = _srcMan.LoadAssetsFile(null, item.Key.FilePath);
                var itemInf = itemFile.file.GetAssetInfo(item.Key.PathId);
                var typeTreeType = itemFile.file.Metadata.FindTypeTreeTypeByID(itemInf.TypeId, itemInf.GetScriptIndex(itemFile.file));
                var dstFile = dstSelector(itemInf);
                var copiedType = TypeTreeCopier.Copy(dstFile.file, typeTreeType, _srcMan, srcFile);
                if (copiedType != null)
                {
                    Console.WriteLine($"  Copied type for this asset: {(AssetClassID)copiedType.TypeId}");
                }
            }

            var orderedRess = crawler.RessMap.OrderBy(p => p.Value);
            Console.WriteLine("ress mappings: ");
            foreach (var item in orderedRess)
            {
                Console.WriteLine($"  0x{item.Key.start:x8}+->0x{item.Key.length:x8} -> 0x{item.Value:x8}");
            }

            // todo: move this above
            foreach (var item in orderedItems)
            {
                var itemFile = _srcMan.LoadAssetsFile(null, item.Key.FilePath);
                var itemInf = itemFile.file.GetAssetInfo(item.Key.PathId);
                var dstFile = dstSelector(itemInf);

                // will update monobehaviour id later
                var newInf = AssetFileInfo.Create(dstFile.file, item.Value, itemInf.TypeId);
                dstFile.file.Metadata.AddAssetInfo(newInf);
            }
        }
    }
}