using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(SoundManager))]
public class SoundManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SoundManager sm = (SoundManager)target;
        serializedObject.Update();

        EditorGUILayout.Space(6);

        SerializedProperty use3D   = serializedObject.FindProperty("use3D");
        SerializedProperty maxDist = serializedObject.FindProperty("maxDistance");

        EditorGUILayout.PropertyField(use3D, new GUIContent("Son 3D"));

        if (use3D.boolValue)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Distance max", GUILayout.Width(90));
            maxDist.floatValue = EditorGUILayout.FloatField(maxDist.floatValue);
            EditorGUILayout.EndHorizontal();
        }

        // ── Volume Master ──────────────────────────────────────────────────
        EditorGUILayout.Space(6);
        SerializedProperty mv = serializedObject.FindProperty("masterVolume");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Volume Master", GUILayout.Width(100));
        mv.floatValue = EditorGUILayout.Slider(mv.floatValue, 0f, 1f);
        EditorGUILayout.EndHorizontal();

        // ── Zone Audio / Priorite ──────────────────────────────────────────
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Zone Audio", EditorStyles.boldLabel);

        SerializedProperty prio     = serializedObject.FindProperty("zonePriority");
        SerializedProperty fadeTime = serializedObject.FindProperty("fadeDuration");
        SerializedProperty tagProp  = serializedObject.FindProperty("listenerTag");
        SerializedProperty minVol   = serializedObject.FindProperty("backgroundVolumeMult");

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Priorite", GUILayout.Width(120));
        prio.intValue = EditorGUILayout.IntField(prio.intValue);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Duree du fade (s)", GUILayout.Width(120));
        fadeTime.floatValue = Mathf.Max(0.01f, EditorGUILayout.FloatField(fadeTime.floatValue));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Tag du joueur", GUILayout.Width(120));
        tagProp.stringValue = EditorGUILayout.TextField(tagProp.stringValue);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Volume fond", GUILayout.Width(120));
        minVol.floatValue = EditorGUILayout.Slider(minVol.floatValue, 0f, 1f);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.HelpBox(
            "Quand une zone de priorite superieure est active, ce son descend a Volume Master x Volume fond.\n" +
            "Priorite plus haute = prend le dessus. A priorites egales, le dernier entre gagne.",
            MessageType.None);

        EditorGUILayout.Space(6);

        // ── Toggle NORMAL / PLAYLIST ───────────────────────────────────────
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        bool isPlaylist = sm.playlistMode;

        GUI.backgroundColor = !isPlaylist
            ? new Color(0.3f, 0.6f, 1f)
            : new Color(0.22f, 0.22f, 0.22f);
        if (GUILayout.Button("  NORMAL  ", GUILayout.Height(32), GUILayout.Width(120)))
        {
            sm.playlistMode = false;
            EditorUtility.SetDirty(sm);
        }

        GUI.backgroundColor = isPlaylist
            ? new Color(1f, 0.6f, 0.2f)
            : new Color(0.22f, 0.22f, 0.22f);
        if (GUILayout.Button("  PLAYLIST  ", GUILayout.Height(32), GUILayout.Width(120)))
        {
            sm.playlistMode = true;
            EditorUtility.SetDirty(sm);
        }

        GUI.backgroundColor = Color.white;
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(10);

        // ════════════════════════════════════════════════════════════════════
        if (!isPlaylist)
        {
            EditorGUILayout.LabelField("Boucle simple", EditorStyles.boldLabel);

            SerializedProperty nc = serializedObject.FindProperty("normalClip");
            SerializedProperty nv = serializedObject.FindProperty("normalVolume");

            EditorGUILayout.PropertyField(nc, new GUIContent("Son a jouer en boucle"));

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Volume", GUILayout.Width(55));
            nv.floatValue = EditorGUILayout.Slider(nv.floatValue, 0f, 1f);
            EditorGUILayout.EndHorizontal();

            float finalNormal = sm.normalVolume * sm.masterVolume;
            EditorGUILayout.HelpBox(
                $"Volume final : {finalNormal:F2}  (individuel {sm.normalVolume:F2} x master {sm.masterVolume:F2})",
                MessageType.None);
        }
        else
        {
            EditorGUILayout.LabelField("Dossier source", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Assets/Sounds/", GUILayout.Width(102));
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("subFolder"), GUIContent.none);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(3);
            GUI.backgroundColor = new Color(0.35f, 0.8f, 0.35f);
            if (GUILayout.Button("Check Again  —  rescanner le dossier", GUILayout.Height(26)))
            {
                sm.RefreshFolder();
                EditorUtility.SetDirty(sm);
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(8);
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("randomOrder"),
                new GUIContent("Ordre aleatoire"));

            EditorGUILayout.Space(8);

            SerializedProperty list = serializedObject.FindProperty("playlistEntries");

            if (list.arraySize == 0)
            {
                EditorGUILayout.HelpBox(
                    "Aucun clip. Ecrivez le nom du dossier et cliquez Check Again.",
                    MessageType.Info);
            }
            else
            {
                EditorGUILayout.LabelField($"Clips  ({list.arraySize} trouves)", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Clip",   EditorStyles.miniLabel, GUILayout.MinWidth(140));
                EditorGUILayout.LabelField("Volume", EditorStyles.miniLabel, GUILayout.Width(90));
                EditorGUILayout.LabelField("Final",  EditorStyles.miniLabel, GUILayout.Width(38));
                EditorGUILayout.LabelField("x fois", EditorStyles.miniLabel, GUILayout.Width(46));
                GUILayout.Space(26);
                EditorGUILayout.EndHorizontal();

                for (int i = 0; i < list.arraySize; i++)
                {
                    SerializedProperty entry  = list.GetArrayElementAtIndex(i);
                    SerializedProperty clip   = entry.FindPropertyRelative("clip");
                    SerializedProperty vol    = entry.FindPropertyRelative("volume");
                    SerializedProperty repeat = entry.FindPropertyRelative("repeatCount");

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(clip, GUIContent.none, GUILayout.MinWidth(140));
                    vol.floatValue = EditorGUILayout.Slider(vol.floatValue, 0f, 1f, GUILayout.Width(90));

                    float finalVol = vol.floatValue * sm.masterVolume;
                    GUI.enabled = false;
                    EditorGUILayout.FloatField(finalVol, GUILayout.Width(38));
                    GUI.enabled = true;

                    repeat.intValue = Mathf.Max(1,
                        EditorGUILayout.IntField(repeat.intValue, GUILayout.Width(46)));

                    GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
                    if (GUILayout.Button("x", GUILayout.Width(22)))
                        list.DeleteArrayElementAtIndex(i);
                    GUI.backgroundColor = Color.white;
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.Space(2);
                if (GUILayout.Button("+ Ajouter un clip manuellement"))
                {
                    list.InsertArrayElementAtIndex(list.arraySize);
                    var e = list.GetArrayElementAtIndex(list.arraySize - 1);
                    e.FindPropertyRelative("clip").objectReferenceValue = null;
                    e.FindPropertyRelative("volume").floatValue         = 1f;
                    e.FindPropertyRelative("repeatCount").intValue      = 1;
                }
            }
        }

        // ── Boutons Play / Stop ───────────────────────────────────────────
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Controles (Play requis)", EditorStyles.boldLabel);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Lancez Play pour tester.", MessageType.Info);
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = new Color(0.4f, 0.85f, 0.4f);
            if (GUILayout.Button("Play", GUILayout.Height(28))) sm.Play();
            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("Stop", GUILayout.Height(28))) sm.Stop();
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif

