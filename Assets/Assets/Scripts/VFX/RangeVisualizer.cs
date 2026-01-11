using UnityEngine;

public class RangeVisualizer : MonoBehaviour
{
    [Header("Referanslar")]
    public Player player;
    public GameObject vfxObject;

    [Header("Ayarlar")]
    public float rotationSpeed = 150f; // Dairenin dönme hızı
    public Vector3 vfxOffset = new Vector3(0, 0.1f, 0); // Yere sıfır olmaması için

    private float currentRange = 2f;

    private void Start()
    {
        // Başlangıçta VFX'in hareketten etkilenmemesi için Particle ayarını yapalım
        ApplyLocalSimulation();
    }

    private void LateUpdate() // Hareketten sonra çalışması titremeyi önler
    {
        if (player == null || vfxObject == null) return;

        // Menzili silahtan al
        if (player.currentWeapon != null)
        {
            currentRange = player.currentWeapon.AttackRange;
        }

        UpdateVFXPosition();
    }

    private void UpdateVFXPosition()
    {
        // 1. Dönüş açısını zamanla hesapla
        float angle = Time.time * rotationSpeed;

        // 2. Karakterin hiyerarşideki Knight objesinin değil, ana Player objesinin pozisyonunu baz al
        // Böylece animasyonlardaki (eğilme, bükülme) titremelerden etkilenmez.
        Vector3 center = player.transform.position;

        // 3. Çember üzerindeki noktayı hesapla (Radyan cinsinden)
        float x = Mathf.Cos(angle * Mathf.Deg2Rad) * currentRange;
        float z = Mathf.Sin(angle * Mathf.Deg2Rad) * currentRange;

        // 4. VFX'in pozisyonunu DÜNYA uzayında ama KARAKTERE bağlı hesapla
        // Bu sayede karakterin yerel rotasyonu veya "leaning" animasyonları daireyi bozmaz.
        vfxObject.transform.position = center + new Vector3(x, 0, z) + vfxOffset;
        
        // VFX'in her zaman merkeze bakması için (opsiyonel)
        vfxObject.transform.LookAt(center + vfxOffset);
    }

    private void ApplyLocalSimulation()
    {
        // Eğer VFX bir Particle System ise, hareket ederken "iz" bırakmasını engellemek için
        // Simulation Space'i Local'e çekiyoruz.
        ParticleSystem ps = vfxObject.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
        }
    }
}