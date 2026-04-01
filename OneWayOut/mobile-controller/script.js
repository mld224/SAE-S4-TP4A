/* ===== RECUPERATION DES ELEMENTS HTML =====
   document.getElementById("id") → va chercher l'element HTML qui a cet id
   On stocke chaque element dans une variable pour pouvoir le modifier plus tard */
const statusDiv = document.getElementById("status");
const infoDiv = document.getElementById("info");
const feedbackDiv = document.getElementById("feedback");
const btnA = document.getElementById("btnA");
const btnB = document.getElementById("btnB");
const btnC = document.getElementById("btnC");

/* ===== TABLEAU DES BOUTONS =====
   On regroupe les 3 boutons dans un tableau
   Ca permet de faire des operations sur les 3 en une seule boucle
   au lieu de repeter 3 fois le meme code */
const allButtons = [btnA, btnB, btnC];

/* ===== VARIABLES D'ETAT =====
   aDejaVote → empeche le joueur de voter 2 fois dans le meme tour
   countdownInterval → stocke le timer du compte a rebours pour pouvoir l'arreter */
let aDejaVote = false;
let countdownInterval = null;

/* ===== CONNEXION WEBSOCKET =====
   new WebSocket("adresse") → ouvre une connexion permanente avec le serveur
   IMPORTANT : l'IP doit etre celle du PC qui fait tourner le serveur Node.js
   En local sur ton PC : ws://localhost:8080
   Sur le reseau Wi-Fi : ws://IP_DU_PC:8080 */
const socket = new WebSocket("ws://192.168.1.188:8080");

/* ===== EVENEMENT : CONNEXION REUSSIE =====
   Se declenche quand le telephone est connecte au serveur
   
   classList.remove("disconnected") → enleve la classe CSS "disconnected"
   classList.add("connected") → ajoute la classe CSS "connected"
   Resultat : le statut passe de rouge a vert */
socket.addEventListener("open", () => {
  statusDiv.textContent = "Connecte";
  statusDiv.classList.remove("disconnected");
  statusDiv.classList.add("connected");
});

/* ===== EVENEMENT : DECONNEXION =====
   Se declenche quand la connexion avec le serveur est perdue
   On remet la classe "disconnected" (rouge) et on desactive les boutons
   car sans serveur, voter ne sert a rien */
socket.addEventListener("close", () => {
  statusDiv.textContent = "Deconnecte";
  statusDiv.classList.remove("connected");
  statusDiv.classList.add("disconnected");
  desactiverBoutons();
});

/* ===== EVENEMENT : MESSAGE RECU DU SERVEUR =====
   Se declenche a chaque fois que le serveur envoie un message
   
   event.data → le message brut en texte (ex: '{"type":"vote_start","duree":10}')
   JSON.parse() → transforme ce texte en objet JS utilisable
     (ex: { type: "vote_start", duree: 10 })
   
   Le serveur envoie 2 types de messages :
     "vote_start" → un nouveau vote commence, avec une duree
     "vote_result" → le vote est termine, avec le resultat gagnant */
socket.addEventListener("message", (event) => {
  const data = JSON.parse(event.data);

  if (data.type === "vote_start") {
    lancerVote(data.duree);
  }

  if (data.type === "vote_result") {
    finVote(data.resultat);
  }
});

/* ===== FONCTION : LANCER UN VOTE =====
   Appelee quand le serveur envoie "vote_start"
   Active les boutons et demarre le compte a rebours
   
   duree → nombre de secondes pour voter (envoye par le serveur) */
function lancerVote(duree) {
  aDejaVote = false;
  feedbackDiv.classList.add("hidden");

  /* On reactive chaque bouton :
     btn.disabled = false → le bouton redevient cliquable
     btn.classList.remove("voted") → on enleve le style grise du tour precedent */
  allButtons.forEach((btn) => {
    btn.disabled = false;
    btn.classList.remove("voted");
  });

  /* On demarre le compte a rebours
     setInterval(fonction, delai) → execute la fonction toutes les X millisecondes
     Ici toutes les 1000ms = 1 seconde, on decremente le compteur
     On stocke l'interval dans countdownInterval pour pouvoir l'arreter plus tard
     avec clearInterval() */
  let tempsRestant = duree;
  infoDiv.textContent = tempsRestant + "s";
  infoDiv.classList.add("active");

  countdownInterval = setInterval(() => {
    tempsRestant--;
    infoDiv.textContent = tempsRestant + "s";

    if (tempsRestant <= 0) {
      clearInterval(countdownInterval);
    }
  }, 1000);
}

/* ===== FONCTION : FIN DU VOTE =====
   Appelee quand le serveur envoie "vote_result"
   Desactive les boutons et affiche le resultat
   
   resultat → le choix gagnant ("A", "B" ou "C")
   
   setTimeout(fonction, delai) → execute la fonction UNE SEULE FOIS apres X ms
     Contrairement a setInterval qui se repete en boucle
     Ici apres 3 secondes, on remet le message d'attente */
function finVote(resultat) {
  clearInterval(countdownInterval);
  desactiverBoutons();

  infoDiv.textContent = "Resultat : " + resultat;
  infoDiv.classList.remove("active");

  setTimeout(() => {
    infoDiv.textContent = "En attente du prochain vote...";
  }, 3000);
}

/* ===== FONCTION : DESACTIVER LES BOUTONS =====
   forEach → parcourt chaque element du tableau et execute la fonction
   btn.disabled = true → rend le bouton non-cliquable
   Le CSS .vote-btn:disabled s'applique automatiquement (opacite 0.3) */
function desactiverBoutons() {
  allButtons.forEach((btn) => {
    btn.disabled = true;
  });
}

/* ===== ECOUTEURS DE CLIC SUR LES BOUTONS =====
   addEventListener("click", fonction) → quand on clique, execute la fonction
   On appelle envoyerVote avec le choix correspondant */
btnA.addEventListener("click", () => envoyerVote("A"));
btnB.addEventListener("click", () => envoyerVote("B"));
btnC.addEventListener("click", () => envoyerVote("C"));

/* ===== FONCTION : ENVOYER UN VOTE =====
   1. Verifie que le joueur n'a pas deja vote (aDejaVote)
   2. Envoie le choix au serveur via WebSocket
   3. Bloque les boutons pour empecher un double vote
   4. Affiche "Vote enregistre !" en vert
   
   socket.send(choix) → envoie le texte au serveur (ex: "A")
   classList.add("voted") → ajoute la classe CSS qui grise le bouton
   classList.remove("hidden") → rend le feedback visible */
function envoyerVote(choix) {
  if (aDejaVote) return;

  socket.send(choix);
  aDejaVote = true;

  allButtons.forEach((btn) => {
    btn.classList.add("voted");
  });

  feedbackDiv.textContent = "Vote enregistre !";
  feedbackDiv.classList.remove("hidden");
}
