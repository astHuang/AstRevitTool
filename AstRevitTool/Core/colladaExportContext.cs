#region Namespaces
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;
using Autodesk.Revit.DB;
using System.Windows.Forms;
using System.Drawing;
using System.Text;
using AstRevitTool.Core.Export;
#endregion

namespace AstRevitTool.Core
{
    internal class ColladaExportContext : IExportContext
    {
        private OptionsExporter exportingOptions;
        private Options geometryOptions;
        private Document mainDocument;
        private Stack<Transform> transformationStack = new Stack<Transform>();
        private Stack<Document> documentStack = new Stack<Document>();
        private bool hasGeometry;
        private int modelNodeId;
        private Dictionary<int, Transform> modelNodes = new Dictionary<int, Transform>();
        private RevitMaterialManager materialManager;
        private ulong currentDocumentUID = ulong.MaxValue;
        private ulong currentMaterialUID = ulong.MaxValue;
        private Dictionary<ulong, IList<IRevitMesh>> materialIdToMeshes = new Dictionary<ulong, IList<IRevitMesh>>();
        private IList<IRevitMesh> currentMeshes;
        private int nSolids;
        protected Stopwatch clock;
        protected ulong nVertices;

        public bool IsSolidsPass { get; set; }

        public bool HasSolids { get; private set; }

        public string LinkDescription { get; private set; }

        public string InstanceDescription { get; private set; }

        public ColladaExportContext(Document document, OptionsExporter exportingOptions)
        {
            this.mainDocument = document;
            this.exportingOptions = exportingOptions;
            this.materialManager = new RevitMaterialManager();
            this.materialManager.Init(document);
            this.materialManager.MergeIfcMaterials = exportingOptions.MergeIfcMaterials;
            this.materialManager.MergeLinkedMaterials = exportingOptions.MergeLinkedMaterials;
            this.materialManager.ColladaExporterMode = true;
            this.geometryOptions = this.mainDocument.Application.Create.NewGeometryOptions();
            this.geometryOptions.ComputeReferences = true;
            this.geometryOptions.DetailLevel = (ViewDetailLevel)3;
        }

        public void WriteFile(string thumbFile)
        {
            if (this.materialIdToMeshes.Count<KeyValuePair<ulong, IList<IRevitMesh>>>() > 0)
            {
                this.clock = new Stopwatch();
                this.clock.Start();
                int count = this.materialIdToMeshes.Count;
                List<MaterialInfo> usedMaterials = new List<MaterialInfo>(count);
                Dictionary<ulong, MeshInfo> materialIdToMergedMeshes = new Dictionary<ulong, MeshInfo>(count);
                foreach (ulong key in this.materialIdToMeshes.Keys)
                {
                    if (this.materialIdToMeshes[key].Count > 0)
                    {
                        MaterialInfo material = this.materialManager.GetMaterial(key);
                        usedMaterials.Add(material);
                        materialIdToMergedMeshes[key] = new MeshInfo()
                        {
                            materialUID = key,
                            matInfo = material
                        };
                    }
                }
                Trace.WriteLine(string.Format("\n\nExporter::Collect surfaces: Number of surfaces:{0}\n\n", (object)materialIdToMergedMeshes.Count));
                Parallel.Invoke((Action)(() =>
                {
                    if (!this.exportingOptions.CollectTextures)
                        return;
                    this.CollectTextures(usedMaterials);
                    this.MakeTexturePathsRelative(usedMaterials);
                }), (Action)(() => Parallel.ForEach<KeyValuePair<ulong, IList<IRevitMesh>>>((IEnumerable<KeyValuePair<ulong, IList<IRevitMesh>>>)this.materialIdToMeshes, (Action<KeyValuePair<ulong, IList<IRevitMesh>>>)(it =>
                {
                    ulong key = it.Key;
                    IList<IRevitMesh> revitMeshList = it.Value;
                    if (revitMeshList.Count == 0)
                        return;
                    MaterialInfo material = this.materialManager.GetMaterial(key);
                    MeshInfo meshInfo = materialIdToMergedMeshes[key];
                    int nVertices = 0;
                    int nIndices = 0;
                    foreach (IRevitMesh revitMesh in (IEnumerable<IRevitMesh>)revitMeshList)
                    {
                        nVertices += revitMesh.GetVerticesCount();
                        nIndices += revitMesh.GetIndicesCount();
                    }
                    meshInfo.Resize(nVertices, nIndices, material.NeedSecondUV() ? 2 : 1);
                    int verticesOfs = 0;
                    int indicesOfs = 0;
                    foreach (IRevitMesh revitMesh in (IEnumerable<IRevitMesh>)revitMeshList)
                    {
                        revitMesh.MergeMesh(meshInfo, material, false, ref verticesOfs, ref indicesOfs);                       
                    }
                    revitMeshList.Clear();
                }))));
                Trace.WriteLine(string.Format("Exporter::Merge surfaces / collect textures {0}[ms]", (object)this.clock.ElapsedMilliseconds));
                this.clock.Restart();
                new ColladaWriter(usedMaterials, materialIdToMergedMeshes, this.modelNodes, this.exportingOptions).Write(this.exportingOptions.FilePath, thumbFile);
                Trace.WriteLine(string.Format("Exporter::Write file {0}[ms]", (object)this.clock.ElapsedMilliseconds));
            }
            this.clock = (Stopwatch)null;
            this.modelNodes.Clear();
            this.materialIdToMeshes.Clear();
            this.currentMeshes = (IList<IRevitMesh>)null;
            this.mainDocument = (Document)null;
            this.transformationStack.Clear();
            this.documentStack.Clear();
            this.currentDocumentUID = ulong.MaxValue;
            this.currentMaterialUID = ulong.MaxValue;
            this.materialManager.Clear();
        }

