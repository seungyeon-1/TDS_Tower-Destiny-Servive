using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Inspector���� �����ϱ� ���� Pool ���� Ŭ����
[System.Serializable]
public class Pool
{
    public ZombieType type; // �� Ǯ�� �ĺ��� �±�
    public GameObject prefab; // Ǯ���� ������
    public int size; // Ǯ�� �ʱ� ũ��
}

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;

    public List<Pool> pools;
    public int index = 0;
    private Dictionary<ZombieType, Queue<GameObject>> poolDictionary;
    private Dictionary<ZombieType, Transform> poolContainers;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // ��ųʸ� �ʱ�ȭ (Key�� ZombieType)
        poolDictionary = new Dictionary<ZombieType, Queue<GameObject>>();
        poolContainers = new Dictionary<ZombieType, Transform>();

        foreach (Pool pool in pools)
        {
            // �����̳� �̸��� enum ���� ���ڿ��� ��� (��: "Normal Pool")
            GameObject containerObject = new GameObject(pool.type.ToString() + " Pool");
            containerObject.transform.SetParent(this.transform);

            // ��ųʸ��� �߰��� �� Key�� pool.type ���
            poolContainers.Add(pool.type, containerObject.transform);

            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab, containerObject.transform);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            // ��ųʸ��� �߰��� �� Key�� pool.type ���
            poolDictionary.Add(pool.type, objectPool);
        }

    }

    public GameObject SpawnFromPool(ZombieType type, Vector3 position, Quaternion rotation,int spawnIndex)
    {
        if (!poolDictionary.ContainsKey(type))
        {
            Debug.LogWarning("Pool with type " + type + " doesn't exist.");
            return null;
        }

        Queue<GameObject> poolQueue = poolDictionary[type];
        GameObject objectToSpawn;

        if (poolQueue.Count > 0)
        {
            objectToSpawn = poolQueue.Dequeue();
        }
        else
        {
            // ���� ���� �ÿ��� enum type�� ���
            Transform parentContainer = poolContainers[type];
            objectToSpawn = Instantiate(pools.Find(p => p.type == type).prefab, parentContainer);
        }

        objectToSpawn.layer = spawnIndex + 9;
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        return objectToSpawn;
    }

    public void ReturnToPool(ZombieType type, GameObject objectToReturn)
    {
        if (!poolDictionary.ContainsKey(type))
        {
            Debug.LogWarning("Pool with type " + type + " doesn't exist.");
            Destroy(objectToReturn);
            return;
        }

        objectToReturn.SetActive(false);

        Transform parentContainer = poolContainers[type];
        objectToReturn.transform.SetParent(parentContainer);

        poolDictionary[type].Enqueue(objectToReturn);
    }
}
