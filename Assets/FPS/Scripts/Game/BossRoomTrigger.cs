using Unity.FPS.Game;
using UnityEngine;

public class BossRoomTrigger : MonoBehaviour
{
    public UnityEngine.Events.UnityEvent OnPlayerEnter;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnPlayerEnter.Invoke();
        }
    }
}
