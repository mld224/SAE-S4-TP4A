using System;
using UnityEngine;
using NativeWebSocket;
using TMPro;

/* Classes pour deserialiser les messages JSON du serveur
   JsonUtility.FromJson a besoin de classes qui correspondent
   a la structure du JSON recu */

/* Pour le message vote_start : {"type":"vote_start","duree":10} */
[Serializable]
public class VoteStartMessage
{
    public string type;
    public int duree;
}

/* Pour le detail des votes dans vote_result */
[Serializable]
public class VotesData
{
    public int A;
    public int B;
    public int C;
}

/* Pour le message vote_result : {"type":"vote_result","resultat":"A","details":{...}} */
[Serializable]
public class VoteResultMessage
{
    public string type;
    public string resultat;
    public VotesData details;
}

public class WebSocketClient : MonoBehaviour
{
    /* Connexion WebSocket vers le serveur Node.js */
    private WebSocket websocket;

    /* Reference vers le VoteManager pour lui transmettre les resultats
       On la relie dans l'inspecteur Unity (glisser-deposer) */
    public VoteManager voteManager;

    /* Textes UI pour afficher l'etat de la connexion */
    public TMP_Text connectionStatus;

    async void Start()
    {
        /* IMPORTANT : remplace par l'URL ngrok ou l'IP locale
           En local : ws://localhost:8080
           Avec ngrok : wss://ton-url.ngrok-free.app */
        websocket = new WebSocket("wss://magali-lowery-smelly.ngrok-free.dev/");

        /* Quand la connexion s'ouvre */
        websocket.OnOpen += () =>
        {
            Debug.Log("Connecte au serveur !");
            if (connectionStatus != null)
                connectionStatus.text = "Connecte";
        };

        websocket.OnError += (e) =>
        {
            Debug.LogError("Erreur WebSocket : " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connexion fermee !");
            if (connectionStatus != null)
                connectionStatus.text = "Deconnecte";
        };

        /* Quand on recoit un message du serveur */
        websocket.OnMessage += (bytes) =>
        {
            /* Convertit les bytes en texte lisible */
            string message = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log("Message recu : " + message);

            try
            {
                /* On essaie d'abord de lire le type du message
                   pour savoir comment le traiter */
                VoteStartMessage baseMsg = JsonUtility.FromJson<VoteStartMessage>(message);

                if (baseMsg.type == "vote_start")
                {
                    /* Le serveur confirme que le vote a demarre
                       On pourrait utiliser baseMsg.duree si besoin */
                    Debug.Log("Vote demarre pour " + baseMsg.duree + " secondes");
                }
                else if (baseMsg.type == "vote_result")
                {
                    /* Le vote est termine, on recoit le resultat
                       On parse le message complet avec les details */
                    VoteResultMessage result = JsonUtility.FromJson<VoteResultMessage>(message);
                    Debug.Log("Resultat du vote : " + result.resultat);

                    /* On transmet le resultat au VoteManager
                       qui va bouger le vehicule en consequence */
                    if (voteManager != null)
                    {
                        voteManager.OnVoteResult(result.resultat, result.details);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Erreur parsing JSON : " + ex.Message);
            }
        };

        await websocket.Connect();
    }

    /* DispatchMessageQueue doit etre appele chaque frame
       pour traiter les messages WebSocket dans le thread principal
       Unity ne permet pas de modifier l'UI depuis un autre thread */
    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();
#endif
    }

    /* Fonction appelee par VoteManager quand le vehicule
       arrive a un embranchement. Envoie START_VOTE au serveur
       duree = nombre de secondes pour voter */
    public async void DemanderVote(int duree = 10)
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.SendText("START_VOTE:" + duree);
            Debug.Log("Demande de vote envoyee au serveur (duree: " + duree + "s)");
        }
    }

    async void OnApplicationQuit()
    {
        if (websocket != null)
        {
            await websocket.Close();
        }
    }
}