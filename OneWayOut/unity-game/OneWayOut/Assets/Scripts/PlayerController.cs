using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;

    private Vector3 direction = Vector3.up;

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    public void MoveLeft()
    {
        direction = new Vector3(-1, 1, 0).normalized;
        Debug.Log("Direction : Gauche");
    }

    public void MoveRight()
    {
        direction = new Vector3(1, 1, 0).normalized;
        Debug.Log("Direction : Droite");
    }

    public void MoveForward()
    {
        direction = Vector3.up;
        Debug.Log("Direction : Tout droit");
    }
}