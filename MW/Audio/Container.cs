using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MW.Audio
{
    public class Container : SongElement
    {
        public List<Instance> Instances { get; private set; } = [];

        public override string ToString() => $"{nameof(Container)} with {this.Instances.Count} element(s)";
    }
}
