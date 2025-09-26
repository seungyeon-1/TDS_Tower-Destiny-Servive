using System.Collections;
using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    public Transform[] spawnPoints; // Y�� �ٸ� 3�� ������
    public float firstSpawnDelay = 1f;
    public float spawnInterval = 0.5f;

    public ZombieType spawnType = ZombieType.ZombieA; // Inspector���� ����

    private int nextSpawnIndex = 0;

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        // ù ���� ���
        yield return new WaitForSeconds(firstSpawnDelay);

        while (true)
        {
            nextSpawnIndex = (int)Random.Range(0f, spawnPoints.Length);
            Transform spawnPoint = spawnPoints[nextSpawnIndex];

            // PoolManager���� ���� ��������
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
