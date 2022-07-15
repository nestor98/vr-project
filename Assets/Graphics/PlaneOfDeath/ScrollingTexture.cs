using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingTexture : MonoBehaviour
{
    public int materialIndex = 0;
    public Vector2 uvAnimationRate = new Vector2(1.0f, 1.0f);
    public string textureName = "_NoiseTex";

    private Vector2 uvOffset = Vector2.zero;

    void LateUpdate() 
    {
        uvOffset += uvAnimationRate * Time.deltaTime;
        if (GetComponent<Renderer>().enabled )
        {
            GetComponent<Renderer>().materials[materialIndex].SetTextureOffset(textureName, uvOffset);
        }
    }
}