        public string GetExceptionRaport()
        {
            string exceptionRaport = "";
            if (this.LinkDescription != "")
                exceptionRaport += this.LinkDescription;
            if (this.InstanceDescription != "")
                exceptionRaport = exceptionRaport + (exceptionRaport != "" ? "\n" : "") + this.InstanceDescription;
            if (exceptionRaport != "")
                exceptionRaport = "\nAt " + exceptionRaport;
            return exceptionRaport;
        }

        protected void SelectCurrentDocument()
        {
            this.currentDocumentUID = ulong.MaxValue;
            this.currentMaterialUID = ulong.MaxValue;
            if (this.documentStack.Count <= 0)
                return;
            this.currentDocumentUID = (ulong)((long)(ulong)((object)this.documentStack.Peek()).GetHashCode() << 32 & -4294967296L);
        }

        protected void SelectCurrentMaterial(ulong materialUID)
        {
            this.currentMaterialUID = materialUID;
            if (this.materialIdToMeshes.TryGetValue(materialUID, out this.currentMeshes))
                return;
            this.currentMeshes = (IList<IRevitMesh>)new List<IRevitMesh>(100);
            this.materialIdToMeshes.Add(this.currentMaterialUID, this.currentMeshes);
        }

        protected ulong GetElementUID(ElementId eId) => this.currentDocumentUID | (ulong)eId.IntegerValue & (ulong)uint.MaxValue;

        private void MakeTexturePathsRelative(List<MaterialInfo> usedMaterials)
        {
            foreach (MaterialInfo materialInfo in usedMaterials.Where<MaterialInfo>((Func<MaterialInfo, bool>)(matInfo => matInfo.ColorTexture.Path != string.Empty)))
                materialInfo.ColorTexture.Path = "textures\\" + RevitMaterialManager.CleanPath(Path.GetFileName(materialInfo.ColorTexture.Path), this.exportingOptions.UnicodeSupport);
        }

