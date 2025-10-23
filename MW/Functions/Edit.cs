using MW.Audio;
using MW.Helpers;
using MW.Parsing;
using NAudio.Wave;
using System.ComponentModel;

namespace MW.Functions
{
    public static class Edit
    {
        [Function(name: "cut", astType: AstType.Sample, description: "Cut a part of a sample")]
        public static Sample Cut(MethodContext context)
        {
            Func.ValidateArgCnt(nameof(Cut), context, numberOfArgs: 3);

            var audioSource = context.Args[0].Evaluate<AudioSource>(context.Thread);
            if (audioSource == null)
            {
                throw new RunException($"{nameof(Cut)} first argument must be a {typeof(AudioSource)}");
            }

            var time = 0D;
            var duration = (double?)null;
            for (var paramNo = 1; paramNo <= 2; ++paramNo)
            {
                var param = context.Args[paramNo].EvaluateDouble(context.Thread);
                if (context.Args[paramNo].Type == AstType.Time)
                {
                    time = param;
                }
                else if (context.Args[paramNo].Type == AstType.Duration)
                {
                    duration = param;
                }
                else
                {
                    throw new RunException($"{nameof(Cut)} illegal param type {context.Args[paramNo].Type}");
                }
            }

            if (!duration.HasValue)
            {
                throw new RunException($"{nameof(Cut)} duration must be set");
            }

            if (duration.Value < 0 ||time < 0)
            {
                throw new RunException($"{nameof(Cut)} duration and time must both be positive");
            }

            // Get data
            var sourceData = audioSource.GetData();
            var slicedData = SliceByTime(sourceData, Env.Song.Format, TimeSpan.FromSeconds(time), TimeSpan.FromSeconds(duration.Value));
            var sample = new Sample(slicedData, Guid.NewGuid().ToString());

            return sample;
        }

        /// <summary>
        /// Returns a new float[] that contains the segment [startTime .. startTime+duration)
        /// from an interleaved float32 buffer described by <paramref name="format"/>.
        /// Times are clamped to the source; result is frame-aligned.
        /// </summary>
        private static float[] SliceByTime(float[] source, WaveFormat format, TimeSpan startTime, TimeSpan duration)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (format is null)
            {
                throw new ArgumentNullException(nameof(format));
            }

            if (duration <= TimeSpan.Zero)
            {
                return Array.Empty<float>();
            }

            int channels = format.Channels;
            int sampleRate = format.SampleRate;
            if (channels <= 0 || sampleRate <= 0)
            {
                throw new ArgumentException("WaveFormat must have positive Channels and SampleRate.", nameof(format));
            }

            // frames = samples per channel; float samples are interleaved across channels
            long startFrame = (long)Math.Floor(startTime.TotalSeconds * sampleRate);
            long durationFrames = (long)Math.Ceiling(duration.TotalSeconds * sampleRate);

            if (startFrame < 0)
            {
                startFrame = 0;
            }

            // convert frames -> float sample indices
            long startSampleIndex = startFrame * channels;
            long maxSamples = source.LongLength;
            if (startSampleIndex >= maxSamples)
            {
                return Array.Empty<float>();
            }

            long desiredSamples = durationFrames * channels;

            // Clamp to available samples
            long available = maxSamples - startSampleIndex;
            long take = Math.Min(available, desiredSamples);
            if (take <= 0)
            {
                return Array.Empty<float>();
            }

            // Copy
            int start = checked((int)startSampleIndex);
            int count = checked((int)take);

            var dest = new float[count];
            Array.Copy(source, start, dest, 0, count);
            
            return dest;
        }
    }
}
