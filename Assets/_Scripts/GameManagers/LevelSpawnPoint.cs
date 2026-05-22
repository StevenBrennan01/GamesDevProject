using UnityEngine;

public class LevelSpawnPoint : MonoBehaviour
{
    public void SpawnPlayerAtPoint(GameObject player)
    {
        player.transform.position = this.transform.position;
        player.transform.rotation = this.transform.rotation;
    }
}