        private void CollectTextures(List<MaterialInfo> usedMaterials)
        {
            if (usedMaterials.Select<MaterialInfo, string>((Func<MaterialInfo, string>)(o => o.ColorTexture.Path)).Distinct<string>().Count<string>() == 0)
                return;
            string path = Path.GetDirectoryName(this.exportingOptions.FilePath) + "\\textures";
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
            }
            foreach (string str1 in usedMaterials.Select<MaterialInfo, string>((Func<MaterialInfo, string>)(o => o.ColorTexture.Path)).Distinct<string>())
            {
                if (!(str1 == ""))
                {
                    string str2 = path + "\\" + RevitMaterialManager.CleanPath(Path.GetFileName(str1), this.exportingOptions.UnicodeSupport);
                    if (!File.Exists(str2))
                    {
                        try
                        {
                            File.Copy(str1, str2);
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
            }
        }

        private void ExportSolids(Element element)
        {
            foreach (GeometryObject geometryObject in element.get_Geometry(this.geometryOptions))
                this.ExportGeometryObject(geometryObject);
        }

        private void ExportGeometryObject(GeometryObject geometryObject)
        {
            if (geometryObject==null || geometryObject.Visibility == Visibility.Invisible || this.documentStack.Peek().GetElement(geometryObject.GraphicsStyleId) is GraphicsStyle element && ((Element)element).Name.Contains("Light Source"))
                return;
            GeometryInstance geometryInstance = geometryObject as GeometryInstance;
            if (geometryInstance!=null)
            {
                this.transformationStack.Push(this.transformationStack.Peek().Multiply(geometryInstance.Transform));
                GeometryElement symbolGeometry = geometryInstance.GetSymbolGeometry();
                if (symbolGeometry!=null)
                {
                    foreach (GeometryObject geometryObject1 in symbolGeometry)
                    {
                        if (geometryObject1!=null)
                            this.ExportGeometryObject(geometryObject1);
                    }
                }
                this.transformationStack.Pop();
            }
            else
            {
                Solid solid = geometryObject as Solid;
                if ((GeometryObject)solid == (GeometryObject)null)
                    return;
                this.ExportSolid(solid);
            }
        }

        private void ExportSolid(Solid solid)
        {
            foreach (Face face in solid.Faces)
            {
                if (face!=null)
                {
                    Mesh mesh = face.Triangulate(this.exportingOptions.ManualTessellatorLOD);
                    if (mesh!=null && mesh.Visibility != Visibility.Invisible)
                    {
                        ulong elementUid = this.GetElementUID(face.MaterialElementId);
                        if ((long)this.currentMaterialUID != (long)elementUid)
                            this.SelectCurrentMaterial(this.materialManager.SelectMaterial(elementUid, face, this.documentStack.Peek()));
                        IRevitMesh revitMesh = (IRevitMesh)new RevitMeshSIMD(this.transformationStack.Peek(), mesh);
                        this.nVertices += (ulong)revitMesh.GetVerticesCount();
                        this.currentMeshes.Add(revitMesh);
                    }
                }
            }
        }

        
        private bool IsElementStructural(Element element)
        {
            Category category = element.Category;
            if (category != null)
            {
                int integerValue = category.Id.IntegerValue;
                if (!integerValue.Equals(-2001320))
                {
                    integerValue = category.Id.IntegerValue;
                    if (!integerValue.Equals(-2000175))
                    {
                        integerValue = category.Id.IntegerValue;
                        if (!integerValue.Equals(-2000017))
                        {
                            integerValue = category.Id.IntegerValue;
                            if (!integerValue.Equals(-2000018))
                            {
                                integerValue = category.Id.IntegerValue;
                                if (!integerValue.Equals(-2000019))
                                {
                                    integerValue = category.Id.IntegerValue;
                                    if (!integerValue.Equals(-2000020))
                                    {
                                        integerValue = category.Id.IntegerValue;
                                        if (!integerValue.Equals(-2000126))
                                            goto label_9;
                                    }
                                }
                            }
                        }
                    }
                }
                return true;
            }
        label_9:
            return false;
        }

        private bool IsElementRailings(Element element)
        {
            Category category = element.Category;
            if (category != null)
            {
                int integerValue = category.Id.IntegerValue;
                if (!integerValue.Equals(-2000175))
                {
                    integerValue = category.Id.IntegerValue;
                    if (!integerValue.Equals(-2000126))
                        goto label_4;
                }
                return true;
            }
        label_4:
            return false;
        }

        private bool IsElementLink(Element element)
        {
            Category category = element.Category;
            return category != null && category.Id.IntegerValue.Equals(-2001352);
        }

        private bool IsElementInInteriorCategory(Element element)
        {
            if (element is ModelText)
                return false;
            Category category = element.Category;
            if (category != null)
            {
                int integerValue = category.Id.IntegerValue;
                if (!integerValue.Equals(-2000080))
                {
                    integerValue = category.Id.IntegerValue;
                    if (!integerValue.Equals(-2001000))
                    {
                        integerValue = category.Id.IntegerValue;
                        if (!integerValue.Equals(-2001040))
                        {
                            integerValue = category.Id.IntegerValue;
                            if (!integerValue.Equals(-2001060))
                            {
                                integerValue = category.Id.IntegerValue;
                                if (!integerValue.Equals(-2001100))
                                {
                                    integerValue = category.Id.IntegerValue;
                                    if (!integerValue.Equals(-2001140))
                                    {
                                        integerValue = category.Id.IntegerValue;
                                        if (!integerValue.Equals(-2001160))
                                        {
                                            integerValue = category.Id.IntegerValue;
                                            if (!integerValue.Equals(-2001350))
                                            {
                                                integerValue = category.Id.IntegerValue;
                                                if (!integerValue.Equals(-2008013))
                                                {
                                                    integerValue = category.Id.IntegerValue;
                                                    if (!integerValue.Equals(-2008075))
                                                    {
                                                        integerValue = category.Id.IntegerValue;
                                                        if (!integerValue.Equals(-2008077))
                                                        {
                                                            integerValue = category.Id.IntegerValue;
                                                            if (!integerValue.Equals(-2008079))
                                                            {
                                                                integerValue = category.Id.IntegerValue;
                                                                if (!integerValue.Equals(-2008081))
                                                                {
                                                                    integerValue = category.Id.IntegerValue;
                                                                    if (!integerValue.Equals(-2008085))
                                                                    {
                                                                        integerValue = category.Id.IntegerValue;
                                                                        if (!integerValue.Equals(-2008083))
                                                                        {
                                                                            integerValue = category.Id.IntegerValue;
                                                                            if (!integerValue.Equals(-2008087))
                                                                            {
                                                                                integerValue = category.Id.IntegerValue;
                                                                                if (!integerValue.Equals(-2000151))
                                                                                {
                                                                                    integerValue = category.Id.IntegerValue;
                                                                                    if (!integerValue.Equals(-2001120))
                                                                                        goto label_22;
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return true;
            }
        label_22:
            return false;
        }


        private bool IsElementUnsupported(Element element) => element is Level;

        public bool Start()
        {
            this.documentStack.Clear();
            this.documentStack.Push(this.mainDocument);
            this.SelectCurrentDocument();
            this.transformationStack.Clear();
            this.transformationStack.Push(Transform.Identity);
            this.modelNodes.Clear();
            this.modelNodeId = 0;
            int num = this.IsSolidsPass ? 1 : 0;
            this.clock = new Stopwatch();
            this.clock.Start();
            this.HasSolids = false;
            this.nSolids = 0;
            return true;
        }

        public void Finish()
        {
            this.clock.Stop();
            int num = this.IsSolidsPass ? 1 : 0;
            this.clock = (Stopwatch)null;
        }

        public bool IsCanceled() => false;

        public RenderNodeAction OnViewBegin(ViewNode node)
        {
            node.LevelOfDetail = this.IsSolidsPass ? this.exportingOptions.SolidsLOD : this.exportingOptions.LevelOfDetail;
            return (RenderNodeAction)0;
        }

        public void OnViewEnd(ElementId elementId)
        {
        }

        public void OnRPC(RPCNode node)
        {
        }

        public RenderNodeAction OnInstanceBegin(InstanceNode node)
        {
            RenderNodeAction renderNodeAction = (RenderNodeAction)0;
            this.hasGeometry = false;
            try
            {
                ElementId symbolId = ((GroupNode)node).GetSymbolId();
                Element element = this.documentStack.Peek().GetElement(symbolId);              
                this.InstanceDescription = !(element is FamilySymbol familySymbol) ? string.Format("Instance: [{0}] {1}", element == null ? (object)"<null>" : (object)string.Concat((object)((object)element).GetType()), (object)((RenderNode)node).NodeName) : string.Format("Family instance: [{0}] of <{1}>", (object)((RenderNode)node).NodeName, (object)((Element)familySymbol).Name);
                this.transformationStack.Push(this.transformationStack.Peek().Multiply(((GroupNode)node).GetTransform()));
            }
            catch (Exception ex)
            {
                this.transformationStack.Push(Transform.Identity);
                renderNodeAction = (RenderNodeAction)1;
            }
            return renderNodeAction;
        }

        public void OnInstanceEnd(InstanceNode node)
        {
            Transform transform = this.transformationStack.Pop();
            if (this.hasGeometry)
                this.modelNodes.Add(++this.modelNodeId, transform);
            this.InstanceDescription = "";
        }

        public RenderNodeAction OnLinkBegin(LinkNode node)
        {
            RenderNodeAction renderNodeAction = (RenderNodeAction)0;
            this.LinkDescription = string.Format("Link: {0} <{1}>", (object)((RenderNode)node).NodeName, node.GetDocument() != null ? (object)node.GetDocument().Title : (object)"no-document");
            this.documentStack.Push(node.GetDocument());
            this.SelectCurrentDocument();
            try
            {
                this.transformationStack.Push(this.transformationStack.Peek().Multiply(((GroupNode)node).GetTransform()));
            }
            catch (Exception ex)
            {
                this.transformationStack.Push(Transform.Identity);
                renderNodeAction = (RenderNodeAction)1;
            }
            return renderNodeAction;
        }

        public void OnLinkEnd(LinkNode node)
        {
            this.documentStack.Pop();
            this.transformationStack.Pop();
            this.SelectCurrentDocument();
            this.LinkDescription = "";
        }

        public RenderNodeAction OnElementBegin(ElementId elementId)
        {
            try
            {
                Element element = this.documentStack.Peek().GetElement(elementId);
                if (element != null)
                {
                    if (this.IsElementUnsupported(element) || this.exportingOptions.SkipInteriorDetails && this.IsElementInInteriorCategory(element))
                        return (RenderNodeAction)1;
                    bool flag = this.IsElementStructural(element);
                    if (this.IsSolidsPass)
                    {
                        if (flag)
                        {
                            ++this.nSolids;
                            if (!this.exportingOptions.OptimizeSolids)
                                return (RenderNodeAction)0;
                            this.hasGeometry = true;
                            this.ExportSolids(element);
                            return (RenderNodeAction)1;
                        }
                        return this.IsElementLink(element) ? (RenderNodeAction)0 : (RenderNodeAction)1;
                    }
                    if (flag)
                    {
                        ++this.nSolids;
                        this.HasSolids = true;
                        return (RenderNodeAction)1;
                    }
                }
            }
            catch (Exception ex)
            {
                return (RenderNodeAction)1;
            }
            return !this.IsSolidsPass ? (RenderNodeAction)0 : (RenderNodeAction)1;
        }

        public void OnElementEnd(ElementId elementId)
        {
        }

        public void OnMaterial(MaterialNode materialNode)
        {
            ulong elementUid = this.GetElementUID(materialNode.MaterialId);
            if ((long)this.currentMaterialUID == (long)elementUid)
                return;
            this.SelectCurrentMaterial(this.materialManager.SelectMaterial(elementUid, materialNode, this.documentStack.Peek()));
        }

        public void OnPolymesh(PolymeshTopology polyMesh)
        {
            Transform tr = this.transformationStack.Peek();
            try
            {
                IRevitMesh revitMesh = (IRevitMesh)new RevitMeshSIMD(tr, polyMesh);
                this.nVertices += (ulong)revitMesh.GetVerticesCount();
                this.hasGeometry = true;
                this.currentMeshes.Add(revitMesh);
            }
            catch (Exception ex)
            {
            }
        }

        public RenderNodeAction OnFaceBegin(FaceNode node) => (RenderNodeAction)1;

        public void OnFaceEnd(FaceNode node)
        {
        }

        public void OnLight(LightNode node)
        {
        }
    }
}
