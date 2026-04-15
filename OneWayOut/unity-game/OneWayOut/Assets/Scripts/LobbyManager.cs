using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    /* Flag global accessible par les autres scripts
       pour savoir si le jeu a commence ou si on est encore en lobby */
    public static bool GameStarted = false;

    /* Panel du lobby (cache apres le lancement) */
    public GameObject lobbyPanel;

    /* Texte du nombre de joueurs connectes */
    public TMP_Text playerCountText;

    /* Bouton pour lancer la partie (cliquable par le presenteur) */
    public Button startButton;

    /* References vers les managers du jeu a demarrer */
    public LevelGenerator levelGenerator;
    public WebSocketClient ws;

    void Start()
    {
        /* Remise a zero au demarrage ou apres un Recommencer */
        GameStarted = false;

        if (lobbyPanel != null)
            lobbyPanel.SetActive(true);

        if (playerCountText != null)
            playerCountText.text = "0 joueur connecte";

        /* On branche le bouton sur StartGame() */
        if (startButton != null)
            startButton.onClick.AddListener(StartGame);
    }

    /* Appele par WebSocketClient quand le serveur envoie player_count */
    public void UpdatePlayerCount(int count)
    {
        if (playerCountText != null)
        {
            string suffixe = (count <= 1) ? " joueur connecte" : " joueurs connectes";
            playerCountText.text = count + suffixe;
        }
    }

    /* Appele par le bouton "Commencer la partie" */
    public void StartGame()
    {
        GameStarted = true;

        if (lobbyPanel != null)
            lobbyPanel.SetActive(false);

        /* Lance la generation des waypoints et embranchements */
        if (levelGenerator != null)
            levelGenerator.StartGeneration();

        /* Previent les tels que le jeu a commence */
        if (ws != null)
            ws.SendGameStart();
    }
}