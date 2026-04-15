using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    /* Reference vers le vehicule pour suivre sa position */
    public Transform player;

    /* Taille du fond en Y (hauteur de l'image en unites Unity)
       A ajuster selon la taille de votre image */
    public float hauteurFond = 20f;

    /* Le Renderer du fond pour acceder au material */
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        /* Le fond suit le joueur horizontalement et verticalement */
        transform.position = new Vector3(
            player.position.x,
            player.position.y,
            10f  /* Z = 10 pour etre derriere tout le reste */
        );

        /* Decale la texture en fonction de la position du joueur
           Ca donne l'illusion que le fond defile a l'infini
           sans jamais s'arreter */
        float offsetY = player.position.y / hauteurFond;
        rend.material.mainTextureOffset = new Vector2(0, offsetY);
    }
}