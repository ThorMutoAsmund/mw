using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MW.Audio
{
    public abstract class SongElement
    {
        public Guid Hash { get; protected set; } = Guid.NewGuid();
    }
}
