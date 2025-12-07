using UnityEngine;

public class CubeCollect : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.CollectCube();
            Destroy(gameObject);
        }
    }
}