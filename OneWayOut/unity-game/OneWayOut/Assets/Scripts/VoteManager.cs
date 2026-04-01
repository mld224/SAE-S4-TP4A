using UnityEngine;
using TMPro;

public class VoteManager : MonoBehaviour
{
    public WebSocketClient ws;

    public TMP_Text timerText;
    public TMP_Text resultText;

    public float voteDuration = 10f;
    private float timer;
    private bool isVoting = true;

    void Start()
    {
        StartVote();
    }

    void Update()
    {
        if (isVoting)
        {
            timer -= Time.deltaTime;
            timerText.text = "Vote : " + Mathf.Ceil(timer);

            if (timer <= 0)
            {
                EndVote();
            }
        }
    }

    void StartVote()
    {
        timer = voteDuration;
        isVoting = true;
        resultText.text = "";
        Debug.Log("Nouveau vote lancé !");
    }

    void EndVote()
    {
        isVoting = false;

        int A = ws.voteA;
        int B = ws.voteB;
        int C = ws.voteC;

        string result = GetWinner(A, B, C);

        resultText.text = "Résultat : " + result;

        Debug.Log("Vote terminé -> " + result);

        // reset serveur
        ws.SendReset();

        Invoke(nameof(StartVote), 3f);
    }

    string GetWinner(int A, int B, int C)
    {
        if (A >= B && A >= C) return "A";
        if (B >= A && B >= C) return "B";
        return "C";
    }
}