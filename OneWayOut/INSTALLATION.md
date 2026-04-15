# One Way Out - Manuel d'installation et de maintenance

## Présentation du jeu

One Way Out est un jeu vidéo massivement multijoueur colocalisé. Jusqu'à 200+ joueurs peuvent jouer simultanément dans une même salle (amphithéâtre ou salle de cours) sur un écran projeté commun, en utilisant leur téléphone comme manette via un simple navigateur web.

Un vaisseau futuriste avance automatiquement sur un parcours infini. À chaque embranchement, les joueurs votent collectivement pour choisir la direction (Gauche / Tout droit / Droite). Le choix majoritaire est appliqué : bon choix = gain de vie, mauvais choix = perte de vie. Le but est d'aller le plus loin possible en obtenant le meilleur score.

## Équipe de développement

- Mohammed CHIBANI BAHI : architecture réseau et serveur WebSocket
- Ylan BENLAADAM : interface mobile et tunneling ngrok
- Mamadou Lamine DIALLO : gameplay Unity 2D

SAÉ S4 - BUT Informatique - 2026

## Architecture technique

    [Téléphones des joueurs] <-- WebSocket --> [Serveur Node.js] <-- WebSocket --> [Unity (écran projeté)]
                            (via tunnel ngrok HTTPS/WSS pour contourner les proxies)

Le serveur Node.js écoute sur le port 8080 et fait office d'intermédiaire : il reçoit les votes des téléphones, les comptabilise, et transmet le résultat à Unity. Il sert également la page HTML de l'interface mobile sur le même port.

## Prérequis

### Machine serveur (PC principal connecté au vidéoprojecteur)
- Système : Windows 10/11, macOS ou Linux
- Node.js v18 ou supérieur : https://nodejs.org
- Unity 6.3 LTS (uniquement pour modifier le jeu, pas nécessaire pour jouer)
- ngrok gratuit : https://ngrok.com (pour contourner les proxies réseau type eduroam)

### Côté joueurs
- Un smartphone avec navigateur web récent (Chrome, Safari, Firefox)
- Connexion internet (aucune application à installer)

### Matériel recommandé pour la projection
- Un vidéoprojecteur ou un grand écran partagé
- Un routeur Wi-Fi ou un partage de connexion si pas de proxy ouvert

## Installation

### 1. Récupérer le projet

    git clone https://github.com/mld224/SAE-S4-TP4A.git
    cd SAE-S4-TP4A

### 2. Installer les dépendances du serveur

    cd OneWayOut/server
    npm install

Cela installe la librairie WebSocket `ws` nécessaire au serveur.

### 3. Configurer ngrok (une seule fois)

- Créer un compte gratuit sur https://ngrok.com
- Récupérer l'authtoken dans le dashboard
- Exécuter la commande suivante avec votre token :

        ngrok config add-authtoken VOTRE_TOKEN_NGROK

### 4. Placer les sprites et sons

Les sprites du vaisseau et les fichiers audio sont déjà inclus dans le dépôt :
- Sprites : `Graphics/SpriteVaisseau/` et `OneWayOut/unity-game/OneWayOut/Assets/Sprite/`
- Sons : `Sound/` (à copier dans `OneWayOut/unity-game/OneWayOut/Assets/Sounds/` si pas encore fait)

## Lancement du jeu

### Étape 1 : démarrer le serveur

Ouvrir un premier terminal dans le dossier du serveur :

    cd OneWayOut/server
    node server.js

Le serveur démarre sur le port 8080. On doit voir :

    Serveur demarre sur le port 8080
    Interface mobile : http://localhost:8080
    En attente de Unity pour lancer les votes...

### Étape 2 : démarrer le tunnel ngrok

