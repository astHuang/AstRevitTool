using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstRevitTool.Core.Export
{
    public interface IRevitMesh
    {
        void ComputeMesh(MeshInfo mi);

        void MergeMesh(
          MeshInfo meshInfo,
          MaterialInfo matInfo,
          bool isBack,
          ref int verticesOfs,
          ref int indicesOfs);

        int GetVerticesCount();

        int GetIndicesCount();
      
    }
}
