using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayOnceMultiAudio : MonoBehaviour
{
    public AudioSource source;

    public AudioClip clip1, clip2, clip3, clip4, clip5, clip6;

    [Header("Delay")]
    public float delay1 = 0f, delay2 = 0f, delay3 = 0f, delay4 = 0f, delay5 = 0f, delay6 = 0f;

    bool p1, p2, p3, p4, p5, p6;

    Queue<(AudioClip clip, float delay)> queue = new();
    bool playing;

    public void Play1() { if (p1) return; QueuePlay(clip1, delay1); p1 = true; }
    public void Play2() { if (p2) return; QueuePlay(clip2, delay2); p2 = true; }
    public void Play3() { if (p3) return; QueuePlay(clip3, delay3); p3 = true; }
    public void Play4() { if (p4) return; QueuePlay(clip4, delay4); p4 = true; }
    public void Play5() { if (p5) return; QueuePlay(clip5, delay5); p5 = true; }
    public void Play6() { if (p6) return; QueuePlay(clip6, delay6); p6 = true; }

    void QueuePlay(AudioClip c, float d)
    {
        if (c == null) return;
        queue.Enqueue((c, d));
        if (!playing) StartCoroutine(Process());
    }

    IEnumerator Process()
    {
        playing = true;
        while (queue.Count > 0)
        {
            var (c, d) = queue.Dequeue();

            // espera si hay algo sonando
            while (source.isPlaying) yield return null;

            if (d > 0) yield return new WaitForSeconds(d);

            source.clip = c;
            source.Play();

            // espera a que termine
            while (source.isPlaying) yield return null;
        }
        playing = false;
    }
}
