using System;
using UnityEngine;
using NativeWebSocket;
using TMPro;

[Serializable]
public class VotesData
{
    public int A;
    public int B;
    public int C;
}

[Serializable]
public class VotesMessage
{
    public string type;
    public VotesData votes;
}

public class WebSocketClient : MonoBehaviour
{
    private WebSocket websocket;

    public int voteA;
    public int voteB;
    public int voteC;

    public TMP_Text textVoteA;
    public TMP_Text textVoteB;
    public TMP_Text textVoteC;

    async void Start()
    {
        websocket = new WebSocket("ws://192.168.1.188:8080");

        websocket.OnOpen += () =>
        {
            Debug.Log("Connexion au serveur !");
        };

        websocket.OnError += (e) =>
        {
            Debug.LogError("Erreur WebSocket : " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connexion fermée !");
        };

        websocket.OnMessage += (bytes) =>
        {
            string message = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log("JSON reçu : " + message);

            try
            {
                VotesMessage data = JsonUtility.FromJson<VotesMessage>(message);

                if (data != null && data.type == "votes" && data.votes != null)
                {
                    voteA = data.votes.A;
                    voteB = data.votes.B;
                    voteC = data.votes.C;

                    Debug.Log($"Votes mis à jour -> A: {voteA} | B: {voteB} | C: {voteC}");

                    UpdateVoteUI();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Erreur parsing JSON : " + ex.Message);
            }
        };

        await websocket.Connect();
        UpdateVoteUI();
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();
#endif
    }

    void UpdateVoteUI()
    {
        if (textVoteA != null)
            textVoteA.text = "A : " + voteA;

        if (textVoteB != null)
            textVoteB.text = "B : " + voteB;

        if (textVoteC != null)
            textVoteC.text = "C : " + voteC;
    }

    async void OnApplicationQuit()
    {
        if (websocket != null)
        {
            await websocket.Close();
        }
    }

    public async void SendReset()
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.SendText("RESET");
        }
    }
}