using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccesVertices : MonoBehaviour
{
    void Start()
    {
        // Assurez-vous d'avoir un MeshFilter attaché à votre GameObject
        MeshFilter meshFilter = GetComponent<MeshFilter>();

        if (meshFilter != null)
        {
            // Obtenez le mesh du MeshFilter
            Mesh mesh = meshFilter.mesh;

            // Obtenez les vertices du mesh
            Vector3[] vertices = mesh.vertices;

            // Parcourez les vertices et faites quelque chose avec eux
            foreach (Vector3 vertex in vertices)
            {
                Debug.Log("Vertex : " + vertex);
            }
        }
        else
        {
            Debug.LogError("Aucun MeshFilter attaché au GameObject");
        }
    }
}