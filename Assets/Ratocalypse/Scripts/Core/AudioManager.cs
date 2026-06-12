// ============================================================
//  AudioManager.cs
//  Ratpocalypse — Core/AudioManager.cs
//
//  Singleton opakowujący FMOD. Wszystkie wywołania dźwięku
//  idą przez tę klasę — nigdzie indziej nie używamy
//  Unity Audio API (AudioSource, AudioClip itp.)
//
//  Wymagania: FMOD for Unity zainstalowany przez Package Manager
//  (fmod.com → pobierz → zaimportuj do projektu Unity 6)
// ============================================================

using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AudioManager : MonoBehaviour
{
    // --------------------------------------------------------
    // Singleton
    // --------------------------------------------------------
    public static AudioManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // --------------------------------------------------------
    // Odtwarzanie jednorazowe (One-Shot)
    // Idealne do: ataków, footstepów, pick-upów
    // --------------------------------------------------------

    /// <summary>Odtwarza dźwięk jednorazowo w danej pozycji 3D.</summary>
    public void PlayOneShot(EventReference sound, Vector3 worldPosition)
    {
        if (sound.IsNull)
        {
            Debug.LogWarning("[AudioManager] Próba odtworzenia pustego EventReference.");
            return;
        }
        RuntimeManager.PlayOneShot(sound, worldPosition);
    }

    /// <summary>Odtwarza dźwięk jednorazowo bez pozycji (UI, muzyka 2D).</summary>
    public void PlayOneShot(EventReference sound)
    {
        if (sound.IsNull) return;
        RuntimeManager.PlayOneShot(sound);
    }

    // --------------------------------------------------------
    // Instance — dźwięki z kontrolą (pętla, parametry, stop)
    // Idealne do: muzyki, ambientu, footstepów w pętli
    // --------------------------------------------------------

    /// <summary>
    /// Tworzy instancję dźwięku FMOD z pełną kontrolą.
    /// Pamiętaj wywołać Release() gdy już niepotrzebna.
    /// </summary>
    public EventInstance CreateInstance(EventReference sound)
    {
        return RuntimeManager.CreateInstance(sound);
    }

    /// <summary>
    /// Inicjalizuje instancję i przyczepia ją do Transform (3D).
    /// Dźwięk będzie śledził pozycję obiektu.
    /// </summary>
    public EventInstance CreateAndAttach(EventReference sound, Transform target)
    {
        var instance = RuntimeManager.CreateInstance(sound);
        RuntimeManager.AttachInstanceToGameObject(instance, target);
        return instance;
    }

    // --------------------------------------------------------
    // Parametry FMOD — sterowanie mikserami / snapshots
    // --------------------------------------------------------

    /// <summary>Ustawia globalny parametr FMOD (np. "MusicIntensity", "Danger").</summary>
    public void SetGlobalParameter(string paramName, float value)
    {
        RuntimeManager.StudioSystem.setParameterByName(paramName, value);
    }

    // --------------------------------------------------------
    // Muzyka — prosty system z crossfade przez parametr FMOD
    // --------------------------------------------------------

    private EventInstance musicInstance;
    private bool musicPlaying = false;

    /// <summary>
    /// Startuje muzykę (event FMOD z wbudowanym crossfade/timeline).
    /// Zatrzymuje poprzednią jeśli grała.
    /// </summary>
    public void PlayMusic(EventReference musicEvent)
    {
        if (musicPlaying)
            StopMusic(allowFadeout: true);

        musicInstance = RuntimeManager.CreateInstance(musicEvent);
        musicInstance.start();
        musicPlaying = true;
    }

    /// <summary>Zatrzymuje aktywną muzykę.</summary>
    public void StopMusic(bool allowFadeout = true)
    {
        if (!musicPlaying) return;
        musicInstance.stop(allowFadeout ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT : FMOD.Studio.STOP_MODE.IMMEDIATE);
        musicInstance.release();
        musicPlaying = false;
    }

    /// <summary>Zmienia parametr aktywnej muzyki (np. przejście do walki).</summary>
    public void SetMusicParameter(string paramName, float value)
    {
        if (!musicPlaying) return;
        musicInstance.setParameterByName(paramName, value);
    }

    // --------------------------------------------------------
    // Cleanup
    // --------------------------------------------------------
    void OnDestroy()
    {
        if (musicPlaying)
            StopMusic(allowFadeout: false);
    }
}