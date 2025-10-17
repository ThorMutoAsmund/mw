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
        public AudioSource AudioSource { get; init; }
        public double Offset { get; private set; }

        private Instance(AudioSource audioSource, double offset)
        {
            AudioSource = audioSource;
            Offset = offset;
        }

        public static Instance CreateFrom(AudioSource audioSource, double offset = 0D)
        {
            if (audioSource is Song)
            {
                throw new RunException("Cannot add Song as Instance");
            }

            return new Instance(audioSource, offset);
        }

        public override string ToString() => nameof(Instance);
    }
}
