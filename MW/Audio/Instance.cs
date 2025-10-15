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
        public SongElement SongElement { get; init; }
        public double Offset { get; private set; }

        private Instance(SongElement songElement, double offset)
        {
            SongElement = songElement;
            Offset = offset;
        }

        public static Instance CreateFrom(SongElement songElement, double offset = 0D)
        {
            if (songElement is Song)
            {
                throw new RunException("Cannot add Song as Instance");
            }

            return new Instance(songElement, offset);
        }

        public override string ToString() => nameof(Instance);
    }
}
