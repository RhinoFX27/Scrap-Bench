using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EffectTextQueue : MonoBehaviour
{
    [SerializeField] private float messageDuration = 1.5f;

    private static EffectTextQueue instance;
    private readonly Queue<string> messages = new Queue<string>();
    private TMP_Text effectText;
    private Coroutine displayCoroutine;

    public static void Enqueue(string message)
    {
        EffectTextQueue queue = GetOrCreate();
        if (queue == null)
            return;

        queue.messages.Enqueue(message);
        if (queue.displayCoroutine == null)
            queue.displayCoroutine = queue.StartCoroutine(queue.DisplayMessages());
    }

    private static EffectTextQueue GetOrCreate()
    {
        if (instance != null)
            return instance;

        GameObject effectTextObject = GameObject.FindWithTag("EffectText");
        if (effectTextObject == null)
        {
            Debug.LogError("Could not find a GameObject tagged EffectText.");
            return null;
        }

        instance = effectTextObject.GetComponent<EffectTextQueue>();
        if (instance == null)
            instance = effectTextObject.AddComponent<EffectTextQueue>();

        return instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }

        instance = this;
        effectText = GetComponent<TMP_Text>();
        if (effectText == null)
        {
            Debug.LogError("EffectTextQueue requires a TMP_Text component on the same GameObject.");
            enabled = false;
        }
    }

    private IEnumerator DisplayMessages()
    {
        while (messages.Count > 0)
        {
            effectText.text = messages.Dequeue();
            yield return new WaitForSeconds(messageDuration);
        }

        displayCoroutine = null;
    }
}
