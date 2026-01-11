using UnityEngine;

public class TextureScroller : MonoBehaviour
{
    // Viteza cu care curge podeaua (X = lateral, Y = inainte/inapoi)
    public Vector2 viteza = new Vector2(20.0f, 10.5f);
    
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        // Calculam noua pozitie a texturii bazata pe timp
        // Folosim material.mainTextureOffset pentru a muta doar "pielea" obiectului
        Vector2 offsetActual = Time.time * viteza;
        
        rend.material.mainTextureOffset = offsetActual;
        
        // Daca folosesti URP si nu merge linia de sus, incearca asta:
        // rend.material.SetTextureOffset("_BaseMap", offsetActual);
    }
}