using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Inspector에서 설정하기 위한 Pool 정보 클래스
[System.Serializable]
public class Pool
{
    public ZombieType type; // 이 풀을 식별할 태그
    public GameObject prefab; // 풀링할 프리팹
    public int size; // 풀의 초기 크기
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
        // 딕셔너리 초기화 (Key가 ZombieType)
        poolDictionary = new Dictionary<ZombieType, Queue<GameObject>>();
        poolContainers = new Dictionary<ZombieType, Transform>();

        foreach (Pool pool in pools)
        {
            // 컨테이너 이름에 enum 값을 문자열로 사용 (예: "Normal Pool")
            GameObject containerObject = new GameObject(pool.type.ToString() + " Pool");
            containerObject.transform.SetParent(this.transform);

            // 딕셔너리에 추가할 때 Key로 pool.type 사용
            poolContainers.Add(pool.type, containerObject.transform);

            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab, containerObject.transform);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            // 딕셔너리에 추가할 때 Key로 pool.type 사용
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
            // 동적 생성 시에도 enum type을 사용
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
