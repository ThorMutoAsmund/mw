using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MW.Audio
{
    public class Sample : AudioSource
    {
        public string SrcPath { get; protected set; } = string.Empty;
        public string SrcName { get; protected set; } = string.Empty;

        public Sample(string src)
        {
            this.SrcPath = src;
            this.SrcName = Path.GetFileName(src);
        }

        public override string ToString() => this.SrcName;
    }
}
