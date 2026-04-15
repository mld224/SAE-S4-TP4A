/* ===== IMPORTS =====
   require() → charge un module (une librairie)
   
   http → module integre a Node.js, permet de creer un serveur web classique
     C'est lui qui va servir les fichiers HTML/CSS/JS aux telephones
   
   fs → "file system", module integre, permet de lire des fichiers sur le disque dur
     On l'utilise pour lire index.html, style.css, script.js quand un telephone les demande
   
   path → module integre, permet de manipuler les chemins de fichiers
     Exemple : path.join("/dossier", "fichier.html") → "/dossier/fichier.html"
     Ca gere automatiquement les / et \ selon Windows ou Mac
   
   WebSocket → la librairie "ws" installee avec npm
     Gere les connexions WebSocket (votes en temps reel) */
const http = require('http');
const fs = require('fs');
const path = require('path');
const WebSocket = require('ws');

const PORT = 8080;
const MOBILE_DIR = path.join(__dirname, '..', 'mobile-controller');

const MIME_TYPES = {
  '.html': 'text/html',
  '.css': 'text/css',
  '.js': 'text/javascript'
};

const httpServer = http.createServer((req, res) => {
  let fichier = req.url === '/' ? '/index.html' : req.url;
  const cheminFichier = path.join(MOBILE_DIR, fichier);
  const extension = path.extname(cheminFichier);
  const typeMime = MIME_TYPES[extension] || 'text/plain';

  fs.readFile(cheminFichier, (erreur, contenu) => {
    if (erreur) {
      res.writeHead(404);
      res.end('Fichier non trouve');
      return;
    }
    res.writeHead(200, { 'Content-Type': typeMime });
    res.end(contenu);
  });
});

const wss = new WebSocket.Server({ server: httpServer });

let votes = { A: 0, B: 0, C: 0 };
let voteOuvert = false;
const clients = new Set();

function broadcast(message) {
  clients.forEach((client) => {
    if (client.readyState === WebSocket.OPEN) {
      client.send(message);
    }
  });
}

/* Envoie le compteur de joueurs a tous les clients (Unity + tels)
   Unity l'utilise pour afficher le nombre de joueurs dans le lobby */
function broadcastPlayerCount() {
  broadcast(JSON.stringify({ type: 'player_count', count: clients.size }));
}

function lancerVote(duree = 10) {
  votes = { A: 0, B: 0, C: 0 };
  voteOuvert = true;
  broadcast(JSON.stringify({ type: 'vote_start', duree: duree }));
  console.log(`Vote lance pour ${duree} secondes...`);

  setTimeout(() => {
    voteOuvert = false;
    const resultat = Object.entries(votes).sort((a, b) => b[1] - a[1])[0][0];
    broadcast(JSON.stringify({ type: 'vote_result', resultat: resultat, details: votes }));
    console.log(`Vote termine ! Resultat: ${resultat}`, votes);
  }, duree * 1000);
}

wss.on('connection', (ws) => {
  clients.add(ws);
  console.log(`Joueur connecte ! Total: ${clients.size}`);

  /* Nouveau : on envoie le compteur a jour a tous les clients */
  broadcastPlayerCount();

  ws.on('message', (message) => {
    const data = message.toString();

    if (data.startsWith("START_VOTE")) {
      const parts = data.split(":");
      const duree = parseInt(parts[1]) || 10;
      lancerVote(duree);
    }

    if (data === "GAME_OVER") {
      voteOuvert = false;
      broadcast(JSON.stringify({ type: 'game_over' }));
      console.log("Game Over recu, bouton Recommencer affiche sur les tels");
    }

    if (data === "RESTART") {
      broadcast(JSON.stringify({ type: 'restart' }));
      console.log("Restart demande par un joueur, partie relancee");
    }

    /* Nouveau : le presenteur a clique sur Commencer dans Unity
       → on previent tous les tels que le jeu a commence */
    if (data === "GAME_START") {
      broadcast(JSON.stringify({ type: 'game_start' }));
      console.log("Partie lancee par le presenteur");
    }

    if (voteOuvert && ['A', 'B', 'C'].includes(data)) {
      votes[data]++;
      console.log(`Vote recu: ${data} | A:${votes.A} B:${votes.B} C:${votes.C}`);
    }
  });

  ws.on('close', () => {
    clients.delete(ws);
    console.log(`Joueur deconnecte. Total: ${clients.size}`);
    /* Nouveau : on previent que le compteur a baisse */
    broadcastPlayerCount();
  });
});

httpServer.listen(PORT, () => {
  console.log(`Serveur demarre sur le port ${PORT}`);
  console.log(`Interface mobile : http://localhost:${PORT}`);
  console.log(`En attente de Unity pour lancer les votes...`);
});