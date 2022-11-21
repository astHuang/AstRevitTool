using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstRevitTool.Core.Export
{
    public class TextureInfo
    {
        public bool IsDirty { get; set; }

        public string Path { get; set; }

        public float ScaleU { get; set; }

        public float ScaleV { get; set; }

        public float OffsetU { get; set; }

        public float OffsetV { get; set; }

        public float RotationAngle { get; set; }

        public float Brightness { get; set; }

        public ulong ModifyTime { get; set; }

        public TextureInfo() => this.Reset();

        public void Reset()
        {
            this.IsDirty = true;
            this.ModifyTime = 0UL;
            this.Path = "";
            this.ScaleU = 1f;
            this.ScaleV = 1f;
            this.OffsetU = 0.0f;
            this.OffsetV = 0.0f;
            this.RotationAngle = 0.0f;
            this.Brightness = 1f;
        }
    }
}
