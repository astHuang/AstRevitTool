using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AstRevitTool.Core.Export
{
    public class MeshInfo
    {
        public ulong materialUID = ulong.MaxValue;
        public MaterialInfo matInfo;
        public bool isDoubleSided;
        public IRevitMesh revitMesh;
        public int numberOfVertices;
        public int numberOfIndices;
        public int numberOfUVSets;
        public int[] indices;
        public float[] vertices;
        public float[] normals;
        public float[] uvs;

        public int GetNumberOfVertices() => this.isDoubleSided ? this.numberOfVertices * 2 : this.numberOfVertices;

        public int GetNumberOfIndices() => this.isDoubleSided ? this.numberOfIndices * 2 : this.numberOfIndices;

        public MeshInfo(int nVertices = 0, int nIndices = 0, int nUVSets = 1)
        {
            this.numberOfVertices = nVertices;
            this.numberOfIndices = nIndices;
            this.numberOfUVSets = nUVSets < 1 ? 1 : nUVSets;
            if (nVertices != 0)
            {
                this.vertices = new float[nVertices * 3];
                this.normals = new float[nVertices * 3];
                this.uvs = new float[nVertices * 2 * this.numberOfUVSets];
            }
            if (nIndices == 0)
                return;
            this.indices = new int[nIndices];
        }

        public void Resize(int nVertices = 0, int nIndices = 0, int nUVSets = 1)
        {
            this.numberOfVertices = nVertices;
            this.numberOfIndices = nIndices;
            this.numberOfUVSets = nUVSets < 1 ? 1 : nUVSets;
            if (nVertices != 0)
            {
                this.vertices = new float[nVertices * 3];
                this.normals = new float[nVertices * 3];
                this.uvs = new float[nVertices * 2 * this.numberOfUVSets];
            }
            if (nIndices == 0)
                return;
            this.indices = new int[nIndices];
        }
    }
}
