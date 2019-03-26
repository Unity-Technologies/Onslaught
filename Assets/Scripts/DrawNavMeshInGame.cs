using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.AI;

public class DrawNavMeshInGame : MonoBehaviour
{
    public Material material;
    NavMeshTriangulation triangulation;

    void Start() {
        StartCoroutine(CalculateTriangulationEveryPeriod(3.0f));
    }

    IEnumerator CalculateTriangulationEveryPeriod(float seconds)
    {
        triangulation = NavMesh.CalculateTriangulation();

        yield return new WaitForSeconds(seconds);

        StartCoroutine(CalculateTriangulationEveryPeriod(3.0f));
    }

    void OnPostRender() {
     if (material == null) {
         return;
     }
     GL.PushMatrix();
 
     material.SetPass(0);
     GL.Begin(GL.TRIANGLES);
     for (int i = 0; i < triangulation.indices.Length; i += 3) {
         var triangleIndex = i / 3;
         var i1 = triangulation.indices[i];
         var i2 = triangulation.indices[i + 1];
         var i3 = triangulation.indices[i + 2];
         var p1 = triangulation.vertices[i1];
         var p2 = triangulation.vertices[i2];
         var p3 = triangulation.vertices[i3];
         var areaIndex = triangulation.areas[triangleIndex];
         Color color;
         switch (areaIndex) {
             case 0: // walkable
                 color = Color.cyan; break;
             case 1: // nonWalkable
                 color = Color.red; break;
             default: // unknown
                 color = Color.black; break;
         }
         GL.Color(color);
         GL.Vertex(p1);
         GL.Vertex(p2);
         GL.Vertex(p3);
     }
     GL.End();
 
     GL.PopMatrix();
 }
}
