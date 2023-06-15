using Autodesk.Revit.DB;
//using LiveSyncAPI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Baml2006;

namespace AstRevitTool.Core.Export
{
    internal class ColladaWriter
    {
        private Dictionary<ulong, MeshInfo> materialIdToMergedMeshes;
        private List<MaterialInfo> usedMaterials;
        private Dictionary<int, Transform> modelNodes;
        private StreamWriter streamWriter;
        private StringBuilder sb = new StringBuilder();
        private OptionsExporter exportingOptions;

        public ColladaWriter(List<MaterialInfo> materials,
      Dictionary<ulong, MeshInfo> meshes,
      Dictionary<int, Transform> nodes,
      OptionsExporter options)
        {
            this.usedMaterials = materials;
            this.materialIdToMergedMeshes = meshes;
            this.modelNodes = nodes;
            this.exportingOptions = options;
            this.sb.Capacity = 1048576;
        }

        public bool Write(string filePath, string thumbFile)
        {
            GC.Collect();
            if (!this.OpenStream(filePath))
                return false;
            this.WriteXmlColladaBegin();
            this.WriteXmlAsset();
            this.WriteXmlLibraryGeometries();
            this.WriteXmlLibraryImages();
            this.WriteXmlLibraryMaterials();
            this.WriteXmlLibraryEffects();
            this.WriteXmlLibraryVisualScenes();
            this.WriteXmlColladaEnd();
            this.CloseStream();
            return true;
        }

