using UnityEngine;

public class Bullet : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        GameManager.Instance.HandleBulletHit(collision.gameObject, gameObject);
    }
}
