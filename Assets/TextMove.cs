using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class TextMove : MonoBehaviour
{
    private TMP_TextInfo textInfo;
    public TMP_Text textComponent;
    private Vector3[] originalVertices;
    private Mesh mesh;
    public float thresholdDistance = 10.0f;
    public float moveAmount = 5f;
    public float k = 10.0f;
    public float smoothness = 5.0f;
    void Start()
    {
        if(textComponent == null)
            textComponent = GetComponentInChildren<TMP_Text>();

        textComponent.ForceMeshUpdate(); // Ensure the mesh is generated
        textInfo = textComponent.textInfo;
        StoreOriginalPositions();
        
    }

    void StoreOriginalPositions()
    {
        mesh = textComponent.mesh;
        originalVertices = mesh.vertices.Clone() as Vector3[];
    }

    public void BeforeTextChanging()
    {
        mesh.vertices = originalVertices;
    }
    
    public void OnTextChanged()
    {
        textInfo = textComponent.textInfo;
        StoreOriginalPositions();
    }
    
    void Update()
    {
        mesh = textComponent.mesh;
        Vector3[] vertices = mesh.vertices;
        
        bool hasChanged = false;
        
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            Vector3 charCenter = Vector3.zero;
            Vector3 mousePosition = Input.mousePosition;
            
            charCenter = transform.TransformPoint((charInfo.bottomLeft + charInfo.topRight) / 2);
            mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));

            mousePosition.z = charCenter.z; // Ensure same z position
            
            float distance = Vector3.Distance(charCenter, mousePosition);
            // Calculate target offset
            Vector3 targetOffset = Vector3.zero;
            
            if (distance < thresholdDistance)
            {
                // Calculate direction away from mouse
                Vector3 direction = (charCenter - mousePosition).normalized;
                // Scale by proximity (closer = stronger effect)
                float proximityFactor = (thresholdDistance - distance)/thresholdDistance * k;
                targetOffset = direction * (moveAmount * proximityFactor);
                
            }
            
            // Smoothly interpolate to target offset
            Vector3 currentOffset = GetCurrentCharacterOffset(i);
            Vector3 newOffset = Vector3.Lerp(currentOffset, targetOffset, 
                Time.deltaTime * smoothness);
            
            // Apply the offset to all vertices of the character
            if (newOffset != currentOffset)
            {
                hasChanged = true;
                int vertexIndex = charInfo.vertexIndex;
                
                for (int j = 0; j < 4; j++) // Each character has 4 vertices
                {
                    vertices[vertexIndex + j] = originalVertices[vertexIndex + j] + newOffset;
                }
            }
        }
        
        if (hasChanged)
        {
            textComponent.ForceMeshUpdate(); 
            mesh.vertices = vertices;
            textComponent.UpdateGeometry(mesh, 0);
        }
    }
    
    // Helper method to get current offset of a character
    private Vector3 GetCurrentCharacterOffset(int charIndex)
    {
        TMP_CharacterInfo charInfo = textInfo.characterInfo[charIndex];
        if (!charInfo.isVisible) return Vector3.zero;
        
        int vertexIndex = charInfo.vertexIndex;
        if(vertexIndex < originalVertices.Length)
            return mesh.vertices[vertexIndex] - originalVertices[vertexIndex];
        else
            return Vector3.zero;
    }
}