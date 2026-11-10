using UnityEngine;

namespace PixelSurvivor
{
    // Short generated cues keep the game audible without external sound assets.
    public static class CombatAudio
    {
        static AudioSource source;
        static AudioClip hit, kill, hurt, level, doom;
        static float nextHit;

        static AudioClip Tone(string name, float frequency, float seconds, float volume, float decay)
        {
            const int rate = 22050;
            int count = Mathf.CeilToInt(rate * seconds);
            var samples = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / rate;
                float envelope = Mathf.Pow(1f - (float)i / count, decay);
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * volume;
            }
            var clip = AudioClip.Create(name, count, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        static AudioClip DoomTone()
        {
            const int rate=22050;
            int count=rate;
            var samples=new float[count];
            float phase=0;
            for(int i=0;i<count;i++)
            {
                float t=i/(float)rate;
                float frequency=Mathf.Lerp(145f,48f,t);
                phase+=2f*Mathf.PI*frequency/rate;
                float envelope=Mathf.Sin(Mathf.PI*t)*.36f;
                samples[i]=(Mathf.Sin(phase)+Mathf.Sin(phase*.5f)*.35f)*envelope;
            }
            var clip=AudioClip.Create("Doom warning",count,1,rate,false);
            clip.SetData(samples,0);return clip;
        }

        static void Ensure()
        {
            if(Object.FindAnyObjectByType<AudioListener>()==null && Camera.main!=null)
                Camera.main.gameObject.AddComponent<AudioListener>();
            if (source != null) return;
            var go = new GameObject("Combat audio");
            Object.DontDestroyOnLoad(go);
            source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.volume = .45f;
            hit = Tone("Hit", 310, .065f, .25f, 2f);
            kill = Tone("Kill", 520, .12f, .34f, 2f);
            hurt = Tone("Player hurt", 140, .24f, .5f, 1.5f);
            level = Tone("Level up", 740, .32f, .4f, 1f);
            doom = DoomTone();
        }

        public static void Hit(bool defeated)
        {
            if (!DisplayOptions.Sound) return;
            Ensure();
            if (!defeated && Time.unscaledTime < nextHit) return;
            nextHit = Time.unscaledTime + .07f;
            source.PlayOneShot(defeated ? kill : hit);
        }

        public static void Hurt() { if (!DisplayOptions.Sound) return; Ensure(); source.PlayOneShot(hurt); }
        public static void LevelUp() { if (!DisplayOptions.Sound) return; Ensure(); source.PlayOneShot(level); }
        public static void Doom() { if (!DisplayOptions.Sound) return; Ensure(); source.PlayOneShot(doom); }
    }
}
