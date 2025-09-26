using System.Collections;
using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    public Transform[] spawnPoints; // Y가 다른 3개 스포너
    public float firstSpawnDelay = 1f;
    public float spawnInterval = 0.5f;

    public ZombieType spawnType = ZombieType.ZombieA; // Inspector에서 설정

    private int nextSpawnIndex = 0;

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        // 첫 스폰 대기
        yield return new WaitForSeconds(firstSpawnDelay);

        while (true)
        {
            nextSpawnIndex = (int)Random.Range(0f, spawnPoints.Length);
            Transform spawnPoint = spawnPoints[nextSpawnIndex];

            // PoolManager에서 좀비 가져오기
            GameObject zombie = PoolManager.Instance.SpawnFromPool(
                spawnType,
                spawnPoint.position,
                Quaternion.identity,
                nextSpawnIndex
            );
            nextSpawnIndex = (nextSpawnIndex + 1) % spawnPoints.Length;
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
