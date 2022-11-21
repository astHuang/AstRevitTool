using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Visual;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
namespace AstRevitTool.Core.Export
{
    public class RevitMaterialManager
    {
        public const ulong MATERIAL_DEFAULT_UID = 0;
        public const ulong MATERIAL_INVALID_UID = 18446744073709551615;
        protected static Encoding usAsciiEncoder = Encoding.GetEncoding("us-ascii", (EncoderFallback)new EncoderReplacementFallback(string.Empty), (DecoderFallback)new DecoderExceptionFallback());
        protected static Encoding Utf16Encoder = Encoding.GetEncoding("unicode", (EncoderFallback)new EncoderReplacementFallback(string.Empty), (DecoderFallback)new DecoderExceptionFallback());
        protected ICollection<string> textureFolders = (ICollection<string>)new List<string>();
        protected HashSet<ulong> affectedMaterials = new HashSet<ulong>();
        protected Dictionary<ulong, MaterialInfo> materialLibrary = new Dictionary<ulong, MaterialInfo>();
        protected Dictionary<ulong, ulong> linkedMaterialsDictionary = new Dictionary<ulong, ulong>();
        protected MaterialInfo defaultMaterial = new MaterialInfo();
        protected int currentDocumentID;

        public static string CleanPath(string name, bool useUnicode = true)
        {
            string source;
            if (useUnicode)
            {
                byte[] bytes = RevitMaterialManager.Utf16Encoder.GetBytes(name);
                source = RevitMaterialManager.Utf16Encoder.GetString(bytes);
            }
            else
            {
                byte[] bytes = RevitMaterialManager.usAsciiEncoder.GetBytes(name);
                source = RevitMaterialManager.usAsciiEncoder.GetString(bytes);
            }
            return new string(source.Where<char>((Func<char, bool>)(c => char.IsWhiteSpace(c) || char.IsLetterOrDigit(c) || c == '.' || c == '-')).ToArray<char>());
        }

        public static string CleanMaterialName(string name, bool useUnicode = true, bool escape = true)
        {
            string source;
            if (useUnicode)
            {
                byte[] bytes = RevitMaterialManager.Utf16Encoder.GetBytes(name);
                source = RevitMaterialManager.Utf16Encoder.GetString(bytes);
            }
            else
            {
                byte[] bytes = RevitMaterialManager.usAsciiEncoder.GetBytes(name);
                source = RevitMaterialManager.usAsciiEncoder.GetString(bytes);
            }
            string str = new string(source.Where<char>((Func<char, bool>)(c => char.IsWhiteSpace(c) || char.IsLetterOrDigit(c) || char.IsPunctuation(c))).ToArray<char>());
            if (escape)
                str = SecurityElement.Escape(str);
            return str;
        }

        public bool MergeIfcMaterials { get; set; }

        public bool MergeLinkedMaterials { get; set; }

        public bool ColladaExporterMode { get; set; }

        public RevitMaterialManager()
        {
            this.defaultMaterial.materialID = 0UL;
            this.defaultMaterial.Name = "Default material";
            this.defaultMaterial.Category = "Generic";
        }

        ~RevitMaterialManager() => this.Clear();
        public void Clear()
        {
            this.affectedMaterials.Clear();
            this.materialLibrary.Clear();
            this.linkedMaterialsDictionary.Clear();
            this.textureFolders.Clear();
            this.currentDocumentID = 0;
            this.MergeIfcMaterials = false;
            this.MergeLinkedMaterials = false;
            this.ColladaExporterMode = false;
        }

        public void Init(Document doc)
        {
            this.Clear();
            this.CollectTexturesFolders();
            this.AddDocumentPath(doc);
            this.currentDocumentID = ((object)doc).GetHashCode();
            this.affectedMaterials.Add(0UL);
        }

        public bool MaterialExist(ulong matUID) => this.materialLibrary.ContainsKey(matUID);

        public MaterialInfo GetMaterial(ulong matUID)
        {
            MaterialInfo materialInfo = (MaterialInfo)null;
            return this.materialLibrary.TryGetValue(matUID, out materialInfo) ? materialInfo : this.defaultMaterial;
        }

        public ulong SelectMaterial(ulong matUID, MaterialNode materialNode, Document doc)
        {
            if (!this.materialLibrary.ContainsKey(matUID) && !this.LoadMaterial(ref matUID, materialNode, doc))
                matUID = 0UL;
            return matUID;
        }

        public ulong SelectMaterial(ulong matUID, Face face, Document doc)
        {
            if (face.MaterialElementId == ElementId.InvalidElementId)
                return 0;
            if (!this.materialLibrary.ContainsKey(matUID) && !this.LoadMaterial(ref matUID, face.MaterialElementId, doc))
                matUID = 0UL;
            return matUID;
        }
       

        public bool UpdateMaterial(ulong matUID, Material material)
        {
            MaterialInfo materialInfo = (MaterialInfo)null;
            if (!this.materialLibrary.TryGetValue(matUID, out materialInfo))
                return this.LoadMaterial(ref matUID, material);
            materialInfo.Reset();
            materialInfo.revitMaterial = material;
            this.ImportMaterial(materialInfo);
            this.affectedMaterials.Add(matUID);
            return true;
        }

        public bool DeleteMaterial(ulong matUID)
        {
            MaterialInfo matInfo = (MaterialInfo)null;
            if (!this.materialLibrary.TryGetValue(matUID, out matInfo))
                return false;
            this.MaterialDeleteAssets(matInfo);
            this.materialLibrary.Remove(matUID);
            return true;
        }

        public bool NeedUpdate() => this.affectedMaterials.Count != 0;

        public void UpdateMaterials()
        {           
            foreach (ulong affectedMaterial in this.affectedMaterials)
            {
                MaterialInfo material = this.GetMaterial(affectedMaterial);
                try
                {                    
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(string.Format("[ActLogger]: MaterialManager::UpdateMaterials(): {0}:{1} - {2}", (object)affectedMaterial, (object)material.Name, (object)ex.Message));
                }
            }
            this.affectedMaterials.Clear();
        }

