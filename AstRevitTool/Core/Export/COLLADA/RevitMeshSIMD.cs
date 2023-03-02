using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace AstRevitTool.Core.Export
{
    public class RevitMeshSIMD : IRevitMesh
    {
        private int numberOfPoints;
        private int numberOfNormals;
        private int numberOfUVs;
        private int numberOfFacets;
        private int numberOfIndices;
        private bool isDoubleSided = false;
        private List<Vector3> points;
        private List<Vector3> normals;
        private List<UV> uvs;
        private List<int> indices;
        private List<int[]> facets;
        private Matrix4x4 transform = Matrix4x4.Identity;
        private Matrix4x4 normalTransform = Matrix4x4.Identity;
        private bool hasReflectionAndConformal;
        private DistributionOfNormals distributionOfNormals;

        protected static Matrix4x4 TransformToMatrix4x4(Transform tr, float transformScale = 0.3048f) => new Matrix4x4((float)tr.BasisX.X, (float)tr.BasisX.Y, (float)tr.BasisX.Z, 0.0f, (float)tr.BasisY.X, (float)tr.BasisY.Y, (float)tr.BasisY.Z, 0.0f, (float)tr.BasisZ.X, (float)tr.BasisZ.Y, (float)tr.BasisZ.Z, 0.0f, (float)tr.Origin.X * transformScale, (float)tr.Origin.Y * transformScale, (float)tr.Origin.Z * transformScale, 1f);

        public RevitMeshSIMD(Transform tr, PolymeshTopology polyMesh)
        {
            this.transform = RevitMeshSIMD.TransformToMatrix4x4(tr);
            this.normalTransform = RevitMeshSIMD.TransformToMatrix4x4(tr, 0.0f);
            this.hasReflectionAndConformal = tr.IsConformal && tr.HasReflection;
            this.distributionOfNormals = polyMesh.DistributionOfNormals;
            this.numberOfPoints = polyMesh.NumberOfPoints;
            this.numberOfNormals = polyMesh.NumberOfNormals;
            this.numberOfUVs = polyMesh.NumberOfUVs;
            this.numberOfFacets = polyMesh.NumberOfFacets;
            this.numberOfIndices = this.numberOfFacets * 3;
            this.indices = (List<int>)null;
            this.points = new List<Vector3>(this.numberOfPoints);
            this.normals = new List<Vector3>(this.numberOfNormals);
            this.uvs = new List<UV>(this.numberOfUVs);
            this.facets = new List<int[]>(this.numberOfFacets);
            foreach (XYZ point in (IEnumerable<XYZ>)polyMesh.GetPoints())
                this.points.Add(new Vector3((float)point.X * 0.3048f, (float)point.Y * 0.3048f, (float)point.Z * 0.3048f));
            foreach (XYZ normal in (IEnumerable<XYZ>)polyMesh.GetNormals())
                this.normals.Add(new Vector3((float)normal.X, (float)normal.Y, (float)normal.Z));
            foreach (UV uv in (IEnumerable<UV>)polyMesh.GetUVs())
                this.uvs.Add(new UV(uv.U, uv.V));
            foreach (PolymeshFacet facet in (IEnumerable<PolymeshFacet>)polyMesh.GetFacets())
                this.facets.Add(new int[3]
                {
          facet.V1,
          facet.V2,
          facet.V3
                });
            if ((int)this.distributionOfNormals != 2)
                return;
            this.numberOfPoints = this.numberOfFacets * 3;
        }

        public RevitMeshSIMD(Transform tr, Mesh mesh)
        {
            this.transform = RevitMeshSIMD.TransformToMatrix4x4(tr);
            this.normalTransform = RevitMeshSIMD.TransformToMatrix4x4(tr, 0.0f);
            this.hasReflectionAndConformal = tr.IsConformal && tr.HasReflection;
            this.numberOfPoints = ((ICollection<XYZ>)mesh.Vertices).Count;
            this.numberOfIndices = mesh.NumTriangles * 3;
            this.points = new List<Vector3>(this.numberOfPoints);
            this.indices = new List<int>(this.numberOfIndices);
            this.normals = (List<Vector3>)null;
            this.uvs = (List<UV>)null;
            this.distributionOfNormals = (DistributionOfNormals)0;
            foreach (XYZ vertex in (IEnumerable<XYZ>)mesh.Vertices)
                this.points.Add(new Vector3((float)vertex.X * 0.3048f, (float)vertex.Y * 0.3048f, (float)vertex.Z * 0.3048f));
            if (this.hasReflectionAndConformal)
            {
                for (int index = 0; index < mesh.NumTriangles; ++index)
                {
                    MeshTriangle meshTriangle = mesh.get_Triangle(index);
                    this.indices.Add((int)meshTriangle.get_Index(0));
                    this.indices.Add((int)meshTriangle.get_Index(2));
                    this.indices.Add((int)meshTriangle.get_Index(1));
                }
            }
            else
            {
                for (int index = 0; index < mesh.NumTriangles; ++index)
                {
                    MeshTriangle meshTriangle = mesh.get_Triangle(index);
                    this.indices.Add((int)meshTriangle.get_Index(0));
                    this.indices.Add((int)meshTriangle.get_Index(1));
                    this.indices.Add((int)meshTriangle.get_Index(2));
                }
            }
        }

        public RevitMeshSIMD(
          Transform tr,
          List<XYZ> meshPoints,
          List<int> meshIndices,
          List<XYZ> meshNormals = null,
          List<UV> meshUVs = null)
        {
            this.transform = RevitMeshSIMD.TransformToMatrix4x4(tr);
            this.normalTransform = RevitMeshSIMD.TransformToMatrix4x4(tr, 0.0f);
            this.hasReflectionAndConformal = tr.IsConformal && tr.HasReflection;
            this.distributionOfNormals = (DistributionOfNormals)0;
            this.numberOfPoints = meshPoints.Count;
            this.points = new List<Vector3>(this.numberOfPoints);
            foreach (XYZ meshPoint in meshPoints)
                this.points.Add(new Vector3((float)meshPoint.X * 0.3048f, (float)meshPoint.Y * 0.3048f, (float)meshPoint.Z * 0.3048f));
            if (meshNormals != null)
            {
                this.numberOfNormals = meshNormals != null ? meshNormals.Count : 0;
                this.normals = new List<Vector3>(this.numberOfNormals);
                foreach (XYZ meshNormal in meshNormals)
                    this.normals.Add(new Vector3((float)meshNormal.X * 0.3048f, (float)meshNormal.Y * 0.3048f, (float)meshNormal.Z * 0.3048f));
            }
            else
            {
                this.numberOfNormals = 0;
                this.normals = (List<Vector3>)null;
            }
            this.numberOfUVs = meshUVs != null ? meshUVs.Count : 0;
            this.uvs = meshUVs;
            this.numberOfIndices = meshIndices.Count;
            this.indices = meshIndices;
        }

        public bool GetDoubleSided() => this.isDoubleSided;

        public void SetDoubleSided(bool val) => this.isDoubleSided = val;

        public int GetVerticesCount() => this.isDoubleSided ? this.numberOfPoints * 2 : this.numberOfPoints;

        public int GetIndicesCount() => this.isDoubleSided ? this.numberOfIndices * 2 : this.numberOfIndices;

        public void ComputeMesh(MeshInfo meshInfo)
        {
            if ((int)this.distributionOfNormals == 2)
                this.UnweldMeshPoints();
            if (this.normals == null)
                this.ComputeNormals(this.hasReflectionAndConformal);
            if (this.uvs == null)
                this.ComputeUVs();
            MaterialInfo matInfo = meshInfo.matInfo;
            this.numberOfIndices = this.indices != null ? this.indices.Count : this.numberOfFacets * 3;
            meshInfo.numberOfVertices = this.numberOfPoints;
            meshInfo.numberOfIndices = this.numberOfIndices;
            meshInfo.numberOfUVSets = matInfo.NeedSecondUV() ? 2 : 1;
            meshInfo.vertices = new float[this.numberOfPoints * 3];
            meshInfo.normals = new float[this.numberOfPoints * 3];
            meshInfo.uvs = new float[meshInfo.numberOfUVSets * (this.numberOfPoints * 2)];
            meshInfo.indices = new int[meshInfo.numberOfIndices];
            for (int index = 0; index < this.numberOfPoints; ++index)
            {
                Vector3 vector3 = Vector3.Transform(this.points[index], this.transform);
                meshInfo.vertices[index * 3] = vector3.X;
                meshInfo.vertices[index * 3 + 1] = vector3.Z + (meshInfo.isDoubleSided ? -1f / 1000f : 0.0f);
                meshInfo.vertices[index * 3 + 2] = vector3.Y;
            }
            switch ((int)this.distributionOfNormals)
            {
                case 0:
                    for (int index = 0; index < this.numberOfPoints; ++index)
                    {
                        Vector3 vector3 = Vector3.Transform(this.normals[index], this.normalTransform);
                        meshInfo.normals[index * 3] = vector3.X;
                        meshInfo.normals[index * 3 + 1] = vector3.Z;
                        meshInfo.normals[index * 3 + 2] = vector3.Y;
                    }
                    break;
                case 1:
                    Vector3 vector3_1 = Vector3.Transform(this.normals[0], this.normalTransform);
                    for (int index = 0; index < this.numberOfPoints; ++index)
                    {
                        meshInfo.normals[index * 3] = vector3_1.X;
                        meshInfo.normals[index * 3 + 1] = vector3_1.Z;
                        meshInfo.normals[index * 3 + 2] = vector3_1.Y;
                    }
                    break;
                case 2:
                    for (int index1 = 0; index1 < this.numberOfFacets; ++index1)
                    {
                        Vector3 vector3_2 = Vector3.Transform(this.normals[index1], this.normalTransform);
                        for (int index2 = 0; index2 < 3; ++index2)
                        {
                            int num = this.facets[index1][index2];
                            meshInfo.normals[num * 3] = vector3_2.X;
                            meshInfo.normals[num * 3 + 1] = vector3_2.Z;
                            meshInfo.normals[num * 3 + 2] = vector3_2.Y;
                        }
                    }
                    break;
            }
            double rotationAngle1 = (double)matInfo.ColorTexture.RotationAngle;
            double offsetU1 = (double)matInfo.ColorTexture.OffsetU;
            double offsetV1 = (double)matInfo.ColorTexture.OffsetV;
            for (int index = 0; index < this.numberOfPoints; ++index)
            {
                UV uv = this.uvs[index];
                double num1 = uv.U;
                double num2 = uv.V;
                if (Math.Abs(rotationAngle1) > 0.03)
                {
                    num1 = Math.Cos(rotationAngle1) * (uv.U - offsetU1) - Math.Sin(rotationAngle1) * (uv.V - offsetV1) + offsetU1;
                    num2 = Math.Sin(rotationAngle1) * (uv.U - offsetU1) + Math.Cos(rotationAngle1) * (uv.V - offsetV1) + offsetV1;
                }
                double num3 = (num1 - (double)matInfo.ColorTexture.OffsetU) * (double)matInfo.ColorTexture.ScaleU;
                double num4 = (num2 - (double)matInfo.ColorTexture.OffsetV) * (double)matInfo.ColorTexture.ScaleV;
                meshInfo.uvs[index * 2] = (float)num3;
                meshInfo.uvs[index * 2 + 1] = 1f - (float)num4;
            }
            if (meshInfo.numberOfUVSets == 2)
            {
                int num5 = this.numberOfPoints * 2;
                double rotationAngle2 = (double)matInfo.BumpTexture.RotationAngle;
                double offsetU2 = (double)matInfo.BumpTexture.OffsetU;
                double offsetV2 = (double)matInfo.BumpTexture.OffsetV;
                for (int index = 0; index < this.numberOfPoints; ++index)
                {
                    UV uv = this.uvs[index];
                    double num6 = uv.U;
                    double num7 = uv.V;
                    if (Math.Abs(rotationAngle2) > 0.02)
                    {
                        num6 = Math.Cos(rotationAngle2) * (uv.U - offsetU2) - Math.Sin(rotationAngle2) * (uv.V - offsetV2) + offsetU2;
                        num7 = Math.Sin(rotationAngle2) * (uv.U - offsetU2) + Math.Cos(rotationAngle2) * (uv.V - offsetV2) + offsetV2;
                    }
                    double num8 = (num6 - (double)matInfo.BumpTexture.OffsetU) * (double)matInfo.BumpTexture.ScaleU;
                    double num9 = (num7 - (double)matInfo.BumpTexture.OffsetV) * (double)matInfo.BumpTexture.ScaleV;
                    meshInfo.uvs[num5 + index * 2] = (float)num8;
                    meshInfo.uvs[num5 + index * 2 + 1] = 1f - (float)num9;
                }
            }
            if (this.indices != null)
            {
                for (int index = 0; index < this.numberOfIndices; ++index)
                    meshInfo.indices[index] = this.indices[index];
            }
            else if (this.hasReflectionAndConformal)
            {
                for (int index = 0; index < this.numberOfFacets; ++index)
                {
                    meshInfo.indices[index * 3] = this.facets[index][0];
                    meshInfo.indices[index * 3 + 1] = this.facets[index][2];
                    meshInfo.indices[index * 3 + 2] = this.facets[index][1];
                }
            }
            else
            {
                for (int index = 0; index < this.numberOfFacets; ++index)
                {
                    meshInfo.indices[index * 3] = this.facets[index][0];
                    meshInfo.indices[index * 3 + 1] = this.facets[index][1];
                    meshInfo.indices[index * 3 + 2] = this.facets[index][2];
                }
            }
            meshInfo.revitMesh = (IRevitMesh)null;
        }

        public void MergeMesh(
          MeshInfo meshInfo,
          MaterialInfo matInfo,
          bool isBack,
          ref int verticesOfs,
          ref int indicesOfs)
        {
            if ((int)this.distributionOfNormals == 2)
                this.UnweldMeshPoints();
            if (this.normals == null)
                this.ComputeNormals(this.hasReflectionAndConformal);
            if (this.uvs == null)
                this.ComputeUVs();
            if (this.indices == null)
                this.ComputeIndices();
            float num1 = isBack ? -1f : 1f;
            for (int index = 0; index < this.numberOfPoints; ++index)
            {
                Vector3 vector3 = Vector3.Transform(this.points[index], this.transform);
                meshInfo.vertices[(verticesOfs + index) * 3] = vector3.X;
                meshInfo.vertices[(verticesOfs + index) * 3 + 1] = vector3.Z + (this.isDoubleSided ? -1f / 1000f : 0.0f);
                meshInfo.vertices[(verticesOfs + index) * 3 + 2] = vector3.Y;
            }
            switch ((int)this.distributionOfNormals)
            {
                case 0:
                    for (int index = 0; index < this.numberOfPoints; ++index)
                    {
                        Vector3 vector3 = Vector3.Transform(this.normals[index], this.normalTransform);
                        meshInfo.normals[(verticesOfs + index) * 3] = num1 * vector3.X;
                        meshInfo.normals[(verticesOfs + index) * 3 + 1] = num1 * vector3.Z;
                        meshInfo.normals[(verticesOfs + index) * 3 + 2] = num1 * vector3.Y;
                    }
                    break;
                case 1:
                    Vector3 vector3_1 = Vector3.Transform(this.normals[0], this.normalTransform);
                    for (int index = 0; index < this.numberOfPoints; ++index)
                    {
                        meshInfo.normals[(verticesOfs + index) * 3] = num1 * vector3_1.X;
                        meshInfo.normals[(verticesOfs + index) * 3 + 1] = num1 * vector3_1.Z;
                        meshInfo.normals[(verticesOfs + index) * 3 + 2] = num1 * vector3_1.Y;
                    }
                    break;
                case 2:
                    for (int index1 = 0; index1 < this.numberOfFacets; ++index1)
                    {
                        Vector3 vector3_2 = Vector3.Transform(this.normals[index1], this.normalTransform);
                        for (int index2 = 0; index2 < 3; ++index2)
                        {
                            int num2 = this.facets[index1][index2];
                            meshInfo.normals[(verticesOfs + num2) * 3] = num1 * vector3_2.X;
                            meshInfo.normals[(verticesOfs + num2) * 3 + 1] = num1 * vector3_2.Z;
                            meshInfo.normals[(verticesOfs + num2) * 3 + 2] = num1 * vector3_2.Y;
                        }
                    }
                    break;
            }
            double rotationAngle1 = (double)matInfo.ColorTexture.RotationAngle;
            double offsetU1 = (double)matInfo.ColorTexture.OffsetU;
            double offsetV1 = (double)matInfo.ColorTexture.OffsetV;
            for (int index = 0; index < this.numberOfPoints; ++index)
            {
                UV uv = this.uvs[index];
                double num3 = uv.U;
                double num4 = uv.V;
                if (Math.Abs(rotationAngle1) > 0.03)
                {
                    num3 = Math.Cos(rotationAngle1) * (uv.U - offsetU1) - Math.Sin(rotationAngle1) * (uv.V - offsetV1) + offsetU1;
                    num4 = Math.Sin(rotationAngle1) * (uv.U - offsetU1) + Math.Cos(rotationAngle1) * (uv.V - offsetV1) + offsetV1;
                }
                double num5 = (num3 - (double)matInfo.ColorTexture.OffsetU) * (double)matInfo.ColorTexture.ScaleU;
                double num6 = (num4 - (double)matInfo.ColorTexture.OffsetV) * (double)matInfo.ColorTexture.ScaleV;
                meshInfo.uvs[(verticesOfs + index) * 2] = (float)num5;
                meshInfo.uvs[(verticesOfs + index) * 2 + 1] = 1f - (float)num6;
            }
            if (meshInfo.numberOfUVSets == 2)
            {
                int num7 = verticesOfs + this.numberOfPoints;
                double rotationAngle2 = (double)matInfo.BumpTexture.RotationAngle;
                double offsetU2 = (double)matInfo.BumpTexture.OffsetU;
                double offsetV2 = (double)matInfo.BumpTexture.OffsetV;
                for (int index = 0; index < this.numberOfPoints; ++index)
                {
                    UV uv = this.uvs[index];
                    double num8 = uv.U;
                    double num9 = uv.V;
                    if (Math.Abs(rotationAngle2) > 0.02)
                    {
                        num8 = Math.Cos(rotationAngle2) * (uv.U - offsetU2) - Math.Sin(rotationAngle2) * (uv.V - offsetV2) + offsetU2;
                        num9 = Math.Sin(rotationAngle2) * (uv.U - offsetU2) + Math.Cos(rotationAngle2) * (uv.V - offsetV2) + offsetV2;
                    }
                    double num10 = (num8 - (double)matInfo.BumpTexture.OffsetU) * (double)matInfo.BumpTexture.ScaleU;
                    double num11 = (num9 - (double)matInfo.BumpTexture.OffsetV) * (double)matInfo.BumpTexture.ScaleV;
                    meshInfo.uvs[(num7 + index) * 2] = (float)num10;
                    meshInfo.uvs[(num7 + index) * 2 + 1] = 1f - (float)num11;
                }
            }
            if (isBack)
            {
                for (int index = 0; index < this.numberOfIndices; ++index)
                    meshInfo.indices[indicesOfs + index] = verticesOfs + this.indices[this.numberOfIndices - index - 1];
            }
            else
            {
                for (int index = 0; index < this.numberOfIndices; ++index)
                    meshInfo.indices[indicesOfs + index] = verticesOfs + this.indices[index];
            }
            verticesOfs += this.numberOfPoints;
            indicesOfs += this.numberOfIndices;
        }

        protected void UnweldMeshPoints()
        {
            this.numberOfPoints = this.numberOfFacets * 3;
            List<Vector3> vector3List = new List<Vector3>(this.numberOfPoints);
            List<UV> uvList = new List<UV>(this.numberOfPoints);
            int num = 0;
            for (int index1 = 0; index1 < this.numberOfFacets; ++index1)
            {
                for (int index2 = 0; index2 < 3; ++index2)
                {
                    int index3 = this.facets[index1][index2];
                    vector3List.Add(this.points[index3]);
                    uvList.Add(this.uvs[index3]);
                    this.facets[index1][index2] = num;
                    ++num;
                }
            }
            this.points = vector3List;
            this.uvs = uvList;
            this.indices = (List<int>)null;
        }

        protected void ComputeNormals(bool mirrored)
        {
            this.normals = new List<Vector3>(this.numberOfPoints);
            for (int index = 0; index < this.numberOfPoints; ++index)
                this.normals.Add(Vector3.Zero);
            for (int index1 = 0; index1 < this.numberOfPoints; ++index1)
            {
                for (int index2 = 0; index2 < this.numberOfIndices; index2 += 3)
                {
                    if (this.indices[index2] == index1 || this.indices[index2 + 1] == index1 || this.indices[index2 + 2] == index1)
                    {
                        Vector3 point1;
                        Vector3 point2;
                        Vector3 point3;
                        if (mirrored)
                        {
                            point1 = this.points[this.indices[index2]];
                            point2 = this.points[this.indices[index2 + 2]];
                            point3 = this.points[this.indices[index2 + 1]];
                        }
                        else
                        {
                            point1 = this.points[this.indices[index2]];
                            point2 = this.points[this.indices[index2 + 1]];
                            point3 = this.points[this.indices[index2 + 2]];
                        }
                        Vector3 vector3_1 = Vector3.Cross(point2 - point1, point3 - point1);
                        if ((double)Math.Abs(vector3_1.Length()) >= 1.40129846432482E-45)
                        {
                            Vector3 vector3_2 = Vector3.Normalize(vector3_1);
                            this.normals[index1] += vector3_2;
                        }
                    }
                }
            }
            for (int index = 0; index < this.normals.Count; ++index)
                this.normals[index] = Vector3.Normalize(this.normals[index]);
            this.numberOfPoints = this.points.Count;
            this.numberOfIndices = this.indices.Count;
            this.numberOfNormals = this.normals.Count;
            this.distributionOfNormals = (DistributionOfNormals)0;
        }

        protected void ComputeUVs(bool keepDensity = true, bool flipForizontal = false)
        {
            this.numberOfUVs = this.numberOfPoints;
            this.uvs = new List<UV>(this.numberOfUVs);
            Vector3 min = new Vector3();
            Vector3 max = new Vector3();
            this.ComputeBoundingBox(ref min, ref max);
            double num1 = (double)max.X - (double)min.X;
            double num2 = (double)max.Y - (double)min.Y;
            double num3 = (double)max.Z - (double)min.Z;
            if (keepDensity)
            {
                double num4;
                num3 = num4 = 1.0;
                num2 = num4;
                num1 = num4;
            }
            for (int index = 0; index < this.numberOfPoints; ++index)
            {
                double num5 = 0.0;
                double num6 = 0.0;
                Vector3 point = this.points[index];
                Vector3 normal = this.normals[index];
                if ((double)Math.Abs(normal.Length()) < 1.40129846432482E-45)
                {
                    this.uvs.Add(UV.Zero);
                }
                else
                {
                    if ((double)Math.Abs(normal.X) >= (double)Math.Abs(normal.Y) && (double)Math.Abs(normal.X) >= (double)Math.Abs(normal.Z))
                    {
                        num5 = (double)normal.X / (double)Math.Abs(normal.X) * (((double)point.Z - (double)min.Z) / num3);
                        num6 = -(((double)point.Y - (double)min.Y) / num2);
                    }
                    else if ((double)Math.Abs(normal.Z) >= (double)Math.Abs(normal.Y) && (double)Math.Abs(normal.Z) >= (double)Math.Abs(normal.X))
                    {
                        num5 = -((double)normal.Z / (double)Math.Abs(normal.Z)) * (((double)point.X - (double)min.X) / num1);
                        num6 = -(((double)point.Y - (double)min.Y) / num2);
                    }
                    else if ((double)Math.Abs(normal.Y) >= (double)Math.Abs(normal.Z) && (double)Math.Abs(normal.Y) >= (double)Math.Abs(normal.X))
                    {
                        num5 = (double)normal.Y / (double)Math.Abs(normal.Y) * (((double)point.X - (double)min.X) / num1);
                        num6 = -(((double)point.Z - (double)min.Z) / num3);
                    }
                    this.uvs.Add(new UV(num5, flipForizontal ? 1.0 - num6 : num6));
                }
            }
        }

        protected void ComputeBoundingBox(ref Vector3 min, ref Vector3 max)
        {
            float x1 = float.MaxValue;
            float y1 = float.MaxValue;
            float z1 = float.MaxValue;
            float x2 = float.MinValue;
            float y2 = float.MinValue;
            float z2 = float.MinValue;
            for (int index = 0; index < this.numberOfPoints; ++index)
            {
                Vector3 point = this.points[index];
                if ((double)point.X < (double)x1)
                    x1 = point.X;
                if ((double)point.Y < (double)y1)
                    y1 = point.Y;
                if ((double)point.Z < (double)z1)
                    z1 = point.Z;
                if ((double)point.X > (double)x2)
                    x2 = point.X;
                if ((double)point.Y > (double)y2)
                    y2 = point.Y;
                if ((double)point.Z > (double)z2)
                    z2 = point.Z;
            }
            min = new Vector3(x1, y1, z1);
            max = new Vector3(x2, y2, z2);
        }

        protected void ComputeIndices()
        {
            this.indices = new List<int>(this.numberOfIndices);
            if (this.hasReflectionAndConformal)
            {
                for (int index = 0; index < this.numberOfFacets; ++index)
                {
                    this.indices.Add(this.facets[index][0]);
                    this.indices.Add(this.facets[index][2]);
                    this.indices.Add(this.facets[index][1]);
                }
            }
            else
            {
                for (int index = 0; index < this.numberOfFacets; ++index)
                {
                    this.indices.Add(this.facets[index][0]);
                    this.indices.Add(this.facets[index][1]);
                    this.indices.Add(this.facets[index][2]);
                }
            }
        }
    }
}
