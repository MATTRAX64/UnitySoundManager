# SoundManager — Unity Audio Component

Un composant Unity simple pour gérer la lecture audio sur un GameObject, avec un **volume master** global et des volumes individuels par clip. Supporte deux modes : boucle simple ou playlist séquentielle.

---

## Fonctionnalités

- **Volume Master** — un seul curseur pour contrôler le volume général, indépendamment des volumes par clip
- **Mode Normal** — joue un unique clip en boucle
- **Mode Playlist** — joue une liste de clips dans l'ordre (ou aléatoirement), avec répétition par clip
- **Son 3D / 2D** — blend spatial configurable avec distance max
- **Inspector custom** — interface claire directement dans l'éditeur Unity
- **Contrôle par code** — méthodes `Play()`, `Stop()` et `SetMasterVolume()` accessibles depuis n'importe quel script

---

## Installation

1. Copier `SoundManager.cs` dans votre dossier `Assets/Scripts/` (ou n'importe quel dossier de votre projet).
2. Attacher le composant à un GameObject via **Add Component → SoundManager**.
3. Un `AudioSource` est ajouté automatiquement sur le même GameObject.

---

## Inspector

### Réglages audio

| Champ | Description |
|---|---|
| **Son 3D** | Active le spatial blend (1.0). Désactivé = son 2D global. |
| **Distance max** | Portée maximale du son en mode 3D. |
| **Volume Master** | Volume global appliqué à tous les clips (0 à 1). |

---

### Mode Normal

Joue un seul clip en boucle continue.

| Champ | Description |
|---|---|
| **Son à jouer en boucle** | L'`AudioClip` à lire. |
| **Volume** | Volume individuel du clip (0 à 1). |

> Volume final appliqué = `Volume` × `Volume Master`

---

### Mode Playlist

Joue une liste de clips à la suite, un par un.

| Champ | Description |
|---|---|
| **Assets/Sounds/[dossier]** | Chemin du sous-dossier à scanner (dans `Assets/Sounds/`). |
| **Check Again** | Rescanne le dossier et recharge les clips automatiquement. |
| **Ordre aléatoire** | Mélange la liste à chaque lecture. |

Chaque entrée de la liste expose :

| Colonne | Description |
|---|---|
| **Clip** | L'`AudioClip` à jouer. |
| **Volume** | Volume individuel du clip (0 à 1). |
| **Final** | Aperçu du volume réel appliqué (`Volume` × `Volume Master`), en lecture seule. |
| **x fois** | Nombre de répétitions avant de passer au clip suivant. |

Formats supportés lors du scan automatique : `.wav`, `.mp3`, `.ogg`, `.aiff`, `.aif`

---

## Utilisation par code

```csharp
SoundManager sm = GetComponent<SoundManager>();

// Lancer / arrêter la lecture
sm.Play();
sm.Stop();

// Changer le volume master à la volée
sm.SetMasterVolume(0.5f);
```

`SetMasterVolume()` met à jour le volume immédiatement si un clip est en cours de lecture en mode Normal. En mode Playlist, le nouveau volume est appliqué au clip suivant.

---

## Comportement automatique

Le composant joue automatiquement dès que le GameObject est **activé** (`OnEnable`) et s'arrête quand il est **désactivé** (`OnDisable`). Pas besoin d'appeler `Play()` manuellement au démarrage si le GameObject est actif dans la scène.

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

Dans l'inspector, écrire `MonDossier` dans le champ de dossier puis cliquer **Check Again**.

---

## Dépendances

- Unity 2021.3+ recommandé
- Aucun package externe requis