// ─── Donnee par clip ──────────────────────────────────────────────────────────
[System.Serializable]
public class PlaylistEntry
{
    public AudioClip clip;
    [Range(0f, 1f)] public float volume      = 1f;
    [Min(1)]        public int   repeatCount = 1;
}

// ─── Composant principal ──────────────────────────────────────────────────────
[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    [HideInInspector] public bool playlistMode = false;

    // Volume Master
    [HideInInspector, Range(0f, 1f)] public float masterVolume = 1f;

    // Mode Normal
    [HideInInspector] public AudioClip normalClip;
    [HideInInspector, Range(0f, 1f)] public float normalVolume = 1f;

    // Mode Playlist
    [HideInInspector] public string subFolder   = "MonDossier";
    [HideInInspector] public bool   randomOrder = false;
    [HideInInspector] public List<PlaylistEntry> playlistEntries = new();

    // ── Zone Audio ────────────────────────────────────────────────────────────
    /// <summary>Priorite de la zone. Plus le chiffre est eleve, plus ce son prend le dessus.</summary>
    [HideInInspector] public int    zonePriority         = 0;
    [HideInInspector] public float  fadeDuration         = 1.5f;
    [HideInInspector] public string listenerTag          = "Player";
    /// <summary>Multiplicateur applique quand ce son passe en arriere-plan (0 = muet total).</summary>
    [HideInInspector, Range(0f, 1f)] public float backgroundVolumeMult = 0.15f;

    [Header("Audio Settings")]
    public bool  use3D       = false;
    public float maxDistance = 20f;

    AudioSource _src;
    Coroutine   _routine;
    Coroutine   _fadeRoutine;

    // Multiplicateur de zone courant (1 = actif / plein volume, backgroundVolumeMult = arriere-plan)
    float _zoneMult = 1f;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Awake()
    {
        _src = GetComponent<AudioSource>();
        _src.playOnAwake = false;
        ApplyAudioSettings();
    }

    void OnEnable()
    {
        AudioZoneManager.Register(this);
        Play();
    }

    void OnDisable()
    {
        AudioZoneManager.Unregister(this);
        StopAll();
    }

    // ── API publique ──────────────────────────────────────────────────────────

    public int Priority => zonePriority;

    public void Play()
    {
        StopAll();
        ApplyAudioSettings();
        if (!playlistMode) PlayNormal();
        else               StartPlaylist();
    }

    public void Stop() => StopAll();

    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);
        ApplyCurrentVolume();
    }

    /// <summary>
    /// Demarre un fondu vers le multiplicateur cible.
    /// 1.0 = plein volume  |  backgroundVolumeMult = arriere-plan
    /// </summary>
    public void FadeToZoneMultiplier(float target)
    {
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeRoutine(target));
    }

    // ── Prive ─────────────────────────────────────────────────────────────────

    void ApplyAudioSettings()
    {
        if (_src == null) return;
        _src.minDistance = 0f;
        _src.rolloffMode = AudioRolloffMode.Linear;
        _src.spatialBlend = use3D ? 1f : 0f;
        if (use3D) _src.maxDistance = maxDistance;
    }

    // Volume reel = volume_clip x masterVolume x _zoneMult
    float EffectiveVolume(float clipVolume) => clipVolume * masterVolume * _zoneMult;

    void ApplyCurrentVolume()
    {
        if (_src == null || playlistMode) return;
        if (_src.isPlaying) _src.volume = EffectiveVolume(normalVolume);
    }

    void PlayNormal()
    {
        if (normalClip == null)
        {
            Debug.LogWarning("[SoundManager] Mode Normal : aucun clip selectionne.");
            return;
        }
        _src.loop   = true;
        _src.clip   = normalClip;
        _src.volume = EffectiveVolume(normalVolume);
        _src.Play();
    }

    void StartPlaylist()
    {
        if (playlistEntries.Count == 0)
        {
            Debug.LogWarning("[SoundManager] Mode Playlist : liste vide.");
            return;
        }
        _src.loop = false;
        _routine  = StartCoroutine(PlaylistRoutine());
    }

    IEnumerator PlaylistRoutine()
    {
        var queue = new List<PlaylistEntry>(playlistEntries);

        if (randomOrder)
            for (int i = queue.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (queue[i], queue[j]) = (queue[j], queue[i]);
            }

        foreach (var entry in queue)
        {
            if (entry.clip == null) continue;

            for (int r = 0; r < entry.repeatCount; r++)
            {
                _src.clip   = entry.clip;
                _src.volume = EffectiveVolume(entry.volume);
                _src.Play();

                // Mise a jour continue du volume (pour les fades en cours de lecture)
                while (_src.isPlaying)
                {
                    _src.volume = EffectiveVolume(entry.volume);
                    yield return null;
                }

                yield return new WaitForSeconds(0.04f);
            }
        }
    }

    IEnumerator FadeRoutine(float target)
    {
        float start   = _zoneMult;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed  += Time.deltaTime;
            _zoneMult = Mathf.Lerp(start, target, elapsed / fadeDuration);
            ApplyCurrentVolume();
            yield return null;
        }

        _zoneMult    = target;
        ApplyCurrentVolume();
        _fadeRoutine = null;
    }

    void StopAll()
    {
        if (_routine     != null) { StopCoroutine(_routine);     _routine     = null; }
        if (_fadeRoutine != null) { StopCoroutine(_fadeRoutine); _fadeRoutine = null; }
        if (_src         != null) { _src.Stop(); _src.loop = false; }
    }

    void OnValidate()
    {
        if (_src == null) _src = GetComponent<AudioSource>();
        ApplyAudioSettings();
        if (!playlistMode && _src != null && _src.isPlaying)
            _src.volume = EffectiveVolume(normalVolume);
    }

    public void RefreshFolder()
    {
        playlistEntries.Clear();

        string fullPath = Path.Combine(Application.dataPath, "Sounds", subFolder);

        if (!Directory.Exists(fullPath))
        {
            Debug.LogWarning($"[SoundManager] Dossier introuvable : Assets/Sounds/{subFolder}");
            return;
        }

        string[] extensions = { "*.wav", "*.mp3", "*.ogg", "*.aiff", "*.aif" };

        foreach (string ext in extensions)
        {
            foreach (string file in Directory.GetFiles(fullPath, ext))
            {
#if UNITY_EDITOR
                string rel = "Assets/Sounds/" + subFolder + "/" + Path.GetFileName(file);
                AudioClip c = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(rel);
#else
                string res = "Sounds/" + subFolder + "/"
                           + Path.GetFileNameWithoutExtension(file);
                AudioClip c = Resources.Load<AudioClip>(res);
#endif
                if (c != null)
                    playlistEntries.Add(new PlaylistEntry
                    {
                        clip        = c,
                        volume      = 1f,
                        repeatCount = 1
                    });
            }
        }

        Debug.Log($"[SoundManager] {playlistEntries.Count} clip(s) depuis Assets/Sounds/{subFolder}/");
    }
}
