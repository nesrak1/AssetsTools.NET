using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetsTools.NET
{
    public struct AssetsFileDependencyList
    {
        public uint dependencyCount;
        //BYTE unknown; //seemingly always 0
        public AssetsFileDependency[] pDependencies;
        public ulong Read(ulong absFilePos, AssetsFileReader reader, uint format, bool bigEndian)
        {
            dependencyCount = reader.ReadUInt32();
            pDependencies = new AssetsFileDependency[dependencyCount];
            for (int i = 0; i < dependencyCount; i++)
            {
                AssetsFileDependency dependency = new AssetsFileDependency();
                dependency.Read(reader.Position, reader, bigEndian);
                pDependencies[i] = dependency;
            }
            return reader.Position;
        }
        public ulong Write(ulong absFilePos, AssetsFileWriter writer, uint format)
        {
            writer.Write(dependencyCount);
            for (int i = 0; i < dependencyCount; i++)
            {
                pDependencies[i].Write(writer.Position, writer);
            }
            return writer.Position;
        }
    }
}
