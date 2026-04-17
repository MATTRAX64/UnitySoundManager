# SoundManager — Unity Audio Component

Un composant Unity pour gérer la lecture audio sur un GameObject, avec **volume master**, **volumes individuels par clip**, et un **système de zones audio par priorité** avec crossfade automatique.

---

## Fonctionnalités

- **Volume Master** — un seul curseur pour contrôler le volume général
- **Mode Normal** — joue un unique clip en boucle
- **Mode Playlist** — joue une liste de clips dans l'ordre (ou aléatoirement), avec répétition par clip
- **Son 3D / 2D** — blend spatial configurable avec distance max
- **Zones audio par priorité** — quand le joueur entre dans une zone, le son de priorité la plus haute prend le dessus et les autres descendent en arrière-plan avec un fondu progressif
- **Crossfade automatique** — transition douce entre les zones, durée configurable
- **Inspector custom** — interface claire directement dans l'éditeur Unity
- **Contrôle par code** — `Play()`, `Stop()`, `SetMasterVolume()`

---

## Fichiers

| Fichier | Rôle |
|---|---|
| `SoundManager.cs` | Composant principal audio + inspector custom |
| `AudioZoneManager.cs` | Contient `AudioZoneManager` (logique de priorité) et `AudioZoneTrigger` (détection du joueur) |

---

## Installation

1. Copier `SoundManager.cs` et `AudioZoneManager.cs` dans `Assets/Scripts/`.
2. Attacher **SoundManager** et **AudioZoneTrigger** sur chaque GameObject audio.
3. Ajouter un **Collider** sur ce même GameObject et cocher **Is Trigger**.
4. S'assurer que le GameObject du joueur a le bon **Tag** (par défaut `Player`).

> `AudioZoneManager` est un système statique, il n'a pas besoin d'être attaché à un GameObject.

---

## Setup dans la scène

```
Zone_Foret (GameObject)
 ├── SoundManager        ← audio + config priorité
 ├── AudioZoneTrigger    ← détecte le joueur
 └── BoxCollider         ← Is Trigger ✓

Zone_Donjon (GameObject)
 ├── SoundManager        ← priorité plus haute
 ├── AudioZoneTrigger
 └── BoxCollider         ← Is Trigger ✓
```

Quand le joueur entre dans `Zone_Donjon` (priorité plus haute), `Zone_Foret` descend progressivement à son volume de fond, et `Zone_Donjon` monte à son volume plein.

---

## Inspector — Réglages audio

| Champ | Description |
|---|---|
| **Son 3D** | Active le spatial blend. Désactivé = son 2D global. |
| **Distance max** | Portée maximale du son en mode 3D. |
| **Volume Master** | Volume global appliqué à tous les clips (0 à 1). |

---

## Inspector — Zone Audio

| Champ | Description |
|---|---|
| **Priorité** | Nombre entier. Plus il est élevé, plus ce son prend le dessus sur les autres. |
| **Durée du fade (s)** | Durée de la transition quand ce son monte ou descend. |
| **Tag du joueur** | Tag Unity du GameObject joueur à détecter (`Player` par défaut). |
| **Volume fond** | Multiplicateur appliqué quand ce son passe en arrière-plan. `0` = silence total, `0.15` = discret. |

### Règle de priorité

```
Priorité 0  →  son ambiant de base (forêt, ville...)
Priorité 1  →  zone spéciale (donjon, grotte...)
Priorité 2  →  zone encore plus importante (boss, cutscene...)
```

À priorités égales, la dernière zone entrée prend le dessus.

---

## Inspector — Mode Normal

Joue un seul clip en boucle continue.

| Champ | Description |
|---|---|
| **Son à jouer en boucle** | L'`AudioClip` à lire. |
| **Volume** | Volume individuel du clip (0 à 1). |

> Volume réel appliqué = `Volume` × `Volume Master` × `Multiplicateur de zone`

---

## Inspector — Mode Playlist

Joue une liste de clips à la suite.

| Champ | Description |
|---|---|
| **Assets/Sounds/[dossier]** | Sous-dossier à scanner dans `Assets/Sounds/`. |
| **Check Again** | Rescanne le dossier et recharge les clips. |
| **Ordre aléatoire** | Mélange la liste à chaque lecture. |

Chaque entrée de la liste :

| Colonne | Description |
|---|---|
| **Clip** | L'`AudioClip` à jouer. |
| **Volume** | Volume individuel (0 à 1). |
| **Final** | Aperçu du volume réel (`Volume` × `Volume Master`), lecture seule. |
| **x fois** | Nombre de répétitions avant de passer au clip suivant. |

Formats supportés lors du scan : `.wav`, `.mp3`, `.ogg`, `.aiff`, `.aif`

---

## Utilisation par code

```csharp
SoundManager sm = GetComponent<SoundManager>();

// Lancer / arrêter
sm.Play();
sm.Stop();

// Changer le volume master à la volée
sm.SetMasterVolume(0.5f);
```

---

## Comportement automatique

Le composant **joue automatiquement** dès que le GameObject est activé (`OnEnable`) et s'arrête quand il est désactivé (`OnDisable`). Le `AudioZoneTrigger` se déregistre automatiquement du système au même moment.

---

## Structure du projet pour le scan de dossier

```
Assets/
└── Sounds/
    └── MonDossier/
        ├── ambiance.wav
        ├── musique.mp3
        └── effet.ogg
```

Dans l'inspector, écrire `MonDossier` dans le champ dossier puis cliquer **Check Again**.

---

## Comment fonctionne le crossfade

```
Joueur entre dans Zone_Donjon (priorité 1)
        │
        ▼
AudioZoneManager compare les priorités
        │
        ├── Zone_Foret  (priorité 0)  →  FadeToZoneMultiplier(backgroundVolumeMult)
        └── Zone_Donjon (priorité 1)  →  FadeToZoneMultiplier(1.0)

Joueur quitte Zone_Donjon
        │
        ▼
AudioZoneManager recalcule
        │
        ├── Zone_Foret  (priorité 0)  →  FadeToZoneMultiplier(1.0)   ← reprend
        └── Zone_Donjon (priorité 1)  →  FadeToZoneMultiplier(backgroundVolumeMult)
```

Le volume final de chaque son à chaque instant est :

```
volume_final = volume_clip × Volume_Master × multiplicateur_de_zone
```

---

## Dépendances

- Unity 2021.3+ recommandé
- Aucun package externe requis