Ouvrir un deuxième terminal (n'importe où) :

    ngrok http 8080

ngrok affiche une URL publique du type :

    Forwarding https://xxxxxxxx.ngrok-free.app -> http://localhost:8080

C'est cette URL qui sera utilisée par les téléphones et par Unity.

### Étape 3 : configurer Unity avec l'URL ngrok

Dans le fichier `OneWayOut/unity-game/OneWayOut/Assets/Scripts/WebSocketClient.cs`, ligne 47 environ, modifier la ligne :

    websocket = new WebSocket("wss://xxxxxxxx.ngrok-free.app/");

Remplacer `xxxxxxxx` par l'URL donnée par ngrok (attention à bien mettre `wss://` au lieu de `https://`). Sauvegarder.

### Étape 4 : lancer Unity ou l'exécutable

Deux options :

**Option A : lancer depuis Unity Editor**
- Ouvrir le projet dans Unity
- Ouvrir la scène `Assets/Scenes/SampleScene.unity`
- Cliquer sur Play

**Option B : lancer l'exécutable**
- Double-cliquer sur `OneWayOut.exe` dans le dossier `Executable/`
- Le lobby s'affiche

### Étape 5 : les joueurs se connectent

Chaque joueur ouvre l'URL ngrok dans son navigateur mobile :

    https://xxxxxxxx.ngrok-free.app

L'interface de manette s'affiche automatiquement avec 3 boutons désactivés. Le compteur de joueurs connectés se met à jour en temps réel sur l'écran projeté.

### Étape 6 : lancer la partie

Quand tous les joueurs sont connectés, le présentateur clique sur le bouton **COMMENCER** sur l'écran projeté. La partie démarre, les manettes des téléphones deviennent actives à chaque vote.

## Flux de jeu complet

1. Les joueurs se connectent au lien ngrok depuis leur téléphone
2. Le lobby Unity affiche le nombre de joueurs connectés en temps réel
3. Le présentateur clique sur COMMENCER pour lancer la partie
4. Le vaisseau avance automatiquement
5. À chaque embranchement, un vote de 10 secondes se déclenche
6. Les 3 boutons s'activent sur les téléphones (Gauche / Tout droit / Droite)
7. Le choix majoritaire est appliqué :
   - Bon choix = gain de vie + points de score + vaisseau qui grandit
   - Mauvais choix = perte de vie + vaisseau qui rétrécit + flash rouge + shake de caméra
   - Chemin neutre = pas d'effet, juste la vie qui continue de baisser
8. Le vaisseau traverse un tunnel avec un décor correspondant au choix (nébuleuse verte, violette ou bleue)
9. Retour au centre et prochain embranchement
10. À 0 vie : Game Over, un bouton Recommencer apparaît sur tous les téléphones
11. N'importe quel joueur peut cliquer pour relancer la partie

## Contrôles

### Côté écran projeté (Unity)
- Bouton COMMENCER : lance la partie depuis le lobby
- F11 : bascule plein écran / fenêtré
- Échap : sort du plein écran

### Côté téléphone
- 3 boutons directionnels : vote pendant les embranchements
- Bouton RECOMMENCER : visible en Game Over, relance la partie

## Paramètres modifiables

Tous les paramètres se règlent dans l'inspecteur Unity sans avoir à recompiler.

### Vitesse et difficulté

Sur le composant **LevelGenerator** (GameObject Managers) :
- `distanceEntreEmbranchements` : distance entre 2 embranchements (60 par défaut)
- `longueurTunnel` : longueur du tunnel où le décor est visible (25 par défaut)
- `ecartLateral` : décalage latéral des chemins A et C (6 par défaut)

Sur le composant **PlayerController** :
- `speed` : vitesse initiale du vaisseau (5 par défaut)
- `accelerationParSeconde` : accélération progressive (0.05 par défaut)

### Vie et score

Sur le composant **HealthManager** :
- `vieMax` : vie maximale (100 par défaut)
- `pertesParSeconde` : vitesse de perte de vie (1 par défaut)
- `bonusVie` : gain sur bon choix (20)
- `malusVie` : perte sur mauvais choix (15)

Sur le composant **ScoreManager** :
- `pointsBonChoix` : points par bon choix (100)
- `pointsParSeconde` : points de survie par seconde (5)

### Sons

Sur les composants HealthManager, VoteManager et LobbyManager, tous les AudioClip sont modifiables via glisser-déposer dans l'inspecteur.

## Maintenance

### Ajouter un nouveau son

1. Placer le fichier .mp3 dans `Assets/Sounds/`
2. Sélectionner le GameObject Managers
3. Glisser le fichier audio dans le champ correspondant (HealthManager, VoteManager ou LobbyManager)

### Modifier le décor du jeu

Les prefabs de décor sont dans `Assets/Prefabs/` :
- DecorBon : affiché pour un bon choix
- DecorMauvais : affiché pour un mauvais choix
- DecorNeutre : affiché pour un chemin neutre

Modifier les sprites dans chaque prefab (section SpriteRenderer) pour changer l'apparence.

### Construire un nouvel exécutable

1. File → Build Settings
2. Add Open Scenes
3. Platform : PC Standalone → Windows x86_64
4. Player Settings → Resolution and Presentation :
   - Fullscreen Mode : Fullscreen Window
   - Default Screen Width : 1920
   - Default Screen Height : 1080
5. Cliquer sur Build

## Dépannage

### Le téléphone affiche "Déconnecté"
- Vérifier que le serveur Node.js est bien lancé (`node server.js`)
- Vérifier que ngrok est actif dans un autre terminal
- Vérifier que l'URL ngrok correspond à celle configurée dans WebSocketClient.cs

### Erreur "502 Bad Gateway" en ouvrant le lien ngrok
- Le serveur Node.js n'est pas lancé
- Lancer `node server.js` dans le dossier `OneWayOut/server`

### Erreur "ERR_NGROK_8012"
- Même problème que 502 : le serveur Node.js ne tourne pas sur le port 8080

### Le vaisseau ne bouge pas
- Vérifier que le lobby a bien été quitté (cliquer sur COMMENCER)
- Vérifier dans la console Unity que le message "Connecte au serveur !" est présent
- Vérifier que l'URL ngrok n'a pas changé (elle change à chaque redémarrage de ngrok en version gratuite)

### Le bouton Recommencer ne recharge pas la partie
- Vérifier que le GameOverManager a bien une référence au WebSocketClient
- Vérifier dans la console serveur que le message "Restart demande par un joueur" apparaît

### Les boutons du téléphone restent grisés
- Pas de vote en cours, c'est normal tant qu'Unity n'a pas déclenché un vote
- Vérifier que le vaisseau est bien en mouvement et qu'il arrive à un embranchement

### L'interface est mal alignée en plein écran
- Vérifier que le Canvas a bien le Canvas Scaler en mode "Scale With Screen Size"
- Vérifier que les anchors des éléments UI sont correctement configurés
- Reference Resolution : 1920 x 1080, Match : 0.5

## Limitations connues

- L'URL ngrok change à chaque redémarrage en version gratuite. Il faut la mettre à jour dans WebSocketClient.cs puis rebuild. Une version payante permet d'avoir une URL fixe.
- Le nombre de joueurs connectés simultanés n'est limité que par les ressources du serveur. Testé avec une vingtaine de connexions simultanées sans problème.
- Le jeu nécessite un accès internet car ngrok passe par ses serveurs. Une alternative serait un routeur Wi-Fi local privé.

## Structure du projet

    SAE-S4-TP4A/
    |
    |-- OneWayOut/
    |   |-- server/
    |   |   |-- server.js               (Serveur Node.js + WebSocket)
    |   |   |-- package.json
    |   |
    |   |-- mobile-controller/
    |   |   |-- index.html              (Interface manette téléphone)
    |   |   |-- style.css               (Design néon futuriste)
    |   |   |-- script.js               (Logique WebSocket + votes)
    |   |
    |   |-- unity-game/
    |       |-- OneWayOut/
    |           |-- Assets/
    |               |-- Scripts/        (Tous les scripts C#)
    |               |-- Sprite/         (Sprites du vaisseau et UI)
    |               |-- Scenes/         (Scène principale)
    |               |-- Prefabs/        (Prefabs de décor)
    |               |-- WebSocket/      (Librairie NativeWebSocket)
    |
    |-- Sound/                          (Tous les fichiers audio)
    |-- Graphics/                       (Sprites bruts téléchargés)
    |-- Maquettes/                      (Maquettes UI originales)
    |-- CahierDesCharges.pdf
    |-- OrganisationEtStructurationProjet.pdf
    |-- RapportSujet.pdf
    |-- INSTALLATION.md                 (Ce fichier)

## Licence des assets utilisés

- Sprites du vaisseau : Pack Void Fleet 2 - Nairan (Foozle, Creative Commons CC0)
- Sprites de fond : SBS Seamless Space Backgrounds (CC0)
- Sons : libres de droits depuis pixabay.com et mixkit.co
- Code source : projet étudiant SAÉ S4 2026
