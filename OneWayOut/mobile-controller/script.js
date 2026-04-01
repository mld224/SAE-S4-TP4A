const statusDiv = document.getElementById("status");
const voteA = document.getElementById("voteA");
const voteB = document.getElementById("voteB");
const voteC = document.getElementById("voteC");

const btnA = document.getElementById("btnA");
const btnB = document.getElementById("btnB");
const btnC = document.getElementById("btnC");
const resetBtn = document.getElementById("resetBtn");

const socket = new WebSocket("ws://192.168.1.188:8080");

socket.addEventListener("open", () => {
  statusDiv.textContent = "Connecté au serveur";
  statusDiv.style.background = "green";
});

socket.addEventListener("close", () => {
  statusDiv.textContent = "Déconnecté du serveur";
  statusDiv.style.background = "red";
});

socket.addEventListener("message", (event) => {
  const data = JSON.parse(event.data);

  if (data.type === "votes") {
    voteA.textContent = data.votes.A;
    voteB.textContent = data.votes.B;
    voteC.textContent = data.votes.C;
  }
});

btnA.addEventListener("click", () => {
  socket.send("A");
});

btnB.addEventListener("click", () => {
  socket.send("B");
});

btnC.addEventListener("click", () => {
  socket.send("C");
});

resetBtn.addEventListener("click", () => {
  socket.send("RESET");
});