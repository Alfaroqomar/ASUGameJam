using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Playlist Settings")]
    [Tooltip("The first song that always plays when the game starts")]
    [SerializeField] private AudioClip firstSong;

    [Tooltip("All songs in the playlist (will be shuffled after the first song)")]
    [SerializeField] private List<AudioClip> playlist = new List<AudioClip>();

    [Header("Audio Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.5f;

    [Tooltip("Seconds of silence between songs")]
    [SerializeField] private float delayBetweenSongs = 0f;

    [Header("SFX")]
    [SerializeField] private AudioSource sfxSource;

    private AudioSource musicSource;
    private Queue<AudioClip> songQueue = new Queue<AudioClip>();
    private AudioClip lastPlayedSong = null;
    private bool hasPlayedFirstSong = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = GetComponent<AudioSource>();
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }

        musicSource.loop = false;
        musicSource.playOnAwake = false;
        musicSource.volume = musicVolume;
    }

    private void Start()
    {
        StartPlaylist();
    }

    public void StartPlaylist()
    {
        hasPlayedFirstSong = false;
        lastPlayedSong = null;
        songQueue.Clear();
        RefillAndShuffleQueue();
        StartCoroutine(PlaylistCoroutine());
    }

    private void RefillAndShuffleQueue()
    {
        // Create a shuffled copy of playlist
        List<AudioClip> shuffled = new List<AudioClip>(playlist);

        // Fisher-Yates shuffle
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            AudioClip temp = shuffled[i];
            shuffled[i] = shuffled[j];
            shuffled[j] = temp;
        }

        // Make sure first song in new shuffle isn't the last played song
        if (lastPlayedSong != null && shuffled.Count > 1 && shuffled[0] == lastPlayedSong)
        {
            int swapIndex = Random.Range(1, shuffled.Count);
            shuffled[0] = shuffled[swapIndex];
            shuffled[swapIndex] = lastPlayedSong;
        }

        // Add all to queue
        foreach (var clip in shuffled)
        {
            songQueue.Enqueue(clip);
        }
    }

    private AudioClip GetNextSong()
    {
        // Refill queue if empty
        if (songQueue.Count == 0)
        {
            RefillAndShuffleQueue();
        }

        if (songQueue.Count == 0)
        {
            return null;
        }

        AudioClip song = songQueue.Dequeue();
        lastPlayedSong = song;
        return song;
    }

    private IEnumerator PlaylistCoroutine()
    {
        // Preload first song if exists
        if (firstSong != null)
        {
            firstSong.LoadAudioData();
        }

        // Preload first song in queue
        if (songQueue.Count > 0)
        {
            AudioClip firstInQueue = songQueue.Peek();
            firstInQueue.LoadAudioData();
        }

        // Play the handpicked first song
        if (firstSong != null && !hasPlayedFirstSong)
        {
            hasPlayedFirstSong = true;

            // Wait for load if needed
            if (firstSong.loadState != AudioDataLoadState.Loaded)
            {
                yield return new WaitUntil(() => firstSong.loadState == AudioDataLoadState.Loaded);
            }

            musicSource.clip = firstSong;
            musicSource.Play();

            yield return new WaitForSeconds(firstSong.length + delayBetweenSongs);
        }

        // Now play shuffled playlist forever
        while (true)
        {
            AudioClip nextSong = GetNextSong();

            if (nextSong == null)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            // Ensure loaded
            if (nextSong.loadState != AudioDataLoadState.Loaded)
            {
                nextSong.LoadAudioData();
                yield return new WaitUntil(() => nextSong.loadState == AudioDataLoadState.Loaded);
            }

            musicSource.clip = nextSong;
            musicSource.Play();

            // Preload next song in queue while this plays
            if (songQueue.Count == 0)
            {
                RefillAndShuffleQueue();
            }
            if (songQueue.Count > 0)
            {
                AudioClip upcoming = songQueue.Peek();
                if (upcoming.loadState != AudioDataLoadState.Loaded)
                {
                    upcoming.LoadAudioData();
                }
            }

            yield return new WaitForSeconds(nextSong.length + delayBetweenSongs);
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }

    public void PauseMusic()
    {
        if (musicSource != null)
        {
            musicSource.Pause();
        }
    }

    public void ResumeMusic()
    {
        if (musicSource != null)
        {
            musicSource.UnPause();
        }
    }

    public void StopMusic()
    {
        StopAllCoroutines();
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        if (sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
        else if (musicSource != null)
        {
            musicSource.PlayOneShot(clip, volume);
        }
    }

    public static void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (Instance != null)
        {
            Instance.PlaySFX(clip, volume);
        }
    }
}