        protected void MaterialDeleteAssets(MaterialInfo matInfo)
        {
            long materialId = (long)matInfo.materialID;
        }

        private void AddDocumentPath(Document document)
        {
            try
            {
                string directoryName = Path.GetDirectoryName(document.PathName);
                if (this.textureFolders.Contains(directoryName))
                    return;
                this.textureFolders.Add(directoryName);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("[ActLogger]: RevitMaterialManager::AddDocumentPath(): {0}", (object)ex.Message));
            }
        }


        private void CollectTexturesFolders()
        {
            try
            {
                RegistryKey registryKey1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey("SOFTWARE\\Wow6432Node\\Autodesk\\ADSKAssetLibrary");
                if (registryKey1 != null)
                {
                    string[] subKeyNames = registryKey1.GetSubKeyNames();
                    for (int index = 0; index < registryKey1.SubKeyCount; ++index)
                    {
                        RegistryKey registryKey2 = registryKey1.OpenSubKey(subKeyNames[index]);
                        string path = (string)registryKey2.GetValue("LibraryPaths");
                        if (path != null)
                        {
                            string str = Path.GetDirectoryName(path) + "\\assetlibrary_base.fbm";
                            if (!this.textureFolders.Contains(str))
                                this.textureFolders.Add(str);
                        }
                        registryKey2.Close();
                    }
                    registryKey1.Close();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("[ActLogger]: RevitMaterialManager::CollectTexturesFolders('ADSKAssetLibrary'): {0}", (object)ex.Message));
            }
            try
            {
                RegistryKey registryKey3 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey("SOFTWARE\\Wow6432Node\\Autodesk\\ADSKTextureLibrary");
                if (registryKey3 != null)
                {
                    string[] subKeyNames = registryKey3.GetSubKeyNames();
                    for (int index = 0; index < registryKey3.SubKeyCount; ++index)
                    {
                        RegistryKey registryKey4 = registryKey3.OpenSubKey(subKeyNames[index]);
                        string path = (string)registryKey4.GetValue("LibraryPaths");
                        if (path != null)
                        {
                            string str = Path.GetDirectoryName(path) + "\\" + subKeyNames[index] + "\\Mats";
                            if (!this.textureFolders.Contains(str))
                                this.textureFolders.Add(str);
                        }
                        registryKey4.Close();
                    }
                }
                registryKey3.Close();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("[ActLogger]: RevitMaterialManager::CollectTexturesFolders('ADSKTextureLibrary'): {0}", (object)ex.Message));
            }
            this.CollectADSKTextureLibraryNew("SOFTWARE\\Wow6432Node\\Autodesk\\ADSKTextureLibraryNew");
            this.CollectADSKTextureLibraryNew("SOFTWARE\\Wow6432Node\\Autodesk\\ADSKPrismTextureLibraryNew");
        }

        private void CollectADSKTextureLibraryNew(string v)
        {
            try
            {
                RegistryKey registryKey1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(v);
                if (registryKey1 != null)
                {
                    string[] subKeyNames1 = registryKey1.GetSubKeyNames();
                    for (int index1 = 0; index1 < registryKey1.SubKeyCount; ++index1)
                    {
                        RegistryKey registryKey2 = registryKey1.OpenSubKey(subKeyNames1[index1]);
                        string[] subKeyNames2 = registryKey2.GetSubKeyNames();
                        for (int index2 = 0; index2 < registryKey2.SubKeyCount; ++index2)
                        {
                            RegistryKey registryKey3 = registryKey2.OpenSubKey(subKeyNames2[index2]);
                            string path = (string)registryKey3.GetValue("LibraryPaths");
                            if (path != null)
                            {
                                string str = Path.GetDirectoryName(path) + "\\" + subKeyNames1[index1] + "\\Mats";
                                if (!this.textureFolders.Contains(str))
                                    this.textureFolders.Add(str);
                            }
                            registryKey3.Close();
                        }
                        registryKey2.Close();
                    }
                }
                registryKey1.Close();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("[ActLogger]: RevitMaterialManager::CollectADSKTextureLibraryNew('{0}'): {1}", (object)v, (object)ex.Message));
            }
        }

        private string FixTexturePath(string inputPath)
        {
            if (inputPath.Length == 0)
                return "";
            string file = inputPath;
            if (file.IndexOf('|') >= 0)
                file = file.Remove(file.IndexOf('|'));
            string str = file;
            try
            {
                if (File.Exists(str))
                    return str;
                if (!Path.IsPathRooted(str))
                    str = Path.GetFileName(str);
                if (!File.Exists(str))
                    str = this.FindTextureInTextureFolders(str);              
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("[ActLogger]: RevitMaterialManager::FixTexturePath(): {0}\n[ActLogger]: inputPath({1}), currentPath({2})", (object)ex.Message, (object)inputPath, (object)file));
                str = inputPath;
            }
            return str;
        }

        private string FindTextureInTextureFolders(string file)
        {
            foreach (string textureFolder in (IEnumerable<string>)this.textureFolders)
            {
                if (File.Exists(textureFolder + "\\" + file))
                    return textureFolder + "\\" + file;
            }
            return "";
        }

        protected bool LoadMaterial(ref ulong matUID, MaterialNode node, Document document)
        {
            Element element = document.GetElement(node.MaterialId);
            return element is Material && this.LoadMaterial(ref matUID, (Material)element);
        }

        protected bool LoadMaterial(ref ulong matUID, ElementId matId, Document doc)
        {
            Element element = doc.GetElement(matId);
            return element is Material && this.LoadMaterial(ref matUID, (Material)element);
        }

        protected bool LoadMaterial(ref ulong matUID, Material material)
        {
            if (material == null || !((Element)material).IsValidObject)
                return false;
            if (this.MergeLinkedMaterials && this.linkedMaterialsDictionary.ContainsKey(matUID))
            {
                matUID = this.linkedMaterialsDictionary[matUID];
                this.affectedMaterials.Add(matUID);
                return true;
            }
            Document document = ((Element)material).Document;
            MaterialInfo materialInfo1 = new MaterialInfo();
            materialInfo1.materialID = matUID;
            materialInfo1.revitMaterial = material;
            
            if (!this.ImportMaterial(materialInfo1) && this.MergeIfcMaterials && materialInfo1.Name.Contains("IfcSurfaceStyle"))
            {
                foreach (MaterialInfo materialInfo2 in this.materialLibrary.Values)
                {
                    if ((long)materialInfo2.RenderCRC == (long)materialInfo1.RenderCRC)
                    {
                        matUID = materialInfo2.materialID;
                        this.affectedMaterials.Add(matUID);
                        return true;
                    }
                }
                materialInfo1.Name = string.Format("IfcMaterial #{0:X}", (object)materialInfo1.RenderCRC);
            }
            if (!this.materialLibrary.ContainsKey(matUID))
            {
                if (this.MergeLinkedMaterials)
                {
                    string str = RevitMaterialManager.CleanMaterialName(((Element)material).Name, escape: this.ColladaExporterMode);
                    foreach (MaterialInfo materialInfo3 in this.materialLibrary.Values)
                    {
                        if ((long)materialInfo3.RenderCRC == (long)materialInfo1.RenderCRC && !(materialInfo3.Name != str))
                        {
                            this.linkedMaterialsDictionary[matUID] = materialInfo3.materialID;
                            matUID = materialInfo3.materialID;
                            this.affectedMaterials.Add(matUID);
                            return true;
                        }
                    }
                }
                this.materialLibrary.Add(matUID, materialInfo1);
            }
            else
            {                
                this.materialLibrary[matUID] = materialInfo1;
            }
            this.affectedMaterials.Add(matUID);
            return true;
        }

        private bool ImportMaterial(MaterialInfo materialInfo)
        {
            bool flag1 = true;
            string strB = "";
            try
            {
                Material revitMaterial = materialInfo.revitMaterial;
                Document document = ((Element)revitMaterial).Document;
                string str1 = RevitMaterialManager.CleanMaterialName(((Element)revitMaterial).Name, escape: this.ColladaExporterMode);
                int num1 = 0;
                strB = str1;
                bool flag2;
                do
                {
                    flag2 = false;
                    foreach (MaterialInfo materialInfo1 in this.materialLibrary.Values)
                    {
                        if ((long)materialInfo1.materialID != (long)materialInfo.materialID && string.Compare(materialInfo1.Name, strB) == 0)
                        {
                            ++num1;
                            strB = string.Format("{0}({1})", (object)str1, (object)num1);
                            flag2 = true;
                            break;
                        }
                    }
                }
                while (flag2);
                materialInfo.Name = strB;
                if (revitMaterial.Color.IsValid)
                    materialInfo.Color = System.Drawing.Color.FromArgb((int)byte.MaxValue, (int)revitMaterial.Color.Red, (int)revitMaterial.Color.Green, (int)revitMaterial.Color.Blue).ToArgb();
                    
                materialInfo.Shininess = (float)revitMaterial.Shininess;
                //materialInfo.SurfaceType = SurfaceType.imported;
                if (revitMaterial.AppearanceAssetId!= ElementId.InvalidElementId)
                {
                    Asset apperance = (document.GetElement(revitMaterial.AppearanceAssetId) as AppearanceAssetElement).GetRenderingAsset();
                    if (((AssetProperties)apperance).Size <= 1)
                    {
                        foreach (Asset asset in (IEnumerable<Asset>)document.Application.GetAssets((AssetType)4))
                        {
                            if (asset != null && ((AssetProperty)asset).Name == ((AssetProperty)apperance).Name)
                            {
                                apperance = asset;
                                break;
                            }
                        }
                    }
                    this.AddDocumentPath(document);
                    string str2 = "";
                    if (this.ImportString(apperance, "BaseSchema", ref str2))
                    {
                        if (str2.EndsWith("Schema"))
                            str2 = str2.Substring(0, str2.LastIndexOf("Schema"));
                        materialInfo.Category = str2;
                    }
                    else if (((AssetProperties)apperance).FindByName("generic_diffuse") != null)
                        materialInfo.Category = "Generic";
                    else if (((AssetProperties)apperance).FindByName("glazing_transmittance_color") != null)
                        materialInfo.Category = "Glazing";
                    else if (((AssetProperties)apperance).FindByName("masonrycmu_color") != null)
                        materialInfo.Category = "Masonry";
                    else if (((AssetProperties)apperance).FindByName("hardwood_color") != null)
                        materialInfo.Category = "Hardwood";
                    else if (((AssetProperties)apperance).FindByName("ceramic_color") != null)
                        materialInfo.Category = "Ceramic";
                    else if (((AssetProperties)apperance).FindByName("concrete_color") != null)
                        materialInfo.Category = "Concrete";
                    else if (((AssetProperties)apperance).FindByName("metal_color") != null)
                        materialInfo.Category = "Metal";
                    else if (((AssetProperties)apperance).FindByName("metallicpaint_base_color") != null)
                        materialInfo.Category = "MetallicPaint";
                    else if (((AssetProperties)apperance).FindByName("plasticvinyl_color") != null)
                        materialInfo.Category = "PlasticVinyl";
                    else if (((AssetProperties)apperance).FindByName("solidglass_transmittance") != null)
                        materialInfo.Category = "SolidGlass";
                    else if (((AssetProperties)apperance).FindByName("stone_color") != null)
                        materialInfo.Category = "Stone";
                    else if (((AssetProperties)apperance).FindByName("wallpaint_color") != null)
                        materialInfo.Category = "WallPaint";
                    else if (((AssetProperties)apperance).FindByName("water_tint_color") != null)
                        materialInfo.Category = "Water";
                    else if (((AssetProperties)apperance).FindByName("mirror_tintcolor") != null)
                        materialInfo.Category = "Mirror";
                    
                    switch (materialInfo.Category)
                    {
                        case "Ceramic":
                            this.ImportFromAssetCeramic(apperance, materialInfo);
                            break;
                        case "Concrete":
                            this.ImportFromAssetConcrete(apperance, materialInfo);
                            break;
                        case "Generic":
                            this.ImportFromAssetGeneric(apperance, materialInfo);
                            break;
                        case "Glazing":
                            this.ImportFromAssetGlazing(apperance, materialInfo);
                            //materialInfo.SurfaceType = SurfaceType.glass;
                            break;
                        case "Hardwood":
                            this.ImportFromAssetWood(apperance, materialInfo);
                            break;
                        case "Masonry":
                        case "MasonryCMU":
                            this.ImportFromAssetMasonry(apperance, materialInfo);
                            break;
                        case "Metal":
                            this.ImportFromAssetMetal(apperance, materialInfo);
                            break;
                        case "MetallicPaint":
                            this.ImportFromAssetMetallicpaint(apperance, materialInfo);
                            break;
                        case "Mirror":
                            this.ImportFromAssetMirror(apperance, materialInfo);
                            //materialInfo.SurfaceType = SurfaceType.glass;
                            break;
                        case "PlasticVinyl":
                            this.ImportFromAssetPlastic(apperance, materialInfo);
                            break;
                        case "PrismLayered":
                            this.ImportFromPrismLayered(apperance, materialInfo);
                            break;
                        case "PrismMetal":
                            this.ImportFromPrismMetal(apperance, materialInfo);
                            break;
                        case "PrismOpaque":
                            this.ImportFromPrismOpaque(apperance, materialInfo);
                            break;
                        case "PrismTransparent":
                            this.ImportFromPrismTransparent(apperance, materialInfo);
                            
                            break;
                        case "PrismWood":
                            this.ImportFromPrismWood(apperance, materialInfo);
                            break;
                        case "SolidGlass":
                            this.ImportFromAssetSolidglass(apperance, materialInfo);
                            //materialInfo.SurfaceType = SurfaceType.glass;
                            break;
                        case "Stone":
                            this.ImportFromAssetStone(apperance, materialInfo);
                            break;
                        case "WallPaint":
                            this.ImportFromAssetWallPaint(apperance, materialInfo);
                            break;
                        case "Water":
                            this.ImportFromAssetWater(apperance, materialInfo);
                            //materialInfo.SurfaceType = SurfaceType.water;
                            break;
                        default:
                            this.ImportFromAssetGeneric(apperance, materialInfo);
                            break;
                    }
                    if ((double)materialInfo.Transparency > 2.0)
                        materialInfo.Transparency *= 0.01f;
                }
                else
                    flag1 = false;
                DateTime lastWriteTime;
                if (materialInfo.ColorTexture.Path.Length != 0 && File.Exists(materialInfo.ColorTexture.Path))
                {
                    materialInfo.ColorTexture.IsDirty = true;
                    TextureInfo colorTexture = materialInfo.ColorTexture;
                    lastWriteTime = File.GetLastWriteTime(materialInfo.ColorTexture.Path);
                    long fileTime = lastWriteTime.ToFileTime();
                    colorTexture.ModifyTime = (ulong)fileTime;
                }
                else
                    materialInfo.ColorTexture.Reset();
                if (materialInfo.BumpTexture.Path.Length != 0 && File.Exists(materialInfo.BumpTexture.Path))
                {
                    materialInfo.BumpTexture.IsDirty = true;
                    TextureInfo bumpTexture = materialInfo.BumpTexture;
                    lastWriteTime = File.GetLastWriteTime(materialInfo.BumpTexture.Path);
                    long fileTime = lastWriteTime.ToFileTime();
                    bumpTexture.ModifyTime = (ulong)fileTime;
                }
                else
                    materialInfo.BumpTexture.Reset();
                long num2 = (long)materialInfo.UpdateRenderCRC();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("[ActLogger]: MaterialManager::ImportMaterial({0}): {1}\n\t\t{2}", (object)strB, (object)ex.Message, (object)ex.StackTrace));
            }
            return flag1;
        }

        private T GetAssetProprty<T>(Asset asset, string propertyName)
        {
            for (int index = 0; index < ((AssetProperties)asset).Size; ++index)
            {
                if (!(((AssetProperties)asset).Get(index).Name != propertyName) && ((AssetProperties)asset).Get(index) is T)
                    return (T)Convert.ChangeType((object)((AssetProperties)asset).Get(index), typeof(T));
            }
            return (T)Convert.ChangeType((object)null, typeof(T));
        }

        private bool ImportString(Asset apperance, string propertyName, ref string value)
        {
            AssetPropertyString assetProprty = this.GetAssetProprty<AssetPropertyString>(apperance, propertyName);
            if (assetProprty == null || !((AssetProperty)assetProprty).IsValidObject)
                return false;
            value = assetProprty.Value;
            return true;
        }

        private void ImportEnum(Asset apperance, string propertyName, ref int value)
        {
            AssetPropertyEnum assetProprty = this.GetAssetProprty<AssetPropertyEnum>(apperance, propertyName);
            if (assetProprty == null || !((AssetProperty)assetProprty).IsValidObject)
                return;
            value = assetProprty.Value;
        }

        private void ImportInt(Asset apperance, string propertyName, ref int value)
        {
            AssetPropertyInteger assetProprty = this.GetAssetProprty<AssetPropertyInteger>(apperance, propertyName);
            if (assetProprty == null || !((AssetProperty)assetProprty).IsValidObject)
                this.ImportEnum(apperance, propertyName, ref value);
            else
                value = assetProprty.Value;
        }

        private void ImportFloat(Asset apperance, string propertyName, ref float value)
        {
            AssetPropertyDouble assetProprty = this.GetAssetProprty<AssetPropertyDouble>(apperance, propertyName);
            if (assetProprty == null || !((AssetProperty)assetProprty).IsValidObject)
                return;
            value = (float)assetProprty.Value;
        }

        private void ImportBool(Asset apperance, string propertyName, ref bool value)
        {
            AssetPropertyBoolean assetProprty = this.GetAssetProprty<AssetPropertyBoolean>(apperance, propertyName);
            if (assetProprty == null || !((AssetProperty)assetProprty).IsValidObject)
                return;
            value = assetProprty.Value;
        }
        private void ImportColor(Asset apperance, string propertyName, ref int color)
        {
            AssetPropertyDoubleArray4d assetProprty = this.GetAssetProprty<AssetPropertyDoubleArray4d>(apperance, propertyName);
            if (assetProprty == null || !((AssetProperty)assetProprty).IsValidObject)
                return;
            //color = color;
            //color = System.Drawing.Color.FromArgb((int)byte.MaxValue, (int)(byte)(assetProprty.GetValueAsDoubles()[0] * (double)byte.MaxValue), (int)(byte)(assetProprty.GetValueAsDoubles()[1] * (double)byte.MaxValue), (int)(byte)(assetProprty.GetValueAsDoubles()[2] * (double)byte.MaxValue)).ToArgb();
        }

        private float distanceFromProperty(AssetPropertyDistance propertyDistance)
        {
            float output;

#if R20
            output = (float)UnitUtils.ConvertToInternalUnits(propertyDistance.Value, propertyDistance.DisplayUnitType);

#elif R22
            output = (float)UnitUtils.ConvertToInternalUnits(propertyDistance.Value, propertyDistance.GetUnitTypeId());
#endif

            return output;

        }
        private void ImportTextureAsset(Asset asset, string propertyName, ref TextureInfo textureInfo)
        {
            textureInfo = new TextureInfo();
            if (((AssetProperties)asset).FindByName(propertyName) == null)
                return;
            Asset asset1 = (Asset)null;
            int num = 0;
            while (num < ((AssetProperties)asset).FindByName(propertyName).NumberOfConnectedProperties && !(((AssetProperties)asset).FindByName(propertyName).GetConnectedProperty(num) is Asset))
                ++num;
            if(((AssetProperties)asset).FindByName(propertyName).GetConnectedProperty(num) is Asset){
                asset1 = (AssetProperties)asset.FindByName(propertyName).GetConnectedProperty(num) as Asset;
            }
            if (asset1 == null || ((AssetProperties)asset1).FindByName("unifiedbitmap_Bitmap") == null)
                return;
            textureInfo.Path = (((AssetProperties)asset1).FindByName("unifiedbitmap_Bitmap") as AssetPropertyString).Value;
            textureInfo.Path = this.FixTexturePath(textureInfo.Path);
            AssetPropertyDistance propertyDistance1 = ((AssetProperties)asset1).FindByName("texture_RealWorldScaleX") == null ? ((AssetProperties)asset1).FindByName("unifiedbitmap_RealWorldScaleX") as AssetPropertyDistance : ((AssetProperties)asset1).FindByName("texture_RealWorldScaleX") as AssetPropertyDistance;
            AssetPropertyDistance propertyDistance2 = ((AssetProperties)asset1).FindByName("texture_RealWorldScaleY") == null ? ((AssetProperties)asset1).FindByName("unifiedbitmap_RealWorldScaleY") as AssetPropertyDistance : ((AssetProperties)asset1).FindByName("texture_RealWorldScaleY") as AssetPropertyDistance;
            AssetPropertyDistance propertyDistance3 = ((AssetProperties)asset1).FindByName("texture_RealWorldOffsetX") == null ? ((AssetProperties)asset1).FindByName("unifiedbitmap_RealWorldOffsetX") as AssetPropertyDistance : ((AssetProperties)asset1).FindByName("texture_RealWorldOffsetX") as AssetPropertyDistance;
            AssetPropertyDistance propertyDistance4 = ((AssetProperties)asset1).FindByName("texture_RealWorldOffsetY") == null ? ((AssetProperties)asset1).FindByName("unifiedbitmap_RealWorldOffsetY") as AssetPropertyDistance : ((AssetProperties)asset1).FindByName("texture_RealWorldOffsetY") as AssetPropertyDistance;
            if (propertyDistance1 != null)
                textureInfo.ScaleU = 1f / distanceFromProperty(propertyDistance1);
            if (propertyDistance2 != null)
                textureInfo.ScaleV = 1f / distanceFromProperty(propertyDistance2);
            if (propertyDistance3 != null)
                textureInfo.OffsetU = distanceFromProperty(propertyDistance3);
            if (propertyDistance4 != null)
                textureInfo.OffsetV = distanceFromProperty(propertyDistance4);
            AssetPropertyDouble assetPropertyDouble = ((AssetProperties)asset1).FindByName("texture_WAngle") == null ? ((AssetProperties)asset1).FindByName("unifiedbitmap_WAngle") as AssetPropertyDouble : ((AssetProperties)asset1).FindByName("texture_WAngle") as AssetPropertyDouble;
            if (assetPropertyDouble != null)
            {
                textureInfo.RotationAngle = (float)assetPropertyDouble.Value;
                textureInfo.RotationAngle *= (float)Math.PI / 180f;
            }
            if (((AssetProperties)asset1).FindByName("unifiedbitmap_RGBAmount") == null || !(((AssetProperties)asset1).FindByName("unifiedbitmap_RGBAmount") is AssetPropertyDouble byName))
                return;
            textureInfo.Brightness = (float)byName.Value;
        }

        private void ImportFromPrismLayered(Asset apperance, MaterialInfo materialInfo)
        {
            this.ImportColor(apperance, "layered_diffuse", ref materialInfo.Color);
            this.ImportTextureAsset(apperance, "layered_diffuse", ref materialInfo.ColorTexture);
            this.ImportTextureAsset(apperance, "layered_normal", ref materialInfo.BumpTexture);
        }

        private void ImportFromPrismMetal(Asset apperance, MaterialInfo materialInfo)
        {
            this.ImportColor(apperance, "metal_f0", ref materialInfo.Color);
            this.ImportTextureAsset(apperance, "metal_f0", ref materialInfo.ColorTexture);
            this.ImportTextureAsset(apperance, "surface_normal", ref materialInfo.BumpTexture);
        }

        private void ImportFromPrismOpaque(Asset apperance, MaterialInfo materialInfo)
        {
            this.ImportColor(apperance, "opaque_albedo", ref materialInfo.Color);
            this.ImportFloat(apperance, "opaque_translucency", ref materialInfo.Transparency);
            this.ImportFloat(apperance, "opaque_f0", ref materialInfo.Shininess);
            this.ImportTextureAsset(apperance, "opaque_albedo", ref materialInfo.ColorTexture);
            this.ImportTextureAsset(apperance, "surface_normal", ref materialInfo.BumpTexture);
        }

        private void ImportFromPrismTransparent(Asset apperance, MaterialInfo materialInfo) => this.ImportColor(apperance, "transparent_color", ref materialInfo.Color);

        private void ImportFromPrismWood(Asset apperance, MaterialInfo materialInfo)
        {
            this.ImportColor(apperance, "opaque_albedo", ref materialInfo.Color);
            this.ImportFloat(apperance, "opaque_translucency", ref materialInfo.Transparency);
            this.ImportFloat(apperance, "opaque_f0", ref materialInfo.Shininess);
            this.ImportTextureAsset(apperance, "opaque_albedo", ref materialInfo.ColorTexture);
            this.ImportTextureAsset(apperance, "surface_normal", ref materialInfo.BumpTexture);
        }

        private void ImportFromAssetGeneric(Asset apperance, MaterialInfo materialInfo)
        {
            this.ImportColor(apperance, "generic_diffuse", ref materialInfo.Color);
            this.ImportFloat(apperance, "generic_transparency", ref materialInfo.Transparency);
            this.ImportFloat(apperance, "generic_glossiness", ref materialInfo.Shininess);
            this.ImportBool(apperance, "generic_is_metal", ref materialInfo.IsMetal);
            this.ImportTextureAsset(apperance, "generic_diffuse", ref materialInfo.ColorTexture);
            this.ImportFloat(apperance, "generic_diffuse_image_fade", ref materialInfo.ColorTextureAmount);
            this.ImportTextureAsset(apperance, "generic_bump_map", ref materialInfo.BumpTexture);
            this.ImportFloat(apperance, "generic_bump_amount", ref materialInfo.BumpTextureAmount);
            bool flag = false;
            this.ImportBool(apperance, "common_Tint_toggle", ref flag);
            if (!flag)
                return;
            this.ImportColor(apperance, "common_Tint_color", ref materialInfo.Tint);
        }

        private void ImportFromAssetMirror(Asset apperance, MaterialInfo materialInfo)
        {
            this.ImportColor(apperance, "mirror_tintcolor", ref materialInfo.Color);
            bool flag = false;
            this.ImportBool(apperance, "common_Tint_toggle", ref flag);
            if (!flag)
                return;
            this.ImportColor(apperance, "common_Tint_color", ref materialInfo.Tint);
        }

        private void ImportFromAssetMasonry(Asset apperance, MaterialInfo materialInfo)
        {
            this.ImportColor(apperance, "masonrycmu_color", ref materialInfo.Color);
            materialInfo.Transparency = 0.0f;
            this.ImportFloat(apperance, "refl_gloss", ref materialInfo.Shininess);
            materialInfo.IsMetal = false;
            this.ImportTextureAsset(apperance, "masonrycmu_color", ref materialInfo.ColorTexture);
            materialInfo.ColorTextureAmount = 1f;
            int num = 0;
            this.ImportInt(apperance, "masonrycmu_pattern", ref num);
            if (num != 0)
            {
                this.ImportTextureAsset(apperance, "masonrycmu_pattern_map", ref materialInfo.BumpTexture);
                this.ImportFloat(apperance, "masonrycmu_pattern_height", ref materialInfo.BumpTextureAmount);
            }
            bool flag = false;
            this.ImportBool(apperance, "common_Tint_toggle", ref flag);
            if (!flag)
                return;
            this.ImportColor(apperance, "common_Tint_color", ref materialInfo.Tint);
        }

        private void ImportFromAssetGlazing(Asset apperance, MaterialInfo materialInfo)
        {
            int num = 0;
            this.ImportInt(apperance, "glazing_transmittance_color", ref num);
            switch (num)
            {
                case 0:
                    materialInfo.Color = -1;
                    break;
                case 1:
                    materialInfo.Color = -5052968;
                    break;
                case 2:
                    materialInfo.Color = -9276814;
                    break;
                case 3:
                    materialInfo.Color = -8349249;
                    break;
                case 4:
                    materialInfo.Color = -5052955;
                    break;
                case 5:
                    materialInfo.Color = -6717338;
                    break;
                case 6:
                    this.ImportColor(apperance, "glazing_transmittance_map", ref materialInfo.Color);
                    break;
            }
            materialInfo.Transparency = 0.5f;
            this.ImportFloat(apperance, "refl_gloss", ref materialInfo.Shininess);
            materialInfo.IsMetal = false;
            this.ImportTextureAsset(apperance, "transmittance_map", ref materialInfo.ColorTexture);
            materialInfo.ColorTextureAmount = 1f;
            bool flag = false;
            this.ImportBool(apperance, "common_Tint_toggle", ref flag);
            if (!flag)
                return;
            this.ImportColor(apperance, "common_Tint_color", ref materialInfo.Tint);
        }

        private void ImportFromAssetWood(Asset apperance, MaterialInfo materialInfo)
        {
            this.ImportColor(apperance, "hardwood_color", ref materialInfo.Color);
            materialInfo.Transparency = 0.0f;
            this.ImportFloat(apperance, "glossiness", ref materialInfo.Shininess);
            materialInfo.IsMetal = false;
            this.ImportTextureAsset(apperance, "hardwood_color", ref materialInfo.ColorTexture);
            materialInfo.ColorTextureAmount = 1f;
            int num = 0;
            this.ImportInt(apperance, "hardwood_imperfections", ref num);
            if (num == 2)
            {
                this.ImportTextureAsset(apperance, "hardwood_imperfections_shader", ref materialInfo.BumpTexture);
                this.ImportFloat(apperance, "hardwood_imperfections_amount", ref materialInfo.BumpTextureAmount);
            }
            bool flag = false;
            this.ImportBool(apperance, "common_Tint_toggle", ref flag);
            if (!flag)
                return;
            this.ImportColor(apperance, "common_Tint_color", ref materialInfo.Tint);
        }

        private void ImportFromAssetCeramic(Asset apperance, MaterialInfo materialInfo)
        {
            this.ImportColor(apperance, "ceramic_color", ref materialInfo.Color);
            materialInfo.Transparency = 0.0f;
            this.ImportFloat(apperance, "refl_gloss", ref materialInfo.Shininess);
            materialInfo.IsMetal = false;
            this.ImportTextureAsset(apperance, "ceramic_color", ref materialInfo.ColorTexture);
            this.ImportFloat(apperance, "ceramic_diffuse_image_fade", ref materialInfo.ColorTextureAmount);
            int num1 = 0;
            this.ImportInt(apperance, "ceramic_bump", ref num1);
            if (num1 != 0)
            {
                this.ImportTextureAsset(apperance, "ceramic_bump_map", ref materialInfo.BumpTexture);
                this.ImportFloat(apperance, "ceramic_bump_amount", ref materialInfo.BumpTextureAmount);
            }
            if (materialInfo.BumpTexture.Path.Length == 0)
            {
                int num2 = 0;
                this.ImportInt(apperance, "ceramic_pattern", ref num2);
                if (num2 != 0)
                {
                    this.ImportTextureAsset(apperance, "ceramic_pattern_map", ref materialInfo.BumpTexture);
                    this.ImportFloat(apperance, "ceramic_pattern_amount", ref materialInfo.BumpTextureAmount);
                }
            }
            bool flag = false;
            this.ImportBool(apperance, "common_Tint_toggle", ref flag);
            if (!flag)
                return;
            this.ImportColor(apperance, "common_Tint_color", ref materialInfo.Tint);
        }

        private void ImportFromAssetConcrete(Asset apperance, MaterialInfo materialInfo)
        {
            this.ImportColor(apperance, "concrete_color", ref materialInfo.Color);
            materialInfo.Transparency = 0.0f;
            this.ImportFloat(apperance, "glossiness_asset", ref materialInfo.Shininess);
            materialInfo.IsMetal = false;
            this.ImportTextureAsset(apperance, "concrete_color", ref materialInfo.ColorTexture);
            materialInfo.ColorTextureAmount = 1f;
            int num = 0;
            this.ImportInt(apperance, "concrete_finish", ref num);
            if (num == 4)
            {
                this.ImportTextureAsset(apperance, "concrete_bump_map", ref materialInfo.BumpTexture);
                this.ImportFloat(apperance, "concrete_bump_amount", ref materialInfo.BumpTextureAmount);
            }
            bool flag = false;
            this.ImportBool(apperance, "common_Tint_toggle", ref flag);
            if (!flag)
                return;
            this.ImportColor(apperance, "common_Tint_color", ref materialInfo.Tint);
        }

        private void ImportFromAssetMetal(Asset apperance, MaterialInfo materialInfo)
        {
            int num1 = 0;
            this.ImportInt(apperance, "metal_type", ref num1);
            switch (num1)
            {
                case 0:
                    materialInfo.Color = -5592406;
                    break;
                case 1:
                    this.ImportColor(apperance, "metal_color", ref materialInfo.Color);
                    break;
                case 2:
                    materialInfo.Color = -5592406;
                    break;
                case 3:
                    materialInfo.Color = -1529049;
                    break;
                case 4:
                    materialInfo.Color = -1715344;
                    break;
                case 5:
                    materialInfo.Color = -5268350;
                    break;
                case 6:
                    materialInfo.Color = -5592406;
                    break;
                case 7:
                    materialInfo.Color = -5592406;
                    break;
            }
            materialInfo.Transparency = 0.0f;
            materialInfo.IsMetal = true;
            int num2 = 0;
            this.ImportInt(apperance, "metal_pattern", ref num2);
            if (num2 == 4)
            {
                this.ImportTextureAsset(apperance, "metal_pattern_shader", ref materialInfo.BumpTexture);
                this.ImportFloat(apperance, "metal_pattern_height", ref materialInfo.BumpTextureAmount);
            }
            bool flag = false;
            this.ImportBool(apperance, "common_Tint_toggle", ref flag);
            if (!flag)
                return;
            this.ImportColor(apperance, "common_Tint_color", ref materialInfo.Tint);
        }

        private void ImportFromAssetMetallicpaint(Asset apperance, MaterialInfo materialInfo)
        {
            this.ImportColor(apperance, "metallicpaint_base_color", ref materialInfo.Color);
            materialInfo.Transparency = 0.0f;
            this.ImportFloat(apperance, "metallicpaint_base_highlightspread", ref materialInfo.Shininess);
            materialInfo.IsMetal = true;
            this.ImportTextureAsset(apperance, "metallicpaint_base_color", ref materialInfo.ColorTexture);
            bool flag = false;
            this.ImportBool(apperance, "common_Tint_toggle", ref flag);
            if (!flag)
                return;
            this.ImportColor(apperance, "common_Tint_color", ref materialInfo.Tint);
        }

        private void ImportFromAssetPlastic(Asset apperance, MaterialInfo materialInfo)
        {
            this.ImportColor(apperance, "plasticvinyl_color", ref materialInfo.Color);
            int num = 0;
            this.ImportInt(apperance, "plasticvinyl_type", ref num);
            if (num == 1)
                materialInfo.Transparency = 0.6f;
            this.ImportTextureAsset(apperance, "plasticvinyl_color", ref materialInfo.ColorTexture);
            this.ImportTextureAsset(apperance, "plasticvinyl_bump_map", ref materialInfo.BumpTexture);
            this.ImportFloat(apperance, "plasticvinyl_bump_amount", ref materialInfo.BumpTextureAmount);
            if (materialInfo.BumpTexture.Path.Length == 0)
            {
                this.ImportTextureAsset(apperance, "plasticvinyl_pattern_map", ref materialInfo.BumpTexture);
                this.ImportFloat(apperance, "plasticvinyl_pattern_amount", ref materialInfo.BumpTextureAmount);
            }
            bool flag = false;
            this.ImportBool(apperance, "common_Tint_toggle", ref flag);
            if (!flag)
                return;
            this.ImportColor(apperance, "common_Tint_color", ref materialInfo.Tint);
        }

        private void ImportFromAssetSolidglass(Asset apperance, MaterialInfo materialInfo)
        {
            int num = 0;
            this.ImportInt(apperance, "solidglass_transmittance", ref num);
            switch (num)
            {
                case 0:
                    materialInfo.Color = -1;
                    break;
                case 1:
                    materialInfo.Color = -5052968;
                    break;
                case 2:
                    materialInfo.Color = -9276814;
                    break;
                case 3:
                    materialInfo.Color = -8349249;
                    break;
                case 4:
                    materialInfo.Color = -5052955;
                    break;
                case 5:
                    materialInfo.Color = -6717338;
                    break;
                case 6:
                    this.ImportColor(apperance, "solidglass_transmittance_custom_color", ref materialInfo.Color);
                    break;
            }
            this.ImportFloat(apperance, "solidglass_glossiness", ref materialInfo.Shininess);
            materialInfo.IsMetal = false;
            this.ImportTextureAsset(apperance, "solidglass_bump_map", ref materialInfo.BumpTexture);
            this.ImportFloat(apperance, "solidglass_bump_amount", ref materialInfo.BumpTextureAmount);
            bool flag = false;
            this.ImportBool(apperance, "common_Tint_toggle", ref flag);
            if (!flag)
                return;
            this.ImportColor(apperance, "common_Tint_color", ref materialInfo.Tint);
        }

        private void ImportFromAssetStone(Asset apperance, MaterialInfo materialInfo)
        {
            this.ImportColor(apperance, "stone_color", ref materialInfo.Color);
            materialInfo.IsMetal = false;
            this.ImportTextureAsset(apperance, "stone_color", ref materialInfo.ColorTexture);
            int num1 = 0;
            this.ImportInt(apperance, "stone_bump", ref num1);
            if (num1 == 4)
            {
                this.ImportTextureAsset(apperance, "stone_bump_map", ref materialInfo.BumpTexture);
                this.ImportFloat(apperance, "stone_bump_amount", ref materialInfo.BumpTextureAmount);
            }
            if (materialInfo.BumpTexture.Path.Length == 0)
            {
                int num2 = 0;
                this.ImportInt(apperance, "stone_pattern", ref num2);
                if (num2 != 0)
                {
                    this.ImportTextureAsset(apperance, "stone_pattern_map", ref materialInfo.BumpTexture);
                    this.ImportFloat(apperance, "stone_pattern_amount", ref materialInfo.BumpTextureAmount);
                }
            }
            bool flag = false;
            this.ImportBool(apperance, "common_Tint_toggle", ref flag);
            if (!flag)
                return;
            this.ImportColor(apperance, "common_Tint_color", ref materialInfo.Tint);
        }

        private void ImportFromAssetWallPaint(Asset apperance, MaterialInfo materialInfo)
        {
            this.ImportColor(apperance, "wallpaint_color", ref materialInfo.Color);
            materialInfo.IsMetal = false;
            bool flag = false;
            this.ImportBool(apperance, "common_Tint_toggle", ref flag);
            if (!flag)
                return;
            this.ImportColor(apperance, "common_Tint_color", ref materialInfo.Tint);
        }

        private void ImportFromAssetWater(Asset apperance, MaterialInfo materialInfo)
        {
            materialInfo.Transparency = 0.5f;
            int num1 = 0;
            this.ImportInt(apperance, "water_type", ref num1);
            switch (num1)
            {
                case 0:
                    materialInfo.Color = -10040133;
                    break;
                case 1:
                    materialInfo.Color = -15066598;
                    break;
                case 2:
                case 3:
                case 4:
                    int num2 = 0;
                    this.ImportInt(apperance, "water_tint_enable", ref num2);
                    switch (num2)
                    {
                        case 0:
                            materialInfo.Color = -10040133;
                            break;
                        case 1:
                            materialInfo.Color = -12563154;
                            break;
                        case 2:
                            materialInfo.Color = -15066598;
                            break;
                        case 3:
                            materialInfo.Color = -15066598;
                            break;
                        case 4:
                            materialInfo.Color = -10918349;
                            break;
                        case 5:
                            materialInfo.Color = -12563154;
                            break;
                        case 6:
                            materialInfo.Color = -12563154;
                            break;
                        case 7:
                            this.ImportColor(apperance, "water_tint_color", ref materialInfo.Color);
                            break;
                    }
                    break;
            }
            bool flag = false;
            this.ImportBool(apperance, "common_Tint_toggle", ref flag);
            if (!flag)
                return;
            this.ImportColor(apperance, "common_Tint_color", ref materialInfo.Tint);
        }


    }
}