        private void WriteXmlColladaBegin()
        {
            this.streamWriter.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
            this.streamWriter.Write("<COLLADA xmlns=\"http://www.collada.org/2005/11/COLLADASchema\" version=\"1.4.1\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">\n");

        }

        private void WriteXmlColladaEnd() => this.streamWriter.Write("</COLLADA>\n");

        private void WriteXmlAsset()
        {
            this.streamWriter.Write("<asset>\n");
            this.streamWriter.Write("   <contributor>\n");
            this.streamWriter.Write("       <author>Zhenxiang Huang</author>");
            this.streamWriter.Write("       <authoring_tool>Revit To Cumulus bridge by Arrowstreet" + "</authoring_tool>\n");
            this.streamWriter.Write("   </contributor>\n");
            this.streamWriter.Write("   <created>" + DateTime.Now.ToString((IFormatProvider)CultureInfo.InvariantCulture) + "</created>\n");
            if (this.exportingOptions.UseCentimeter)
            {
                this.streamWriter.Write("   <unit name=\"centimeter\" meter=\"0.01\"/>\n");
            }
            else
            {
                this.streamWriter.Write("   <unit name=\"meter\" meter=\"1.00\"/>\n");
            }
            this.streamWriter.Write("   <up_axis>Z_UP</up_axis>\n");
            this.streamWriter.Write("</asset>\n");
        }

        private void WriteXmlLibraryVisualScenes()
        {
            this.streamWriter.Write("<library_visual_scenes>\n");
            this.streamWriter.Write("   <visual_scene id=\"Revit_project\">\n");
            this.streamWriter.Write("      <node id=\"node-Model\" name=\"geometry-Model\">\n");
            foreach (ulong key in this.materialIdToMergedMeshes.Keys)
            {
                string materialSid = this.GetMaterialSID(key);
                this.streamWriter.Write("           <instance_geometry url=\"#geom-" + materialSid + "\">\n");
                this.streamWriter.Write("               <bind_material>\n");
                this.streamWriter.Write("                   <technique_common>\n");
                this.streamWriter.Write("                       <instance_material target=\"#material-" + materialSid + "\" symbol=\"material-" + materialSid + "\" >\n");
                this.streamWriter.Write("                       </instance_material>\n");
                this.streamWriter.Write("                   </technique_common>\n");
                this.streamWriter.Write("               </bind_material>\n");
                this.streamWriter.Write("           </instance_geometry>\n");
            }
            if (this.exportingOptions.ExportNodes)
                this.streamWriter.Write(ColladaTemplates.ExtraFlattenHierarchy);
            this.streamWriter.Write("      </node>\n");
            if (this.exportingOptions.ExportNodes)
            {
                foreach (KeyValuePair<int, Transform> modelNode in this.modelNodes)
                {
                    this.streamWriter.Write("       <node id=\"node-" + (object)modelNode.Key + "\" name=\"geometry-" + (object)modelNode.Key + "\">\n");
                    this.WriteXmlMatrix(modelNode.Value);
                    this.streamWriter.Write(ColladaTemplates.NodeGeometryInstance);
                    this.streamWriter.Write("       </node>\n");
                }
            }
            this.streamWriter.Write("   </visual_scene>\n");
            this.streamWriter.Write("</library_visual_scenes>\n");
            this.streamWriter.Write("<scene>\n");
            this.streamWriter.Write("   <instance_visual_scene url=\"#Revit_project\"/>\n");
            this.streamWriter.Write("</scene>\n");
        }

        private void WriteXmlLibraryGeometries()
        {
            this.WriteXmlLibraryGeometriesBegin();
            foreach (KeyValuePair<ulong, MeshInfo> materialIdToMergedMesh in this.materialIdToMergedMeshes)
            {
                string materialSid = this.GetMaterialSID(materialIdToMergedMesh.Key);
                MeshInfo meshInfo = materialIdToMergedMesh.Value;
                MaterialInfo matInfo = meshInfo.matInfo;
                this.sb.Clear();
                this.WriteXmlGeometryBegin(materialSid, matInfo);
                this.WriteXmlGeometrySourcePositions(materialSid, meshInfo);
                this.WriteXmlGeometrySourceNormals(materialSid, meshInfo);
                this.WriteXmlGeometrySourceMap(materialSid, meshInfo);
                this.WriteXmlGeometryVertices(materialSid);
                this.WriteXmlGeometryTrianglesWithMap(materialSid, meshInfo);
                this.WriteXmlGeometryEnd();
                int val1 = 10485760;
                while (this.sb.Length > 0)
                {
                    int length = Math.Min(val1, this.sb.Length);
                    this.streamWriter.Write(this.sb.ToString(0, length));
                    this.sb.Remove(0, length);
                }
            }
            if (this.exportingOptions.ExportNodes)
                this.streamWriter.Write(ColladaTemplates.NodeGeometry);
            this.WriteXmlLibraryGeometriesEnd();

        }
        private void WriteXmlLibraryGeometriesBegin() => this.streamWriter.Write("<library_geometries>\n");

        private void WriteXmlLibraryGeometriesEnd() => this.streamWriter.Write("</library_geometries>\n");

        private void WriteXmlGeometryBegin(string materialSID, MaterialInfo materialInfo)
        {
            this.sb.AppendFormat("  <geometry id=\"geom-{0}\" name=\"{1}\">\n", (object)materialSID, (object)materialInfo.Name);
            this.sb.Append("    <mesh>\n");
        }

        private void WriteXmlGeometryEnd()
        {
            this.sb.Append("    </mesh>\n");
            this.sb.Append("  </geometry>\n");
        }

        private void WriteXmlGeometrySourcePositions(string materialSID, MeshInfo meshInfo)
        {
            int numberOfVertices = meshInfo.numberOfVertices;
            this.sb.AppendFormat("          <source id=\"geom-{0}-positions\">\n", (object)materialSID);
            this.sb.AppendFormat("              <float_array id=\"geom-{0}-positions-array\" count=\"{1}\">", (object)materialSID, (object)(numberOfVertices * 3));
            for (int index = 0; index < numberOfVertices; ++index)
                this.sb.AppendFormat((IFormatProvider)CultureInfo.InvariantCulture.NumberFormat, "{0:0.###} {1:0.###} {2:0.###} ", (object)meshInfo.vertices[index * 3], (object)meshInfo.vertices[index * 3 + 2], (object)meshInfo.vertices[index * 3 + 1]);
            this.sb.Append("</float_array>\n");
            this.sb.Append("              <technique_common>\n");
            this.sb.AppendFormat("                  <accessor source=\"#geom-{0}-positions-array\" count=\"{1}\" stride=\"3\">\n", (object)materialSID, (object)numberOfVertices);
            this.sb.Append("                    <param name=\"X\" type=\"float\"/>\n");
            this.sb.Append("                    <param name=\"Y\" type=\"float\"/>\n");
            this.sb.Append("                    <param name=\"Z\" type=\"float\"/>\n");
            this.sb.Append("                </accessor>\n");
            this.sb.Append("              </technique_common>\n");
            this.sb.Append("          </source>\n");
        }

        private void WriteXmlGeometrySourceNormals(string materialSID, MeshInfo meshInfo)
        {
            int numberOfVertices = meshInfo.numberOfVertices;
            this.sb.AppendFormat("          <source id=\"geom-{0}-normals\">\n", (object)materialSID);
            this.sb.AppendFormat("              <float_array id=\"geom-{0}-normals-array\" count=\"{1}\">", (object)materialSID, (object)(numberOfVertices * 3));
            for (int index = 0; index < numberOfVertices; ++index)
                this.sb.AppendFormat((IFormatProvider)CultureInfo.InvariantCulture.NumberFormat, "{0:0.###} {1:0.###} {2:0.###} ", (object)meshInfo.normals[index * 3], (object)meshInfo.normals[index * 3 + 2], (object)meshInfo.normals[index * 3 + 1]);
            this.sb.Append("</float_array>\n");
            this.sb.Append("              <technique_common>\n");
            this.sb.AppendFormat("                  <accessor source=\"#geom-{0}-normals-array\" count=\"{1}\" stride=\"3\">\n", (object)materialSID, (object)numberOfVertices);
            this.sb.Append("                    <param name=\"X\" type=\"float\"/>\n");
            this.sb.Append("                    <param name=\"Y\" type=\"float\"/>\n");
            this.sb.Append("                    <param name=\"Z\" type=\"float\"/>\n");
            this.sb.Append("                </accessor>\n");
            this.sb.Append("              </technique_common>\n");
            this.sb.Append("          </source>\n");
        }

        private void WriteXmlGeometrySourceMap(string materialSID, MeshInfo meshInfo)
        {
            int numberOfVertices = meshInfo.numberOfVertices;
            this.sb.AppendFormat("          <source id=\"geom-{0}-map\">\n", (object)materialSID);
            this.sb.AppendFormat("              <float_array id=\"geom-{0}-map-array\" count=\"{1}\">", (object)materialSID, (object)(numberOfVertices * 2));
            for (int index = 0; index < numberOfVertices; ++index)
                this.sb.AppendFormat((IFormatProvider)CultureInfo.InvariantCulture.NumberFormat, "{0:0.###} {1:0.###} ", (object)meshInfo.uvs[index * 2], (object)(float)(1.0 - (double)meshInfo.uvs[index * 2 + 1]));
            this.sb.Append("</float_array>\n");
            this.sb.Append("              <technique_common>\n");
            this.sb.AppendFormat("                  <accessor source=\"#geom-{0}-map-array\" count=\"{1}\" stride=\"2\">\n", (object)materialSID, (object)numberOfVertices);
            this.sb.Append("                    <param name=\"S\" type=\"float\"/>\n");
            this.sb.Append("                    <param name=\"T\" type=\"float\"/>\n");
            this.sb.Append("                  </accessor>\n");
            this.sb.Append("              </technique_common>\n");
            this.sb.Append("          </source>\n");
        }

        private void WriteXmlGeometryVertices(string materialSID)
        {
            this.sb.AppendFormat("          <vertices id=\"geom-{0}-vertices\">\n", (object)materialSID);
            this.sb.AppendFormat("              <input semantic=\"POSITION\" source=\"#geom-{0}-positions\"/>\n", (object)materialSID);
            this.sb.Append("          </vertices>\n");
        }
        private void WriteXmlGeometryTrianglesWithMap(string materialSID, MeshInfo meshInfo)
        {
            int numberOfIndices = meshInfo.numberOfIndices;
            this.sb.AppendFormat("          <triangles count=\"{0}\" material=\"material-{1}\">\n", (object)(numberOfIndices / 3), (object)materialSID);
            this.sb.AppendFormat("              <input offset=\"0\" semantic=\"VERTEX\" source=\"#geom-{0}-vertices\"/>\n", (object)materialSID);
            this.sb.AppendFormat("              <input offset=\"0\" semantic=\"TEXCOORD\" source=\"#geom-{0}-map\" set=\"0\"/>\n", (object)materialSID);
            this.sb.AppendFormat("              <input offset=\"0\" semantic=\"NORMAL\" source=\"#geom-{0}-normals\"/>\n", (object)materialSID);
            this.sb.Append("            <p>");
            for (int index = 0; index < numberOfIndices; ++index)
                this.sb.AppendFormat((IFormatProvider)CultureInfo.InvariantCulture.NumberFormat, "{0} ", (object)meshInfo.indices[index]);
            this.sb.Append("</p>\n");
            this.sb.Append("          </triangles>\n");
        }
        private void WriteXmlLibraryImages()
        {
            this.streamWriter.Write("<library_images>\n");
            if (!this.exportingOptions.CollectTextures)
            {
                this.streamWriter.Write("</library_images>\n");
            }
            else
            {
                foreach (string str in this.usedMaterials.Select<MaterialInfo, string>((Func<MaterialInfo, string>)(o => o.ColorTexture.Path)).Distinct<string>())
                {
                    if (!(str == ""))
                    {
                        this.streamWriter.Write("   <image id=\"image-" + str.GetHashCode().ToString("X") + "\">\n");
                        this.streamWriter.Write("       <init_from>" + this.EncodeAnyURI(str) + "</init_from>\n");
                        this.streamWriter.Write("   </image>\n");
                    }
                }
                this.streamWriter.Write("</library_images>\n");
            }
        }

        private void WriteXmlLibraryMaterials()
        {
            this.streamWriter.Write("<library_materials>\n");
            foreach (MaterialInfo usedMaterial in this.usedMaterials)
            {
                string materialSid = this.GetMaterialSID(usedMaterial.materialID);
                string name = usedMaterial.Name;
                this.streamWriter.Write("   <material id=\"material-" + materialSid + "\" name=\"" + name + "\">\n");
                this.streamWriter.Write("       <instance_effect url=\"#effect-" + materialSid + "\" />\n");
                this.streamWriter.Write("   </material>\n");
            }
            if (this.exportingOptions.ExportNodes)
                this.streamWriter.Write(ColladaTemplates.NodeMaterial);
            this.streamWriter.Write("</library_materials>\n");
        }

        private void WriteXmlLibraryEffects()
        {
            this.streamWriter.Write("<library_effects>\n");
            foreach (MaterialInfo usedMaterial in this.usedMaterials)
            {
                string materialSid = this.GetMaterialSID(usedMaterial.materialID);
                string name = usedMaterial.Name;
                string str1 = "";
                this.streamWriter.Write("   <effect id=\"effect-" + materialSid + "\" name=\"" + name + "\">\n");
                this.streamWriter.Write("       <profile_COMMON>\n");
                if (this.exportingOptions.CollectTextures && usedMaterial.ColorTexture.Path.Length > 0)
                {
                    str1 = usedMaterial.ColorTexture.Path.GetHashCode().ToString("X");
                    this.streamWriter.Write(" <newparam sid=\"surface-" + str1 + "\">\n");
                    this.streamWriter.Write("  <surface type=\"UNTYPED\">\n");
                    this.streamWriter.Write("   <init_from>image-" + str1 + "</init_from>\n");
                    this.streamWriter.Write("  </surface>\n");
                    this.streamWriter.Write(" </newparam>\n");
                    this.streamWriter.Write(" <newparam sid=\"sampler2d-" + str1 + "\">\n");
                    this.streamWriter.Write("  <sampler2D>\n");
                    this.streamWriter.Write("   <source>surface-" + str1 + "</source>\n");
                    this.streamWriter.Write("  </sampler2D>\n");
                    this.streamWriter.Write(" </newparam>\n");
                }
                this.streamWriter.Write("           <technique sid=\"standard\">\n");
                this.streamWriter.Write("               <phong>\n");
                this.streamWriter.Write("                   <emission><color>0 0 0 1</color></emission>");
                this.streamWriter.Write("                   <ambient>\n");
                this.streamWriter.Write("                       <color>0.01 0.01 0.01 1.0</color>\n");
                this.streamWriter.Write("                   </ambient>\n");
                this.streamWriter.Write("                   <diffuse>\n");
                if (this.exportingOptions.CollectTextures && usedMaterial.ColorTexture.Path.Length > 0)
                {
                    this.streamWriter.Write("                       <texture texture=\"sampler2d-" + str1 + "\" texcoord=\"CHANNEL0\"/>\n");
                }
                else
                {
                    
                    System.Drawing.Color color = System.Drawing.Color.FromArgb(usedMaterial.Color);
                    float num1 = (float)color.R / (float)byte.MaxValue;
                    float num2 = (float)color.G / (float)byte.MaxValue;
                    float num3 = (float)color.B / (float)byte.MaxValue;
                    float num4 = 1f - usedMaterial.Transparency;
                    num4 = num4 * num4;//enhance transparency if exists
                    if (this.exportingOptions.BlackAndWhite) {
                        float bw = ((num1 + num2 + num3) * (float)0.49);//prefer brighter color than darker in BW mode
                        num1 = bw < 0.98 ? bw : (float)0.98;
                        num2 = bw < 0.98 ? bw : (float)0.98;
                        num3 = bw < 0.98 ? bw : (float)0.98;
                    }
                    this.streamWriter.Write("                       <color>" + Convert.ToString(num1, (IFormatProvider)CultureInfo.InvariantCulture.NumberFormat) + " " + Convert.ToString(num2, (IFormatProvider)CultureInfo.InvariantCulture.NumberFormat) + " " + Convert.ToString(num3, (IFormatProvider)CultureInfo.InvariantCulture.NumberFormat) + " " + Convert.ToString(num4, (IFormatProvider)CultureInfo.InvariantCulture.NumberFormat) + "</color>\n");
                }
                this.streamWriter.Write("                   </diffuse>\n");
                this.streamWriter.Write("                   <specular>\n");
                this.streamWriter.Write("                       <color>0.01 0.01 0.01 1</color>\n");
                this.streamWriter.Write("                   </specular>\n");
                /*
                this.streamWriter.Write("<shininess>\n");
                this.streamWriter.Write("<float>" + Convert.ToString(usedMaterial.Shininess, (IFormatProvider)CultureInfo.InvariantCulture.NumberFormat) + "</float>\n");
                this.streamWriter.Write("</shininess>\n");
                this.streamWriter.Write("<reflective>\n");
                this.streamWriter.Write("<color>0 0 0 1.0</color>\n");
                this.streamWriter.Write("</reflective>\n");
                this.streamWriter.Write("<reflectivity>\n");
                this.streamWriter.Write("<float>1.0</float>\n");
                this.streamWriter.Write("</reflectivity>\n");*/
                string str2 = Convert.ToString(1.0 - (double)usedMaterial.Transparency, (IFormatProvider)CultureInfo.InvariantCulture.NumberFormat);
                this.streamWriter.Write("                   <transparent opaque=\"A_ONE\">\n");
                this.streamWriter.Write("                       <color>1.0 1.0 1.0 1.0</color>\n");
                this.streamWriter.Write("                   </transparent>\n");
                this.streamWriter.Write("                   <transparency>\n");
                this.streamWriter.Write("                       <float>" + str2 + "</float>\n");
                this.streamWriter.Write("                   </transparency>\n");
                this.streamWriter.Write("               </phong>\n");
                this.streamWriter.Write("           </technique>\n");
                this.streamWriter.Write("       </profile_COMMON>\n");
                this.streamWriter.Write("   </effect>\n");
            }
            if (this.exportingOptions.ExportNodes)
                this.streamWriter.Write(ColladaTemplates.NodeMaterialEffect);
            this.streamWriter.Write("</library_effects>\n");
        }


        private void WriteXmlMatrix(Transform tr)
        {
            this.sb.Clear();
            this.sb.AppendFormat((IFormatProvider)CultureInfo.InvariantCulture.NumberFormat, "{0:0.###} {1:0.###} {2:0.###} {3:0.###} ", (object)tr.BasisX.X, (object)tr.BasisY.X, (object)tr.BasisZ.X, (object)(tr.Origin.X * 0.304800003767014));
            this.sb.AppendFormat((IFormatProvider)CultureInfo.InvariantCulture.NumberFormat, "{0:0.###} {1:0.###} {2:0.###} {3:0.###} ", (object)tr.BasisX.Y, (object)tr.BasisY.Y, (object)tr.BasisZ.Y, (object)(tr.Origin.Y * 0.304800003767014));
            this.sb.AppendFormat((IFormatProvider)CultureInfo.InvariantCulture.NumberFormat, "{0:0.###} {1:0.###} {2:0.###} {3:0.###} ", (object)tr.BasisX.Z, (object)tr.BasisY.Z, (object)tr.BasisZ.Z, (object)(tr.Origin.Z * 0.304800003767014));
            this.sb.Append("0 0 0 1");
            this.streamWriter.Write("<matrix>");
            this.streamWriter.Write(this.sb.ToString(0, this.sb.Length));
            this.streamWriter.Write("</matrix>\n");
        }

        private bool OpenStream(string filePath)
        {
            this.streamWriter = new StreamWriter(filePath, false, Encoding.UTF8, 11048576);
            return this.streamWriter != null;
        }

        private void CloseStream() => this.streamWriter.Close();

        private string GetMaterialSID(ulong materialID) => materialID.ToString("X");
        private string EncodeAnyURI(string str) => Uri.EscapeDataString(str);
        private void WriteXmlColladaExtras(string thumbFile)
        {
            this.streamWriter.Write("\t<extra>\n");
            this.streamWriter.Write("\t\t<technique profile=\"CUMULUS_EXPORT\">\n");
            this.streamWriter.Write("\t\t\t<insertion_point>" + this.exportingOptions.InsertionPoint.ToString() + "</insertion_point>\n");
            this.streamWriter.Write("\t\t\t<solids_lod>" + this.exportingOptions.SolidsLOD.ToString() + "</solids_lod>\n");
            this.streamWriter.Write("\t\t\t<manual_lod>" + this.exportingOptions.ManualTessellatorLOD.ToString((IFormatProvider)CultureInfo.InvariantCulture) + "</manual_lod>\n");
            this.streamWriter.Write("\t\t\t<level_of_detail>" + this.exportingOptions.LevelOfDetail.ToString() + "</level_of_detail>\n");
            this.streamWriter.Write("\t\t\t<optimize_solids>" + (this.exportingOptions.OptimizeSolids ? "1" : "0") + "</optimize_solids>\n");
            this.streamWriter.Write("\t\t\t<merge_ifc_materials>" + (this.exportingOptions.MergeIfcMaterials ? "1" : "0") + "</merge_ifc_materials>\n");
            this.streamWriter.Write("\t\t\t<skip_interior_details>" + (this.exportingOptions.SkipInteriorDetails ? "1" : "0") + "</skip_interior_details>\n");
            this.streamWriter.Write("\t\t\t<unicode_support>" + (this.exportingOptions.UnicodeSupport ? "1" : "0") + "</unicode_support>\n");
            this.streamWriter.Write("\t\t\t<collect_textures>" + (this.exportingOptions.CollectTextures ? "1" : "0") + "</collect_textures>\n");
            this.streamWriter.Write("\t\t\t<export_nodes>" + (this.exportingOptions.ExportNodes ? "1" : "0") + "</export_nodes>\n");
            this.streamWriter.Write("\t\t\t<view_name>" + this.EncodeAnyURI(((Element)this.exportingOptions.MainView3D).Name) + "</view_name>\n");
            
            if (thumbFile != "" && File.Exists(thumbFile))
            {
                byte[] numArray = File.ReadAllBytes(thumbFile);
                this.streamWriter.Write("\t\t\t<thumbnail><data>");
                for (int index = 0; index < numArray.Length; ++index)
                    this.streamWriter.Write("{0:X2}", (object)numArray[index]);
                this.streamWriter.Write("</data></thumbnail>\n");
            }
            this.streamWriter.Write("\t\t</technique>\n");
            this.streamWriter.Write("\t</extra>\n");
        }


    }
}
