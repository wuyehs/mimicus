using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponSpawner : MonoBehaviour
{
    [Header("生成设置")]
    public float minSpawnDelay = 1f;   // 最小等待时间（秒）
    public float maxSpawnDelay = 5f;  // 最大等待时间（秒）
    public bool spawnOnlyWhenEmpty = true; // 仅当场上无道具时生成

    [Header("道具预制体")]
    public GameObject[] weaponPrefabs;  // 将烟雾弹、手枪、炸弹的预制体拖入此处
    // 注意：请确保每个预制体的 Tag 都设置为 "Pickup"，以便生成器检测

    private bool isSpawning = false;

    void Start()
    {
        // 启动生成循环
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // 等待随机时间
            float delay = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(delay);

            // 检查场上是否已有道具（通过 Tag 为 "Pickup" 的对象数量判断）
            GameObject[] existingPickups = GameObject.FindGameObjectsWithTag("Pickup");
            if (spawnOnlyWhenEmpty && existingPickups.Length > 0)
            {
                // 场上还有道具，跳过本次生成，进入下一轮等待
                continue;
            }

            // 生成道具
            SpawnWeapon();
        }
    }

    void SpawnWeapon()
    {
        if (weaponPrefabs == null || weaponPrefabs.Length == 0)
        {
            Debug.LogError("WeaponSpawner: 没有指定任何武器预制体！");
            return;
        }

        // 随机选择一个武器类型
        int index = Random.Range(0, weaponPrefabs.Length);
        GameObject weaponPrefab = weaponPrefabs[index];

        // 计算随机生成位置（在相机视野内）
        Vector2 spawnPosition = GetRandomPositionWithinCamera();

        // 实例化道具
        GameObject weapon = Instantiate(weaponPrefab, spawnPosition, Quaternion.identity);

        // 确保道具的标签为 "Pickup"（如果预制体本身没有设置，则强制设置）
        if (!weapon.CompareTag("Pickup"))
        {
            weapon.tag = "Pickup";
        }

        // 可选：打印生成信息
        Debug.Log($"生成道具: {weaponPrefab.name} 在位置 {spawnPosition}");
    }

    Vector2 GetRandomPositionWithinCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("WeaponSpawner: 场景中没有 MainCamera！");
            return Vector2.zero;
        }

        // 获取相机视口的左下角和右上角世界坐标
        Vector2 min = cam.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector2 max = cam.ViewportToWorldPoint(new Vector3(1, 1, 0));

        // 为了避免生成在屏幕边缘，可以缩小一点范围（可选）
        float paddingX = 0.5f;
        float paddingY = 0.5f;
        float x = Random.Range(min.x + paddingX, max.x - paddingX);
        float y = Random.Range(min.y + paddingY, max.y - paddingY);

        return new Vector2(x, y);
    }
}