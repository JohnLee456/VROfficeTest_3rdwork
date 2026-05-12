using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TableSeatingInitializer : MonoBehaviour
{
    private static readonly string[] CharacterNames =
    {
        "ZJR",
        "ZHZ",
        "DCY",
        "GCHbot"
    };

    [SerializeField] private Vector3 fallbackTableCenter = new Vector3(2.55f, 0f, 2.37f);
    [SerializeField] private Vector2 tableSideOffset = new Vector2(1.65f, 1.8f);
    [SerializeField] private float modelPitchOffset = -90f;
    [SerializeField] private float modelYawOffset;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateForScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.name != "OfficeLoggedIn" || FindObjectOfType<TableSeatingInitializer>() != null)
        {
            return;
        }

        GameObject manager = new GameObject("Table Seating Initializer");
        manager.AddComponent<TableSeatingInitializer>();
    }

    private IEnumerator Start()
    {
        yield return null;
        ArrangeCharactersAroundTable();
    }

    private void ArrangeCharactersAroundTable()
    {
        Vector3 tableCenter = FindTableCenter();
        Vector3[] positions =
        {
            new Vector3(tableCenter.x, 0f, tableCenter.z + tableSideOffset.y),
            new Vector3(tableCenter.x + tableSideOffset.x, 0f, tableCenter.z),
            new Vector3(tableCenter.x, 0f, tableCenter.z - tableSideOffset.y),
            new Vector3(tableCenter.x - tableSideOffset.x, 0f, tableCenter.z)
        };

        for (int i = 0; i < CharacterNames.Length; i++)
        {
            GameObject character = GameObject.Find(CharacterNames[i]);
            if (character == null)
            {
                Debug.LogWarning($"Table seating skipped {CharacterNames[i]}: character was not found.", this);
                continue;
            }

            Transform characterTransform = character.transform;
            Vector3 position = positions[i];
            position.y = characterTransform.position.y;
            characterTransform.position = position;
            FacePoint(characterTransform, tableCenter);
        }
    }

    private Vector3 FindTableCenter()
    {
        GameObject table = GameObject.Find("Table");
        if (table != null)
        {
            Vector3 center = table.transform.position;
            center.y = 0f;
            return center;
        }

        return fallbackTableCenter;
    }

    private void FacePoint(Transform target, Vector3 point)
    {
        Vector3 direction = point - target.position;
        direction.y = 0f;
        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        float yaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        target.rotation = Quaternion.Euler(modelPitchOffset, yaw + modelYawOffset, 0f);
    }
}
