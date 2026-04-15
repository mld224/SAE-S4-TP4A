using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    /* Flag global : le jeu a-t-il commence ? */
    public static bool GameStarted = false;

    /* UI du lobby */
    public GameObject lobbyPanel;
    public TMP_Text playerCountText;
    public Button startButton;

    /* References vers les autres managers */
    public LevelGenerator levelGenerator;
    public WebSocketClient ws;

    /* ===== SONS =====
       musiqueSource : AudioSource dedie a la musique (Loop = true)
       effetsSource : AudioSource pour les effets ponctuels (Loop = false)
       musiqueFond : glisse SpaceshipMusic.mp3 (musique en boucle pendant tout le jeu) */
    public AudioSource musiqueSource;
    public AudioSource effetsSource;
    public AudioClip musiqueFond;

    void Start()
    {
        GameStarted = false;

        if (lobbyPanel != null)
            lobbyPanel.SetActive(true);

        if (playerCountText != null)
            playerCountText.text = "0 joueur connecte";

        if (startButton != null)
            startButton.onClick.AddListener(StartGame);

        /* Lance la musique de fond en boucle des le lobby
           Elle continuera pendant toute la partie */
        if (musiqueSource != null && musiqueFond != null)
        {
            musiqueSource.clip = musiqueFond;
            musiqueSource.loop = true;
            musiqueSource.volume = 0.3f;  /* volume bas pour ne pas couvrir les effets */
            musiqueSource.Play();
        }
    }

    /* ===== MISE A JOUR DU COMPTEUR DE JOUEURS ===== */
    public void UpdatePlayerCount(int count)
    {
        if (playerCountText != null)
        {
            string suffixe = (count <= 1) ? " joueur connecte" : " joueurs connectes";
            playerCountText.text = count + suffixe;
        }
    }

    /* ===== LANCEMENT DE LA PARTIE ===== */
    public void StartGame()
    {
        GameStarted = true;

        if (lobbyPanel != null)
            lobbyPanel.SetActive(false);

        if (levelGenerator != null)
            levelGenerator.StartGeneration();

        if (ws != null)
            ws.SendGameStart();

        /* La musique continue de tourner en boucle pendant le jeu */
    }
}