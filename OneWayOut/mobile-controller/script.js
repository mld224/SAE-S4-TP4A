/* ===== RECUPERATION DES ELEMENTS HTML ===== */
const statusDiv = document.getElementById("status");
const infoDiv = document.getElementById("info");
const feedbackDiv = document.getElementById("feedback");
const btnA = document.getElementById("btnA");
const btnB = document.getElementById("btnB");
const btnC = document.getElementById("btnC");

const allButtons = [btnA, btnB, btnC];

let aDejaVote = false;
let countdownInterval = null;

/* ===== CONNEXION WEBSOCKET =====
   Detecte automatiquement l'adresse du serveur
   et utilise wss:// si on est en https:// (ngrok) */
const wsProtocol = window.location.protocol === 'https:' ? 'wss:' : 'ws:';
const socket = new WebSocket(wsProtocol + '//' + window.location.host);

socket.addEventListener("open", () => {
  statusDiv.textContent = "Connecte";
  statusDiv.classList.remove("disconnected");
  statusDiv.classList.add("connected");
});

socket.addEventListener("close", () => {
  statusDiv.textContent = "Deconnecte";
  statusDiv.classList.remove("connected");
  statusDiv.classList.add("disconnected");
  desactiverBoutons();
});

socket.addEventListener("message", (event) => {
  const data = JSON.parse(event.data);

  if (data.type === "vote_start") {
    lancerVote(data.duree);
  }

  if (data.type === "vote_result") {
    finVote(data.resultat);
  }
});

function lancerVote(duree) {
  aDejaVote = false;
  feedbackDiv.classList.add("hidden");

  allButtons.forEach((btn) => {
    btn.disabled = false;
    btn.classList.remove("voted");
  });

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

function finVote(resultat) {
  clearInterval(countdownInterval);
  desactiverBoutons();

  infoDiv.textContent = "Resultat : " + resultat;
  infoDiv.classList.remove("active");

  setTimeout(() => {
    infoDiv.textContent = "En attente du prochain vote...";
  }, 3000);
}

function desactiverBoutons() {
  allButtons.forEach((btn) => {
    btn.disabled = true;
  });
}

btnA.addEventListener("click", () => envoyerVote("A"));
btnB.addEventListener("click", () => envoyerVote("B"));
btnC.addEventListener("click", () => envoyerVote("C"));

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
