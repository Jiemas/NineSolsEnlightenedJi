using UnityEngine;

namespace EnlightenedJi;

public class DestroyOnDisable : MonoBehaviour
{
    void OnDisable()
    {
        // When disabled, destroy the entire GameObject
        Destroy(gameObject);
    }
}
