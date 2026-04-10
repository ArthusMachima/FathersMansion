
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class MethodDebugger : MonoBehaviour
{
    [System.Serializable]
    public class MethodEvent
    {
        public KeyCode triggerKey;
        public UnityEvent methodEvent;
    }

    public List<MethodEvent> methodEvents = new List<MethodEvent>();

    private void Update()
    {
        foreach (var methodEvent in methodEvents)
        {
            if (Input.GetKeyDown(methodEvent.triggerKey))
            {
                methodEvent.methodEvent?.Invoke();
            }
        }
    }

    public void CallMethod(int index)
    {
        if (index >= 0 && index < methodEvents.Count)
        {
            methodEvents[index].methodEvent?.Invoke();
        }
        else
        {
            Debug.LogError($"Index {index} is out of range!");
        }
    }

    public void Test()
    {
        Debug.Log("tested");
    }
}
