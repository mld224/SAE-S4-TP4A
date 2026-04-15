using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using NativeWebSocket;
using TMPro;

[Serializable]
public class VoteStartMessage
{
    public string type;
    public int duree;
}

[Serializable]
public class VotesData
{
    public int A;
    public int B;
    public int C;
}

[Serializable]
public class VoteResultMessage
{
    public string type;
    public string resultat;
    public VotesData details;
}

/* Nouveau message pour le compteur de joueurs */
[Serializable]
public class PlayerCountMessage
{
    public string type;
    public int count;
}

public class WebSocketClient : MonoBehaviour
{
    private WebSocket websocket;

    public VoteManager voteManager;
    public TMP_Text connectionStatus;

    /* Nouvelle reference vers LobbyManager pour mettre a jour le compteur */
    public LobbyManager lobbyManager;

    async void Start()
    {
        websocket = new WebSocket("wss://magali-lowery-smelly.ngrok-free.dev/");

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

        websocket.OnMessage += (bytes) =>
        {
            string message = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log("Message recu : " + message);

            try
            {
                VoteStartMessage baseMsg = JsonUtility.FromJson<VoteStartMessage>(message);

                if (baseMsg.type == "vote_start")
                {
                    Debug.Log("Vote demarre pour " + baseMsg.duree + " secondes");
                }
                else if (baseMsg.type == "vote_result")
                {
                    VoteResultMessage result = JsonUtility.FromJson<VoteResultMessage>(message);
                    Debug.Log("Resultat du vote : " + result.resultat);

                    if (voteManager != null)
                        voteManager.OnVoteResult(result.resultat, result.details);
                }
                else if (baseMsg.type == "restart")
                {
                    Debug.Log("Restart recu du serveur, rechargement de la scene");
                    Time.timeScale = 1;
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
                /* Nouveau : le serveur nous envoie le nombre de joueurs connectes */
                else if (baseMsg.type == "player_count")
                {
                    PlayerCountMessage msg = JsonUtility.FromJson<PlayerCountMessage>(message);
                    if (lobbyManager != null)
                        lobbyManager.UpdatePlayerCount(msg.count);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Erreur parsing JSON : " + ex.Message);
            }
        };

        await websocket.Connect();
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();
#endif
    }

    public async void DemanderVote(int duree = 10)
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.SendText("START_VOTE:" + duree);
            Debug.Log("Demande de vote envoyee au serveur (duree: " + duree + "s)");
        }
    }

    public async void SendGameOver()
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.SendText("GAME_OVER");
            Debug.Log("Game Over envoye au serveur");
        }
    }

    /* Nouveau : previent le serveur que le presenteur a lance la partie */
    public async void SendGameStart()
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.SendText("GAME_START");
            Debug.Log("Game Start envoye au serveur");
        }
    }

    async void OnApplicationQuit()
    {
        if (websocket != null)
            await websocket.Close();
    }
}