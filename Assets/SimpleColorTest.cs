using UnityEngine;

public class SimpleColorTest : MonoBehaviour
{
    private SpriteRenderer sr;
    
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogError("没找到SpriteRenderer");
            enabled = false;
        }
    }
    
    void Update()
    {
        if (sr != null)
        {
            sr.color = Color.red;
        }
    }
}