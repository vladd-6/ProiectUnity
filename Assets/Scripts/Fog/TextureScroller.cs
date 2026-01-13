using UnityEngine;

public class TextureScroller : MonoBehaviour
{
    public Vector2 viteza = new Vector2(20.0f, 10.5f);
    
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        Vector2 offsetActual = Time.time * viteza;
        
        rend.material.mainTextureOffset = offsetActual;
    }
}