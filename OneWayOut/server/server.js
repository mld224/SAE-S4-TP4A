const WebSocket = require("ws");

const wss = new WebSocket.Server({ port: 8080 });

let votes = {
  A: 0,
  B: 0,
  C: 0
};

console.log("Serveur WebSocket lancé sur le port 8080");

wss.on("connection", (ws) => {
  console.log("Un client est connecté");

  ws.send(JSON.stringify({
    type: "votes",
    votes: votes
  }));

  ws.on("message", (message) => {
    const msg = message.toString().trim();

    if (msg === "A" || msg === "B" || msg === "C") {
      votes[msg]++;

      console.log("Vote reçu :", msg);
      console.log("Votes actuels :", votes);

      const payload = JSON.stringify({
        type: "votes",
        votes: votes
      });

      wss.clients.forEach((client) => {
        if (client.readyState === WebSocket.OPEN) {
          client.send(payload);
        }
      });
    } else if (msg === "RESET") {
      votes = { A: 0, B: 0, C: 0 };
      console.log("Votes réinitialisés");

      const payload = JSON.stringify({
        type: "votes",
        votes: votes
      });

      wss.clients.forEach((client) => {
        if (client.readyState === WebSocket.OPEN) {
          client.send(payload);
        }
      });
    } else {
      console.log("Message inconnu :", msg);
    }
  });

  ws.on("close", () => {
    console.log("Un client s'est déconnecté");
  });
});