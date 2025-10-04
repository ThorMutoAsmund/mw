using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MW.Audio
{
    public abstract class AudioSource : SongElement
    {
        public static AudioSource EmptySource = new EmptyAudioSource();
    }
}
