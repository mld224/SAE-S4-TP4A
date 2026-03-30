// On importe la librairie WebSocket "ws" qu'on a installe avec npm
const WebSocket = require('ws');

// On cree un serveur WebSocket qui ecoute sur le port 8080
const server = new WebSocket.Server({ port: 8080 });

// Objet qui stocke les votes en cours (compteur pour chaque choix)
let votes = { A: 0, B: 0, C: 0 };

// Boolean qui indique si un vote est en cours ou non
let voteOuvert = false;

// Set qui contient tous les clients (telephones) connectes (tableau)
const clients = new Set();

// Evenement "connection" : se declenche a chaque fois qu'un telephone se connecte
server.on('connection', (ws) => {
    // "ws" represente la connexion avec CE telephone precis

    // On ajoute ce telephone a notre liste de clients
    clients.add(ws);
    console.log(`Joueur connecte ! Total: ${clients.size}`);// affiche le nombre de clients total

    // Evenement "message" : se declenche quand CE telephone envoie un message
    ws.on('message', (message) => {
        // On convertit le message en texte (il arrive en binaire)
        const data = message.toString();

        // On verifie que :
        // 1. Un vote est bien en cours (voteOuvert)
        // 2. Le message est un vote valide (A, B ou C)
        if (voteOuvert && ['A', 'B', 'C'].includes(data)) {
            // On incremente le compteur du choix correspondant
            votes[data]++;
            console.log(`Vote recu: ${data} | A:${votes.A} B:${votes.B} C:${votes.C}`);// on verifie que les compteurs sont a jour
        }
    });

    // Evenement "close" : se declenche quand CE telephone se deconnecte
    ws.on('close', () => {
        clients.delete(ws);
        console.log(`Joueur deconnecte. Total: ${clients.size}`);
    });
});

// Fonction utilitaire : envoie un message a TOUS les clients connectes
// C'est comme ca que le serveur "parle" aux telephones
function broadcast(message) {
    clients.forEach((client) => {
        // On verifie que le client est toujours connecte avant d'envoyer
        if (client.readyState === WebSocket.OPEN) {
            client.send(message);
        }
    });
}

// Fonction qui lance un nouveau vote
// "duree" = nombre de secondes pour voter (10 par defaut)
function lancerVote(duree = 10) {
    // On remet les compteurs a zero
    votes = { A: 0, B: 0, C: 0 };

    // On ouvre le vote
    voteOuvert = true;

    // On previent tous les telephones que le vote commence
    // JSON.stringify transforme un objet JS en texte envoyable
    broadcast(JSON.stringify({ type: 'vote_start', duree: duree }));
    console.log(`Vote lance pour ${duree} secondes...`);

    // setTimeout execute du code APRES un delai
    // Ici : apres "duree" secondes, on ferme le vote
    setTimeout(() => {
        voteOuvert = false;

        // On trie les votes pour trouver le choix avec le plus de votes
        // Object.entries transforme {A:3, B:5, C:1} en [["A",3], ["B",5], ["C",1]]
        // .sort trie du plus grand au plus petit
        // [0][0] prend le premier element (le gagnant) et son nom (A, B ou C)
        // et sa trier si b - a > 0 alors b passe avant a ( a la fin on a le choix avec le plus de vote)
        const resultat = Object.entries(votes).sort((a, b) => b[1] - a[1])[0][0];// renvoie le premier element du tab trier 

        // On prepare le message avec le resultat et le detail des votes
        const message = {
            type: 'vote_result',
            resultat: resultat,
            details: votes
        };

        // On envoie le resultat a tous les telephones
        broadcast(JSON.stringify(message));
        console.log(`Vote termine ! Resultat: ${resultat}`, votes);
    }, duree * 1000); // duree est en secondes, setTimeout attend des millisecondes
}

console.log('Serveur WebSocket demarre sur le port 8080');

// POUR TESTER : on lance un vote automatiquement toutes les 15 secondes
// (a supprimer plus tard, c'est Unity qui declenchera les votes)
setInterval(() => {
    lancerVote(10);
}, 15000); // 15000ms = 15 secondes
