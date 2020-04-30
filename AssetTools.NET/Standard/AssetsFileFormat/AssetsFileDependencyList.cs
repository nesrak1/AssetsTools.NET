using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetsTools.NET
{
    public class AssetsFileDependencyList
    {
        public int dependencyCount;
        public List<AssetsFileDependency> dependencies;
        public void Read(AssetsFileReader reader)
        {
            dependencyCount = reader.ReadInt32();
            dependencies = new List<AssetsFileDependency>();
            for (int i = 0; i < dependencyCount; i++)
            {
                AssetsFileDependency dependency = new AssetsFileDependency();
                dependency.Read(reader);
                dependencies.Add(dependency);
            }
        }
        public void Write(AssetsFileWriter writer)
        {
            writer.Write(dependencyCount);
            for (int i = 0; i < dependencyCount; i++)
            {
                dependencies[i].Write(writer);
            }
        }
    }
}
