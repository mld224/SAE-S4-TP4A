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

/* ===== CONFIGURATION =====
   PORT → le port unique pour tout (site web + WebSocket)
   
   MOBILE_DIR → chemin vers le dossier mobile-controller
     __dirname → variable speciale de Node.js, c'est le dossier ou se trouve CE fichier
     path.join(__dirname, '..', 'mobile-controller') → remonte d'un dossier (..)
     puis entre dans mobile-controller
     Resultat : OneWayOut/server/../mobile-controller = OneWayOut/mobile-controller */
const PORT = 8080;
const MOBILE_DIR = path.join(__dirname, '..', 'mobile-controller');

/* ===== TABLE DES TYPES MIME =====
   Quand le navigateur recoit un fichier, il a besoin de savoir ce que c'est
   Le serveur lui dit via le "Content-Type" (type MIME)
   
   Sans ca, le navigateur recoit style.css mais ne sait pas que c'est du CSS
   et il ne l'applique pas
   
   .html → text/html (page web)
   .css  → text/css (feuille de style)
   .js   → text/javascript (script) */
const MIME_TYPES = {
  '.html': 'text/html',
  '.css': 'text/css',
  '.js': 'text/javascript'
};

/* ===== CREATION DU SERVEUR HTTP =====
   http.createServer(callback) → cree un serveur web
   Le callback est appele A CHAQUE FOIS qu'un navigateur fait une requete
   
   req → la requete du navigateur (quelle page il demande)
     req.url → l'URL demandee, ex: "/" ou "/style.css" ou "/script.js"
   
   res → la reponse qu'on renvoie au navigateur
     res.writeHead(code, headers) → envoie le code HTTP + les entetes
       200 = OK (tout va bien)
       404 = Not Found (fichier pas trouve)
     res.end(contenu) → envoie le contenu et ferme la reponse */
const httpServer = http.createServer((req, res) => {

  /* Si le navigateur demande "/" (la racine), on lui sert index.html
     Sinon on sert le fichier demande (ex: "/style.css" → style.css) */
  let fichier = req.url === '/' ? '/index.html' : req.url;

  /* On construit le chemin complet vers le fichier sur le disque dur
     Exemple : MOBILE_DIR + "/style.css" → "OneWayOut/mobile-controller/style.css" */
  const cheminFichier = path.join(MOBILE_DIR, fichier);

  /* path.extname() → recupere l'extension du fichier
     Exemple : path.extname("style.css") → ".css"
     On cherche le type MIME correspondant, ou "text/plain" par defaut */
  const extension = path.extname(cheminFichier);
  const typeMime = MIME_TYPES[extension] || 'text/plain';

  /* fs.readFile() → lit le contenu du fichier de maniere asynchrone
     Si erreur (fichier pas trouve) → on renvoie 404
     Sinon → on renvoie le contenu avec le bon type MIME */
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

/* ===== CREATION DU SERVEUR WEBSOCKET =====
   On attache le WebSocket au serveur HTTP existant avec { server: httpServer }
   Ca veut dire que les 2 partagent le meme port (8080)
   
   Le navigateur fait d'abord une requete HTTP classique, puis "upgrade"
   la connexion en WebSocket. Le serveur HTTP detecte ca et passe la main
   au serveur WebSocket automatiquement */
const wss = new WebSocket.Server({ server: httpServer });

/* ===== STOCKAGE DES VOTES ===== */
let votes = { A: 0, B: 0, C: 0 };
let voteOuvert = false;
const clients = new Set();

/* ===== FONCTION : ENVOYER UN MESSAGE A TOUS LES CLIENTS =====
   Parcourt tous les clients connectes et envoie le message
   On verifie que le client est toujours connecte (OPEN) avant d'envoyer */
function broadcast(message) {
  clients.forEach((client) => {
    if (client.readyState === WebSocket.OPEN) {
      client.send(message);
    }
  });
}

/* ===== EVENEMENT : UN TELEPHONE SE CONNECTE =====  */
wss.on('connection', (ws) => {
  clients.add(ws);
  console.log(`Joueur connecte ! Total: ${clients.size}`);

  /* Quand ce telephone envoie un message (un vote) */
  ws.on('message', (message) => {
    const data = message.toString();

    if (voteOuvert && ['A', 'B', 'C'].includes(data)) {
      votes[data]++;
      console.log(`Vote recu: ${data} | A:${votes.A} B:${votes.B} C:${votes.C}`);
    }
  });

  /* Quand ce telephone se deconnecte */
  ws.on('close', () => {
    clients.delete(ws);
    console.log(`Joueur deconnecte. Total: ${clients.size}`);
  });
});

/* ===== FONCTION : LANCER UN VOTE ===== */
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

/* ===== DEMARRAGE DU SERVEUR =====
   httpServer.listen(PORT) → le serveur commence a ecouter sur le port 8080
   A partir de ce moment :
     - http://adresse:8080 → sert les fichiers HTML/CSS/JS
     - ws://adresse:8080   → accepte les connexions WebSocket
   Tout sur le meme port */
httpServer.listen(PORT, () => {
  console.log(`Serveur demarre sur le port ${PORT}`);
  console.log(`Interface mobile : http://localhost:${PORT}`);
});

/* ===== TEST AUTOMATIQUE =====
   Lance un vote toutes les 15 secondes pour tester
   A supprimer plus tard quand Unity declenchera les votes */
setInterval(() => {
  lancerVote(10);
}, 15000);
