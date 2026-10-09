using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-1000)]
public sealed class AudioManager : MonoBehaviour
{
    private const string OpenPacksButtonName = "OpenPacksIconButton";

    private static AudioManager instance;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip bgmClip;
    [SerializeField] private AudioClip buttonPressClip;
    [SerializeField] private AudioClip packOpenClip;
    [SerializeField] private AudioClip cardReceiveClip;
    [SerializeField] private AudioClip packRipClip;

    [Header("Volume")]
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.3f;
    [SerializeField, Range(0f, 1f)] private float soundEffectsVolume = 0.8f;

    private readonly List<Button> hookedButtons = new List<Button>();
    private AudioSource musicSource;
    private AudioSource soundEffectsSource;
    private Coroutine buttonHookRoutine;

    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<AudioManager>();

                if (instance == null)
                {
                    GameObject managerObject =
                        new GameObject("[Runtime] AudioManager");
                    instance = managerObject.AddComponent<AudioManager>();
                }
            }

            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        CreateAudioSources();

        SceneManager.sceneLoaded += OnSceneLoaded;
        PlayBgm();
    }

    private void OnDestroy()
    {
        if (instance != this)
        {
            return;
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnhookButtons();
        instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (buttonHookRoutine != null)
        {
            StopCoroutine(buttonHookRoutine);
        }

        buttonHookRoutine = StartCoroutine(HookButtonsAfterSceneStarts());
    }

    private IEnumerator HookButtonsAfterSceneStarts()
    {
        yield return null;
        NameVuforiaPersistentObjects();
        HookAllButtons();
        buttonHookRoutine = null;
    }

    private static void NameVuforiaPersistentObjects()
    {
        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();

        foreach (Transform candidate in transforms)
        {
            if (candidate.parent != null ||
                candidate.gameObject.scene.name != "DontDestroyOnLoad")
            {
                continue;
            }

            Component[] components = candidate.GetComponents<Component>();

            foreach (Component component in components)
            {
                if (component == null)
                {
                    continue;
                }

                string typeName = component.GetType().FullName;

                if (typeName ==
                    "Vuforia.Internal.Core.UnityLifeCycleListener")
                {
                    candidate.name = "[Vuforia] Lifecycle";
                    break;
                }

                if (typeName ==
                    "Vuforia.Internal.Utility.VuforiaCoroutineUtility")
                {
                    candidate.name = "[Vuforia] Coroutine Utility";
                    break;
                }
            }

            if (candidate.name == "TextureBufferCamera")
            {
                candidate.name = "[Vuforia] Texture Buffer Camera";
            }
        }
    }

    private void CreateAudioSources()
    {
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;

        soundEffectsSource = gameObject.AddComponent<AudioSource>();
        soundEffectsSource.playOnAwake = false;
        soundEffectsSource.loop = false;
        soundEffectsSource.spatialBlend = 0f;

        ApplyVolumes();
    }

    private void HookAllButtons()
    {
        UnhookButtons();

        Button[] buttons = FindObjectsByType<Button>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (Button button in buttons)
        {
            if (button.name == OpenPacksButtonName)
            {
                button.onClick.AddListener(PlayPackOpen);
            }
            else
            {
                button.onClick.AddListener(PlayButtonPress);
            }

            hookedButtons.Add(button);
        }
    }

    private void UnhookButtons()
    {
        foreach (Button button in hookedButtons)
        {
            if (button != null)
            {
                button.onClick.RemoveListener(PlayButtonPress);
                button.onClick.RemoveListener(PlayPackOpen);
            }
        }

        hookedButtons.Clear();
    }

    public void PlayBgm()
    {
        if (bgmClip == null || musicSource == null)
        {
            return;
        }

        if (musicSource.clip != bgmClip)
        {
            musicSource.clip = bgmClip;
        }

        if (!musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }

    public void StopBgm()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void PlayButtonPress()
    {
        PlaySoundEffect(buttonPressClip);
    }

    public void PlayPackOpen()
    {
        PlaySoundEffect(packOpenClip);
    }

    public void PlayCardReceived()
    {
        PlaySoundEffect(cardReceiveClip);
    }

    public void PlayPackRip()
    {
        PlaySoundEffect(packRipClip);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        ApplyVolumes();
    }

    public void SetSoundEffectsVolume(float volume)
    {
        soundEffectsVolume = Mathf.Clamp01(volume);
        ApplyVolumes();
    }

    private void PlaySoundEffect(AudioClip clip)
    {
        if (clip != null && soundEffectsSource != null)
        {
            soundEffectsSource.PlayOneShot(clip);
        }
    }

    private void ApplyVolumes()
    {
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }

        if (soundEffectsSource != null)
        {
            soundEffectsSource.volume = soundEffectsVolume;
        }
    }

}
