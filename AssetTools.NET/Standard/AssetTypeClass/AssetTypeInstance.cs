namespace AssetsTools.NET
{
    public class AssetTypeInstance
    {
        public int baseFieldCount;
        public AssetTypeValueField[] baseFields;
        public uint allocationCount; public uint allocationBufLen;
        public byte[] memoryToClear;
        public AssetTypeInstance(AssetTypeTemplateField[] baseFields, AssetsFileReader reader, long filePos)
        {
            reader.bigEndian = false;
            reader.Position = filePos;
            this.baseFieldCount = baseFields.Length;
            this.baseFields = new AssetTypeValueField[baseFieldCount];
            for (int i = 0; i < baseFieldCount; i++)
            {
                AssetTypeTemplateField templateBaseField = baseFields[i];
                AssetTypeValueField atvf;
                templateBaseField.MakeValue(reader, out atvf);
                this.baseFields[i] = atvf;
            }
        }
        public AssetTypeInstance(AssetTypeTemplateField baseField, AssetsFileReader reader, long filePos)
            : this(new[] { baseField }, reader, filePos) { }
        ///public bool SetChildList(AssetTypeValueField valueField, AssetTypeValueField[] childrenList, uint childrenCount, bool freeMemory = true);
        ///public bool AddTempMemory(byte[] memory);

        public static AssetTypeValueField GetDummyAssetTypeField()
        {
            AssetTypeValueField atvf = new AssetTypeValueField();
            atvf.childrenCount = 0xFF;
            return atvf;
        }

        public AssetTypeValueField GetBaseField(int index = 0)
        {
            if (index >= baseFieldCount)
                return GetDummyAssetTypeField();
            return baseFields[index];
        }
    }
}
