using NineSolsAPI;
using UnityEngine;

namespace EnlightenedJi;

public class DestroyOnDisable : MonoBehaviour
{
    void Awake() 
    {
        ToastManager.Toast($"{this.gameObject.name} created!");   
        // Destroy(gameObject); 
    }
    // void OnDisable()
    // {
    //     ToastManager.Toast($"{this.gameObject.name}: Circle disabled!");    

    //     // When disabled, destroy the entire GameObject
    //     // Destroy(gameObject);
    // }
}
