using System;

using Autodesk.Revit.DB;

using System.Runtime.InteropServices;

namespace AstRevitTool.Core.Export
{
    public class MaterialInfo
    {
        public ulong materialID = ulong.MaxValue;
        public Material revitMaterial;       
        public float Transparency;
        public float Shininess;
        public bool isMetal;
        public int Color;
        public int Tint;
        public bool IsMetal;

        public double[] diffuse;
        public float specular;
        public float ior;
        private ulong __renderCRC = ulong.MaxValue;

        public TextureInfo ColorTexture;
        public float ColorTextureAmount = 1f;
        public TextureInfo BumpTexture;
        public float BumpTextureAmount = 1f;

        public string Name { get; set; }

        public string Category { get; set; }

        public MaterialInfo()
        {
            this.materialID = 0UL;
            this.Name = "Default material";
            this.ColorTexture = new TextureInfo();
            this.BumpTexture = new TextureInfo();
            this.Reset();
        }
        public bool NeedSecondUV()
        {
            if (this.BumpTexture.Path.Length <= 0)
                return false;
            return (double)this.ColorTexture.RotationAngle != (double)this.BumpTexture.RotationAngle || (double)this.ColorTexture.OffsetU != (double)this.BumpTexture.OffsetU || (double)this.ColorTexture.OffsetV != (double)this.BumpTexture.OffsetV || (double)this.ColorTexture.ScaleU != (double)this.BumpTexture.ScaleU || (double)this.ColorTexture.ScaleV != (double)this.BumpTexture.ScaleV;
        }
        public void Reset()
        {
            this.Category = "";
            this.diffuse = new double[] { 0.8, 0.8, 0.8, 1 };
            this.specular = 0.2f;
            this.ior = 1.45f;
            this.Transparency = 0.0f;
            this.Shininess = 0.0f;
            this.Color = System.Drawing.Color.FromArgb(255,230,230,230).ToArgb(); ;
            this.Tint = -1;
            this.ColorTexture.Reset();
            this.ColorTextureAmount = 1f;
            this.BumpTexture.Reset();
            this.BumpTextureAmount = 0.0f;

        }

        public ulong RenderCRC
        {
            get
            {
                if (this.__renderCRC == ulong.MaxValue)
                {
                    long num = (long)this.UpdateRenderCRC();
                }
                return this.__renderCRC;
            }
        }

        public ulong UpdateRenderCRC()
        {
            bool flag1 = this.ColorTexture.Path.Length != 0;
            bool flag2 = this.BumpTexture.Path.Length != 0;
            float[] src1 = new float[16]
            {
        this.Transparency,
        this.Shininess,
        flag1 ? this.ColorTextureAmount : 0.0f,
        flag2 ? this.BumpTextureAmount : 0.0f,
        this.ColorTexture.ScaleU,
        this.ColorTexture.ScaleV,
        this.ColorTexture.OffsetU,
        this.ColorTexture.OffsetV,
        this.ColorTexture.RotationAngle,
        this.ColorTexture.Brightness,
        this.BumpTexture.ScaleU,
        this.BumpTexture.ScaleV,
        this.BumpTexture.OffsetU,
        this.BumpTexture.OffsetV,
        this.BumpTexture.RotationAngle,
        this.BumpTexture.Brightness
            };
            int num = src1.Length * 4;
            int[] src2 = new int[5]
            {
        this.Color,
        this.Tint,
        this.IsMetal ? 1 : 0,
        this.ColorTexture.Path.GetHashCode(),
        this.BumpTexture.Path.GetHashCode()
            };
            int count1 = src2.Length * 4;
            ulong[] src3 = new ulong[2]
            {
        this.ColorTexture.ModifyTime,
        this.BumpTexture.ModifyTime
            };
            int count2 = src3.Length * 8;
            int bufferSize = num + count1 + count2;
            byte[] numArray = new byte[bufferSize];
            Buffer.BlockCopy((Array)src1, 0, (Array)numArray, 0, num);
            Buffer.BlockCopy((Array)src2, 0, (Array)numArray, num, count1);
            Buffer.BlockCopy((Array)src3, 0, (Array)numArray, num + count1, count2);
            this.__renderCRC = HashBytes(numArray, bufferSize);
            return this.__renderCRC;
        }

        public static ulong HashBytes(byte[] buffer, int bufferSize)
        {
            try
            {
                byte[] destination = new byte[8];
                IntPtr num1 = Marshal.AllocHGlobal(8);
                IntPtr num2 = Marshal.AllocHGlobal(bufferSize);
                Marshal.Copy(buffer, 0, num2, bufferSize);
                /*
                if (LiveSyncUtils.HashData(num2, (uint)bufferSize, num1, 8U) != 0L)
                    throw new Exception();*/
                Marshal.Copy(num1, destination, 0, 8);
                Marshal.FreeHGlobal(num2);
                Marshal.FreeHGlobal(num1);
                return BitConverter.ToUInt64(destination, 0);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
    }
}
