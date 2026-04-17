using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
//  AudioZoneManager
//
//  Singleton statique qui centralise la logique de priorite entre zones audio.
//
//  Fonctionnement :
//  1. Chaque SoundManager se registre / deregistre automatiquement (OnEnable/OnDisable).
//  2. Le AudioZoneTrigger signale les entrees/sorties du joueur.
//  3. Le manager calcule quelle zone a la priorite la plus haute parmi celles
//     ou se trouve le joueur, et demarre les fades en consequence.
//
//  Aucun GameObject ne doit porter ce script : c'est un systeme purement statique.
// ─────────────────────────────────────────────────────────────────────────────
public static class AudioZoneManager
{
    // Tous les SoundManagers actifs dans la scene
    static readonly HashSet<SoundManager> _allManagers = new();

    // Zones ou se trouve actuellement le joueur
    static readonly HashSet<SoundManager> _activeZones = new();

    // Zone actuellement dominante (plus haute priorite)
    static SoundManager _dominant;

    // ── Registration ─────────────────────────────────────────────────────────

    public static void Register(SoundManager sm)
    {
        _allManagers.Add(sm);
    }

    public static void Unregister(SoundManager sm)
    {
        _allManagers.Remove(sm);
        _activeZones.Remove(sm);
        Recalculate();
    }

    // ── Appels depuis AudioZoneTrigger ────────────────────────────────────────

    public static void OnPlayerEnter(SoundManager sm)
    {
        _activeZones.Add(sm);
        Recalculate();
    }

    public static void OnPlayerExit(SoundManager sm)
    {
        _activeZones.Remove(sm);
        Recalculate();
    }

    // ── Logique de priorite ───────────────────────────────────────────────────

    static void Recalculate()
    {
        // La zone dominante est celle avec la priorite la plus elevee parmi les zones actives.
        // En cas d'egalite, la derniere entree gagne (ordre d'insertion dans le HashSet).
        SoundManager newDominant = _activeZones.Count > 0
            ? _activeZones.OrderByDescending(s => s.Priority).First()
            : null;

        if (newDominant == _dominant) return;

        _dominant = newDominant;
        ApplyFades();
    }

    static void ApplyFades()
    {
        foreach (var sm in _allManagers)
        {
            if (sm == null) continue;

            bool isDominant = sm == _dominant;

            // La zone dominante monte a 1.0
            // Toutes les autres descendent a leur backgroundVolumeMult
            float target = isDominant ? 1f : sm.backgroundVolumeMult;

            // Si aucune zone n'est active (_dominant == null), toutes remontent a 1.0
            if (_dominant == null) target = 1f;

            sm.FadeToZoneMultiplier(target);
        }
    }
}

// ─────────────────────────────────────────────────────────────────────────────
//  AudioZoneTrigger
//
//  A placer sur le meme GameObject que le SoundManager.
//  Necessite un Collider avec "Is Trigger" coche.
//  Detecte les entrees/sorties du joueur (identifie par son tag).
// ─────────────────────────────────────────────────────────────────────────────
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(SoundManager))]
public class AudioZoneTrigger : MonoBehaviour
{
    SoundManager _sm;

    void Awake()
    {
        _sm = GetComponent<SoundManager>();

        // S'assurer que le collider est bien en mode trigger
        Collider col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            col.isTrigger = true;
            Debug.LogWarning(
                $"[AudioZoneTrigger] Le Collider sur '{gameObject.name}' n'etait pas en mode Trigger. " +
                "Il a ete active automatiquement.");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(_sm.listenerTag)) return;
        AudioZoneManager.OnPlayerEnter(_sm);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(_sm.listenerTag)) return;
        AudioZoneManager.OnPlayerExit(_sm);
    }

    // Securite : si le joueur sort de la scene ou est detruit pendant qu'il est dans la zone
    void OnDisable()
    {
        AudioZoneManager.OnPlayerExit(_sm);
    }
}
