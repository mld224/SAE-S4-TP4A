# 🎮 ONE WAY OUT – Guide de lancement

## 📌 Présentation

Ce projet est un jeu multijoueur local où les joueurs utilisent leur téléphone comme manette pour voter en temps réel.
Le jeu est affiché sur un écran principal via Unity.

---

## ⚙️ Prérequis

Avant de lancer le projet, assurez-vous d’avoir :

* Node.js installé
* Unity installé
* Tous les appareils connectés au **même réseau Wi-Fi**

---

## 🚀 Lancement du projet (étapes complètes)

### 1️⃣ Lancer le serveur WebSocket

Ouvrir un terminal (CMD ou PowerShell) :

```bash
cd OneWayOut/server
node server.js
```

Résultat attendu :

```
Serveur WebSocket lancé sur le port 8080
```

⚠️ Ne pas fermer ce terminal.

---

### 2️⃣ Lancer le serveur web (manette mobile)

Ouvrir un **nouveau terminal** :

```bash
cd OneWayOut/mobile-controller
npx http-server
```

Résultat attendu :

```
Available on:
http://192.168.X.X:8081
```

👉 Retenir l’adresse IP (ex : `192.168.1.188`)

---

### 3️⃣ Connexion des joueurs

Sur les téléphones :

* Se connecter au **même Wi-Fi**
* Ouvrir dans le navigateur :

```
http://192.168.X.X:8081
```

Les joueurs accèdent à la manette (boutons A / B / C).

---

### 4️⃣ Lancer le jeu Unity

* Ouvrir le projet Unity
* Lancer la scène principale
* Cliquer sur **Play**

---

## 🎯 Fonctionnement

* Les joueurs votent via leur téléphone
* Les votes sont envoyés au serveur
* Unity reçoit les votes en temps réel
* Le jeu utilise ces votes pour prendre des décisions

---

## 🛠️ Architecture

```
Téléphone (web)
        ↓
WebSocket
        ↓
Serveur Node.js
        ↓
Unity (jeu)
```

---

## ⚠️ Problèmes courants

### ❌ Les téléphones ne se connectent pas

* Vérifier le Wi-Fi (même réseau)
* Vérifier l’IP dans le navigateur

### ❌ Aucun vote reçu

* Vérifier que `node server.js` est lancé

### ❌ Unity ne reçoit rien

* Vérifier l’IP dans le script Unity

---

## 💡 Conseils pour la démo

* Afficher un QR code avec l’URL du jeu
* Tester avec plusieurs téléphones avant présentation
* Garder les terminaux ouverts

---

## ✅ État actuel

✔ Communication téléphone → serveur
✔ Communication serveur → Unity
✔ Votes en temps réel

---

## 🚧 Prochaines étapes

* Interface utilisateur dans Unity
* Timer de vote
* Système de niveaux
* Gameplay complet

---

# 🎉 Bon jeu !
