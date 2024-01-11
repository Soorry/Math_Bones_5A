using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class NewBehaviourScript : MonoBehaviour
{
    public GameObject pointPrefab;
    public GameObject parent;
    public List<Vector3> listPoints = new List<Vector3>();

    // Start is called before the first frame update
    void Start()
    {
        GeneratePoints();
    }

    public  void GeneratePoints()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 point = new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), Random.Range(-5f, 5f));
            listPoints.Add(point);
            InstantiatePoint(point);
        }
    }
    
    void InstantiatePoint(Vector3 position)
    {
        GameObject newP = Instantiate(pointPrefab, position, Quaternion.identity);
        newP.transform.parent = parent.transform;
    }

    void generateBones()
    {
        //Récupération du barycentre
        Vector3 bary = getBarycentre(listPoints);
        
        //Mise à l'origine
        List<Vector3> pointsToOrigin = new List<Vector3>();
        foreach (var p in listPoints)
        {
            pointsToOrigin.Add(p - bary);
        }
        
        //On créée la matrice de covalence
        Matrix4x4 matCov = Matrix4x4.zero;
    }

    Vector3 getBarycentre(List<Vector3> points)
    {
        Vector3 bary = new Vector3();
        foreach (var p in points)
        {
            bary += p;
        }
        bary /= points.Count;
        return bary;
    }

    float getCovariance(List<float> x, List<float> y)
    {
        float moyX = x.Sum() / x.Count;
        float moyY = y.Sum() / y.Count;
        float moyXY = 0;

        for (int i = 0; i < x.Count; i++)
        {
            moyXY += x[i] * y[i];
        }

        moyXY /= x.Count;

        return moyXY - moyX * moyY;
    }

    float getVariance(List<float> x)
    {
        float moyX = x.Sum() / x.Count;
        float moySqrX = 0;
        for (int i = 0; i < x.Count; i++)
        {
            moySqrX += x[i] * x[i];
        }
        moySqrX /= x.Count;

        return moySqrX - moyX * moyX;
    }
    
    Matrix4x4 getMatCovariance(List<Vector3> points)
    {
        List<float> x = new List<float>(), y = new List<float>(), z = new List<float>();
        foreach (var p in points)
        {
            x.Add(p.x);
            y.Add(p.y);
            z.Add(p.z);
        }
        Matrix4x4 mat = Matrix4x4.zero;
        mat[0, 0] = getVariance(x);
        mat[1, 1] = getVariance(y);
        mat[2, 2] = getVariance(z);

        mat[0, 1] = getCovariance(x, y);
        mat[1, 0] = mat[0, 1];
        mat[0, 2] = getCovariance(x, z);
        mat[2, 0] = mat[0, 2];
        mat[1, 2] = getCovariance(y, z);
        mat[2, 1] = mat[1, 2];
        
        return mat;
    }

    float getValeurPropre(Matrix4x4 mat)
    {
        float lambda = 0;
        float p = mat[0, 0] + mat[1, 1] + mat[2, 2];
        float q = mat[0, 0] * mat[1, 1] + mat[0, 0] * mat[2, 2] + mat[1, 1] * mat[2, 2] 
                  - mat[0, 1] * mat[1, 0] - mat[0, 2] * mat[2, 0] - mat[1, 2] * mat[2, 1];
        float r = mat[0, 0] * (mat[1, 2] * mat[2, 1] - mat[1, 1] * mat[2, 2])
                  + mat[0, 1] * (mat[2, 0] * mat[1, 2] - mat[1, 0] * mat[2, 2])
                  + mat[0, 2] * (mat[1, 0] * mat[2, 1] - mat[2, 0] * mat[1, 1]);

        return lambda;
    }
    
    
}
