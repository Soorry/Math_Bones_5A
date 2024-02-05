using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class BonesGenerator : MonoBehaviour
{
    public GameObject pointPrefab;
    public GameObject parent;

    public Material boneMat;
    public Material linkMat;

    //public List<Vector3> listPoints = new List<Vector3>();

    // Start is called before the first frame update
    void Start()
    {/*
        Matrix4x4 m = new Matrix4x4();
        m[0,0] = -1;
        m[0,1] = -1;
        m[0,2] = -1;

        m[1,0] = -1;
        m[1,1] = 6;
        m[1,2] = -1;

        m[2,0] = -1;
        m[2,1] = -1;
        m[2,2] = -1;

        Debug.Log(methodePuissance(m));*/
        var points = getPoints(transform.gameObject);
        var minmax = generateBones(points);
        boneStruct(transform.gameObject, minmax.Item1, minmax.Item2);
    }

    public void boneStruct(GameObject curGO, Vector3 max, Vector3 min)
    {
        var allPoints = getPoints(curGO);
        var minmax = generateBones(allPoints);

        Tuple<Vector3, Vector3> link = new Tuple<Vector3, Vector3>(max, minmax.Item1);
        if (Vector3.Distance(max, minmax.Item2) < Vector3.Distance(link.Item1, link.Item2))
            link = new Tuple<Vector3, Vector3>(max, minmax.Item2);
        if (Vector3.Distance(min, minmax.Item1) < Vector3.Distance(link.Item1, link.Item2))
            link = new Tuple<Vector3, Vector3>(min, minmax.Item1);
        if (Vector3.Distance(min, minmax.Item2) < Vector3.Distance(link.Item1, link.Item2))
            link = new Tuple<Vector3, Vector3>(min, minmax.Item2);

        GameObject boneLink = new GameObject();
        var lr = boneLink.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, (link.Item1) * transform.localScale.x);
        lr.SetPosition(1, (link.Item2) * transform.localScale.x);
        lr.startWidth = .03f;
        lr.endWidth = .03f;
        lr.sharedMaterial = linkMat;

        for (int i = 0; i < curGO.transform.childCount; i++)
        {
            var newGo = curGO.transform.GetChild(i).gameObject;
            boneStruct(newGo, minmax.Item1, minmax.Item2);
        }
    }

    void InstantiatePoint(Vector3 position)
    {
        GameObject newP = Instantiate(pointPrefab, position, Quaternion.identity);
        newP.transform.parent = parent.transform;
    }

    Tuple<Vector3, Vector3> generateBones(List<Vector3> allPoints)
    {
        //Récupération du barycentre
        Vector3 bary = getBarycentre(allPoints);
        
        //Mise à l'origine
        List<Vector3> pointsToOrigin = new List<Vector3>();
        foreach (var p in allPoints)
        {
            pointsToOrigin.Add(p - bary);
            //var point = Instantiate(pointPrefab);
            //point.transform.position = p;
        }
        
        //On créée la matrice de covalence
        Matrix4x4 matCov = getMatCovariance(pointsToOrigin);
        
        //On calcule les valeurs & vecteurs propres
        var lambda = methodePuissance(matCov);

        List<Vector3> pointsProj = new List<Vector3>();
        var v0 = lambda.Item2;
        Tuple<Vector3,Vector3> max = new Tuple<Vector3, Vector3>(Vector3.zero,Vector3.zero), 
            min = new Tuple<Vector3, Vector3>(Vector3.zero,Vector3.zero);
        foreach (var p in pointsToOrigin)
        {
            var newP = (Vector3.Dot(p, v0)) / v0.sqrMagnitude * v0;
            if (Vector3.Distance(v0 * 100, newP) < Vector3.Distance(v0 * 100, max.Item1))
            {
                max = new Tuple<Vector3, Vector3>(newP, p);
            }
            if (Vector3.Distance(v0 * -100, newP) < Vector3.Distance(v0 * -100, min.Item1))
            {
                min = new Tuple<Vector3, Vector3>(newP, p);
            }            
            pointsProj.Add(newP);
        }

        //var minPoint = Instantiate(pointPrefab);
        //minPoint.transform.position = (min.Item2 + bary) *transform.localScale.x;
        
        //var maxPoint = Instantiate(pointPrefab);
        //maxPoint.transform.position = (max.Item2 + bary) *transform.localScale.x;

        GameObject bone = new GameObject();
        var lr = bone.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, (min.Item2 + bary) * transform.localScale.x);
        lr.SetPosition(1, (max.Item2 + bary) * transform.localScale.x);
        lr.startWidth = .05f;
        lr.endWidth = .05f;
        lr.sharedMaterial = boneMat;

        return new Tuple<Vector3, Vector3>(min.Item2 + bary, max.Item2 + bary);
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

    Tuple<float, Vector3> methodePuissance(Matrix4x4 mat)
    {
        Vector3 v0 = new Vector3(1, 0, 0);
        var lambdaK = 0.0f;
        for (int k = 0; k < 10; k++)
        {
            var mul = mat * v0;
            lambdaK = mul[0];
            if (Mathf.Abs(mul[1]) > lambdaK)
                lambdaK = mul[1];
            if (Mathf.Abs(mul[2]) > lambdaK)
                lambdaK = mul[2];
            v0 = (1 / lambdaK) * mul;
        }

        return new Tuple<float, Vector3>(lambdaK, v0);
    }

    List<Vector3> getPoints(GameObject go)
    {
        // Assurez-vous d'avoir un MeshFilter attaché à votre GameObject
        MeshFilter meshFilter = go.GetComponent<MeshFilter>();

        if (meshFilter != null)
        {
            // Obtenez le mesh du MeshFilter
            Mesh mesh = meshFilter.mesh;

            // Obtenez les vertices du mesh
            return mesh.vertices.ToList();
        }
        else
        {
            Debug.LogError("Aucun MeshFilter attaché au GameObject");
        }

        return null;
    }
}
