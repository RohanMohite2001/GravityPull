using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int totalCubes;
    private int collected;

    private void Awake()
    {
        Instance = this;
        collected = 0;
        UIManager.Instance.UpdateCubeCount(collected, totalCubes);
    }

    public void CollectCube()
    {
        collected++;
        UIManager.Instance.UpdateCubeCount(collected, totalCubes);

        if (collected >= totalCubes)
            UIManager.Instance.GameOver();
    }
}