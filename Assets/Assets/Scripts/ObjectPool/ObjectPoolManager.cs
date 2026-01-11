using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }

    // Dictionary (Sözlük) kullanarak farklı prefablar için ayrı havuzlar tutuyoruz.
    // List kullanımı, pasif objeyi kesin olarak bulmamızı sağlar.
    private Dictionary<int, List<GameObject>> poolDictionary = new Dictionary<int, List<GameObject>>();

    private void Awake()
    {
        // Singleton Kurulumu
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Havuzdan obje ister. Varsa pasif (kapalı) olanı verir, yoksa yeni yaratır.
    /// </summary>
    public GameObject GetPooledObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        int poolKey = prefab.GetInstanceID();

        // 1. Havuz yoksa oluştur.
        if (!poolDictionary.ContainsKey(poolKey))
        {
            poolDictionary.Add(poolKey, new List<GameObject>());
        }

        // 2. Listeyi tara ve İLK PASİF (kapalı) objeyi bul.
        GameObject objectToSpawn = null;

        foreach (GameObject obj in poolDictionary[poolKey])
        {
            // Obje null değilse (sahneden silinmemişse) VE Hiyerarşide aktif değilse (kapalıysa)
            if (obj != null && !obj.activeInHierarchy)
            {
                objectToSpawn = obj;
                break; // Boşta bir tane bulduk!
            }
        }

        // 3. Pasif obje bulamazsak, YENİ YARAT.
        if (objectToSpawn == null)
        {
            objectToSpawn = Instantiate(prefab);
            // Yeni yaratılanı havuza ekle.
            poolDictionary[poolKey].Add(objectToSpawn);
        }

        // 4. Objeyi ayarla ve aktif et (Spawn için hazırla)
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;
        objectToSpawn.SetActive(true);

        return objectToSpawn;
    }
}