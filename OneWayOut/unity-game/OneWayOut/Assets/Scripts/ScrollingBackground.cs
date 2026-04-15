using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    public Transform player;
    public float hauteurFond = 20f;
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        /* Le background reste centre en X (toujours visible)
           et suit le joueur en Y pour l'effet de defilement */
        transform.position = new Vector3(
            0f,                  /* X fixe au centre */
            player.position.y,   /* Y suit le vaisseau */
            10f                   /* Z derriere tout */
        );

        /* Defilement de la texture pour effet infini */
        float offsetY = player.position.y / hauteurFond;
        rend.material.mainTextureOffset = new Vector2(0, offsetY);
    }
}