using MW.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MW.Audio
{
    public class Instance 
    {
        public Guid Hash { get; protected set; } = Guid.Empty;
        public SongElement SongElement { get; private set; } = AudioSource.EmptySource;
        public double Offset { get; private set; } = 0D;

        public static Instance CreateFrom(SongElement element, double offset = 0D)
        {
            if (element is Song)
            {
                throw new RunException("Cannot add Song as Instance");
            }
            return new Instance()
            {
                SongElement = element,
                Offset = offset
            };            
        }

        public override string ToString() => nameof(Instance);
    }
}
