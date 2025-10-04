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
        public AudioSource AudioSource { get; private set; } = AudioSource.EmptySource;

        public Instance(AudioSource audioSource)
        {
            this.AudioSource = audioSource;
        }

        public override string ToString() => nameof(Instance);
    }
}
