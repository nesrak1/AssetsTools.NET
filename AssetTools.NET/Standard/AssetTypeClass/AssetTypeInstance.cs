namespace AssetsTools.NET
{
    public class AssetTypeInstance
    {
        public uint baseFieldCount;
        public AssetTypeValueField[] baseFields;
        public uint allocationCount; public uint allocationBufLen;
        public byte[] memoryToClear;
        public AssetTypeInstance(uint baseFieldCount, AssetTypeTemplateField[] ppBaseFields, AssetsFileReader reader, bool bigEndian, ulong filePos = 0)
        {
            this.baseFieldCount = baseFieldCount;
            reader.bigEndian = false;
            reader.BaseStream.Position = (long)filePos;
            baseFields = new AssetTypeValueField[this.baseFieldCount];
            for (int i = 0; i < baseFieldCount; i++)
            {
                //Debug.WriteLine(reader.BaseStream.Position);
                AssetTypeTemplateField templateBaseField = ppBaseFields[i];
                AssetTypeValueField atvf;
                templateBaseField.MakeValue(reader, reader.Position, out atvf, reader.bigEndian);
                //atvf.Read(atvf.value, templateBaseField, atvf.childrenCount, atvf.pChildren);
                baseFields[i] = atvf;
            }
        }
        ///public bool SetChildList(AssetTypeValueField pValueField, AssetTypeValueField[] pChildrenList, uint childrenCount, bool freeMemory = true);
        ///public bool AddTempMemory(byte[] pMemory);

        public static AssetTypeValueField GetDummyAssetTypeField()
        {
            AssetTypeValueField atvf = new AssetTypeValueField();
            atvf.childrenCount = 0xFF;
            return atvf;
        }

        public AssetTypeValueField GetBaseField(uint index = 0)
        {
            if (index >= baseFieldCount)
                return GetDummyAssetTypeField();
            return baseFields[index];
        }
    }
}
