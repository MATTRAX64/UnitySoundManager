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
        EditorGUILayout.Space(4);

        // ── Toggle NORMAL / PLAYLIST ───────────────────────────────────────
        EditorGUILayout.Space(6);
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
            // ── MODE NORMAL ───────────────────────────────────────────────
            EditorGUILayout.LabelField("Boucle simple", EditorStyles.boldLabel);

            SerializedProperty nc = serializedObject.FindProperty("normalClip");
            SerializedProperty nv = serializedObject.FindProperty("normalVolume");

            EditorGUILayout.PropertyField(nc, new GUIContent("Son a jouer en boucle"));

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Volume", GUILayout.Width(55));
            nv.floatValue = EditorGUILayout.Slider(nv.floatValue, 0f, 1f);
            EditorGUILayout.EndHorizontal();

            // Apercu volume final
            float finalNormal = sm.normalVolume * sm.masterVolume;
            EditorGUILayout.HelpBox($"Volume final applique : {finalNormal:F2}  (individuel {sm.normalVolume:F2} × master {sm.masterVolume:F2})", MessageType.None);
        }
        else
        {
            // ── MODE PLAYLIST ─────────────────────────────────────────────
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

            // ── Liste des clips ───────────────────────────────────────────
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

                // En-tete
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Clip",      EditorStyles.miniLabel, GUILayout.MinWidth(140));
                EditorGUILayout.LabelField("Volume",    EditorStyles.miniLabel, GUILayout.Width(90));
                EditorGUILayout.LabelField("Final",     EditorStyles.miniLabel, GUILayout.Width(38));
                EditorGUILayout.LabelField("x fois",    EditorStyles.miniLabel, GUILayout.Width(46));
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

                    // Affichage volume final (lecture seule)
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

        // ── Boutons Play / Stop (runtime) ─────────────────────────────────
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

    // Volume Master (point de controle unique)
    [HideInInspector, Range(0f, 1f)] public float masterVolume = 1f;

    // Mode Normal
    [HideInInspector] public AudioClip normalClip;
    [HideInInspector, Range(0f, 1f)] public float normalVolume = 1f;

    // Mode Playlist
    [HideInInspector] public string subFolder   = "MonDossier";
    [HideInInspector] public bool   randomOrder = false;
    [HideInInspector] public List<PlaylistEntry> playlistEntries = new();

    AudioSource _src;
    Coroutine   _routine;

    [Header("Audio Settings")]
    public bool  use3D       = false;
    public float maxDistance = 20f;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Awake()
    {
        _src = GetComponent<AudioSource>();
        _src.playOnAwake = false;
        ApplyAudioSettings();
    }

    void OnEnable()  => Play();
    void OnDisable() => StopAll();

    // ── API publique ──────────────────────────────────────────────────────────

    /// <summary>
    /// Modifie le volume master a la volee (0 a 1).
    /// Met a jour le volume en cours de lecture immediatement.
    /// </summary>
    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);
        // Si en mode Normal et en lecture, on applique directement
        if (!playlistMode && _src != null && _src.isPlaying)
            _src.volume = normalVolume * masterVolume;
        // En mode Playlist le volume est mis a jour au prochain clip
        // (la coroutine relit masterVolume a chaque clip)
    }

    void ApplyAudioSettings()
    {
        if (_src == null) return;

        _src.minDistance = 0f;
        _src.rolloffMode = AudioRolloffMode.Linear;

        if (use3D)
        {
            _src.spatialBlend = 1f;
            _src.maxDistance  = maxDistance;
        }
        else
        {
            _src.spatialBlend = 0f;
        }
    }

    public void Play()
    {
        StopAll();
        ApplyAudioSettings();
        if (!playlistMode) PlayNormal();
        else               StartPlaylist();
    }

    public void Stop() => StopAll();

    /// <summary>Rescanne Assets/Sounds/<subFolder> et remplit la liste.</summary>
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

    // ── Prive ─────────────────────────────────────────────────────────────────

    void PlayNormal()
    {
        if (normalClip == null)
        {
            Debug.LogWarning("[SoundManager] Mode Normal : aucun clip selectionne.");
            return;
        }
        _src.loop   = true;
        _src.clip   = normalClip;
        _src.volume = normalVolume * masterVolume;   // <-- volume final
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
                _src.volume = entry.volume * masterVolume;   // <-- volume final
                _src.Play();

                yield return new WaitWhile(() => _src.isPlaying);
                yield return new WaitForSeconds(0.04f);
            }
        }
    }

    void StopAll()
    {
        if (_routine != null) { StopCoroutine(_routine); _routine = null; }
        if (_src     != null) { _src.Stop(); _src.loop = false; }
    }

    void OnValidate()
    {
        if (_src == null) _src = GetComponent<AudioSource>();
        ApplyAudioSettings();
        // Mise a jour immediate du volume en mode Normal pendant l'edition
        if (!playlistMode && _src != null && _src.isPlaying)
            _src.volume = normalVolume * masterVolume;
    }
}
