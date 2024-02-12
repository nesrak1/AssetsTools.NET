using AssetsTools.NET;
using AssetsTools.NET.Extra;

namespace TobyCopy
{
    internal class DependencyCrawler
    {
        private readonly AssetsManager _man;
        private readonly AssetsFileInstance _file;
        private readonly bool _randomIds;

        public readonly Dictionary<AssetPPtr, long> PathIdMap;
        public readonly Dictionary<ResourceRange, long> RessMap;
        public readonly Dictionary<ResourceRange, long> ResourceMap;

        private readonly Random _rand;
        private readonly HashSet<long> _usedPids;

        private long currentPathId = 1;
        private long currentRessOffset = 0;
        private long currentResourceOffset = 0;

        public DependencyCrawler(AssetsManager man, AssetsFileInstance file, bool randomIds)
        {
            _man = man;
            _file = file;
            _randomIds = randomIds;

            PathIdMap = new();
            RessMap = new();
            ResourceMap = new();

            _rand = new();
            _usedPids = new();

            if (!randomIds)
            {
                _usedPids = new();
                foreach (var inf in _file.file.AssetInfos)
                {
                    _usedPids.Add(inf.PathId);
                }

                currentPathId = 1;
            }
            else
            {
                _rand = new Random();
                currentPathId = _rand.NextInt64();
            }
            GetNewPathId();
        }

        public void Crawl(AssetFileInfo inf)
        {
            Crawl(_file, inf);
        }

        // we assume we haven't crawled this asset yet
        public void Crawl(AssetsFileInstance file, AssetFileInfo inf, int depth = 0)
        {
            Console.WriteLine($"{new string(' ', depth * 2)}{file.name} -> {(AssetClassID)inf.TypeId} {inf.PathId}");

            var baseField = _man.GetBaseField(file, inf);
            var pptr = new AssetPPtr(file.path, inf.PathId);
            PathIdMap[pptr] = GetNewPathId();

            CheckResources(inf, baseField);
            Crawl(file, baseField, depth + 1);
        }

        private void CheckResources(AssetFileInfo inf, AssetTypeValueField field)
        {
            var assetTypeId = (AssetClassID)inf.TypeId;
            if (assetTypeId == AssetClassID.Texture2D || assetTypeId == AssetClassID.Mesh)
            {
                var streamData = field["m_StreamData"];
                if (!streamData.IsDummy)
                {
                    var size = streamData["size"].AsLong;
                    if (size == 0)
                    {
                        return;
                    }

                    var offset = streamData["offset"].AsLong;

                    var range = new ResourceRange(offset, size);
                    if (!RessMap.ContainsKey(range))
                    {
                        RessMap[range] = GetNewRessMapOffset(size);
                    }
                }
            }
            else if (assetTypeId == AssetClassID.AudioClip)
            {
                var resource = field["m_Resource"];
                if (!resource.IsDummy)
                {
                    var size = resource["m_Size"].AsLong;
                    if (size == 0)
                    {
                        return;
                    }

                    var offset = resource["m_Offset"].AsLong;

                    var range = new ResourceRange(offset, size);
                    if (!ResourceMap.ContainsKey(range))
                    {
                        ResourceMap[range] = GetNewResourceMapOffset(size);
                    }
                }
            }
        }

        private void Crawl(AssetsFileInstance file, AssetTypeValueField field, int depth)
        {
            foreach (var child in field)
            {
                var tempField = child.TemplateField;

                if (tempField.HasValue && !tempField.IsArray)
                    continue;

                if (tempField.IsArray && tempField.Children[1].ValueType != AssetValueType.None)
                    continue;

                var typeName = tempField.Type;
                if (typeName.StartsWith("PPtr<") && typeName.EndsWith(">"))
                {
                    var fileId = child["m_FileID"].AsInt;
                    var pathId = child["m_PathID"].AsLong;

                    if (pathId == 0)
                        continue;

                    var ext = _man.GetExtAsset(file, fileId, pathId, true);
                    var pptr = new AssetPPtr(ext.file.path, ext.info.PathId);

                    if (PathIdMap.ContainsKey(pptr))
                        continue;

                    ext = _man.GetExtAsset(file, fileId, pathId, false);

                    Crawl(ext.file, ext.info, depth);
                }
                else
                {
                    Crawl(file, child, depth);
                }
            }
        }

        private long GetNewRessMapOffset(long currentLength)
        {
            var oldOffset = currentRessOffset;
            currentRessOffset += currentLength;
            return oldOffset;
        }

        private long GetNewResourceMapOffset(long currentLength)
        {
            var oldOffset = currentResourceOffset;
            currentResourceOffset += currentLength;
            return oldOffset;
        }

        private long GetNewPathId()
        {
            if (_randomIds)
            {
                while (_usedPids.Contains(currentPathId))
                {
                    currentPathId = _rand.NextInt64();
                }
                _usedPids.Add(currentPathId);
                return currentPathId;
            }
            else
            {
                while (_usedPids.Contains(currentPathId))
                {
                    currentPathId++;
                }
                _usedPids.Add(currentPathId);
                return currentPathId;
            }
        }
    }
}
