using UnityEngine;

public class NextLevelLoader : MonoBehaviour
{
    private LevelLoadManager levelLoadManager;

    private void Awake()
    {
        levelLoadManager = FindAnyObjectByType<LevelLoadManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            levelLoadManager.LoadNextLevel();
        }
    }
}
