using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Cluster
{
    public List<Point> pointsList;
    public GameObject meanPosition;
    public Color clusterColor;

    public Cluster(List<Point> list, GameObject position, Color color)
    {
        pointsList = list;
        meanPosition = position;
        clusterColor = color;
        Renderer renderer = meanPosition.gameObject.GetComponent<Renderer>();
        renderer.material.color = Color.black;
    }
}

public class PointPlacement : MonoBehaviour
{
    public GameObject pointPrefab;
    public GameObject meanPrefab;
    private Vector3 point;
    public List<Cluster> clusters;
    public Cluster cluster1;
    public Cluster cluster2;
    public Cluster cluster3;
    public Dictionary<int, Color> colors;
    public float distanceToNeighbour = 1f;
    public int minimumPoints = 4;

    private void Start()
    {
        colors = new Dictionary<int, Color>();
        colors.Add(1, Color.red);
        colors.Add(2, Color.blue);
        colors.Add(3, Color.green);
        colors.Add(4, Color.yellow);
        colors.Add(5, Color.black);
    }


    public void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            point.z = 0;
            Instantiate(pointPrefab, point, Quaternion.identity, this.transform);
        }
        if (Input.GetMouseButtonDown(1))
        {
            float randomCap = Random.Range(60, 100);
            for (int i = 0; i < 3; i++)
            {
                //RandomSpawn();
                RandomClusterSpawn();
            }
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            InitialiseClusters();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            IterateClusters();
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            InitialiseMedoidClusters();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            IterateMedoidClusters();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            DBScan();
        }
    }

    public void RandomSpawn()
    {
        Vector2 randomPositionOnScreen = Camera.main.ViewportToWorldPoint(new Vector2(Random.value, Random.value));
        Instantiate(pointPrefab, randomPositionOnScreen, Quaternion.identity, this.transform);
    }

    public void RandomClusterSpawn()
    {
        Vector2 randomPositionOnScreen = Camera.main.ViewportToWorldPoint(new Vector2(Random.Range(0f, 0.85f), Random.Range(0f, 0.75f)));
        
        for (int i = 0; i < Random.Range(30, 50); i++)
        {
            Vector2 pos = randomPositionOnScreen + new Vector2(Random.Range(0f, 3f), Random.Range(0f, 3f));
            Instantiate(pointPrefab, pos, Quaternion.identity, this.transform);
        }
    }

    public void InitialiseClusters()
    {
        cluster1 = new Cluster(new List<Point>(), Instantiate(meanPrefab, Vector3.zero, Quaternion.identity), Color.red);
        cluster2 = new Cluster(new List<Point>(), Instantiate(meanPrefab, Vector3.zero, Quaternion.identity), Color.blue);
        cluster3 = new Cluster(new List<Point>(), Instantiate(meanPrefab, Vector3.zero, Quaternion.identity), Color.green);
        clusters = new List<Cluster>();
        clusters.Add(cluster1);
        clusters.Add(cluster2);
        clusters.Add(cluster3);

        Point[] points = GetComponentsInChildren<Point>();
        foreach (Point child in points)
        {
            int random = Random.Range(1, 4);
            switch (random)
            {
                case 1:
                    cluster1.pointsList.Add(child);
                    break;
                case 2:
                    cluster2.pointsList.Add(child);
                    break;
                case 3:
                    cluster3.pointsList.Add(child);
                    break;
            }
        }
        CalculateMeans();
    }

    public void InitialiseMedoidClusters()
    {
        cluster1 = new Cluster(new List<Point>(), Instantiate(meanPrefab, Vector3.zero, Quaternion.identity), Color.red);
        cluster2 = new Cluster(new List<Point>(), Instantiate(meanPrefab, Vector3.zero, Quaternion.identity), Color.blue);
        cluster3 = new Cluster(new List<Point>(), Instantiate(meanPrefab, Vector3.zero, Quaternion.identity), Color.green);
        clusters = new List<Cluster>();
        clusters.Add(cluster1);
        clusters.Add(cluster2);
        clusters.Add(cluster3);

        Point[] points = GetComponentsInChildren<Point>();
        foreach (Point child in points)
        {
            int random = Random.Range(1, 4);
            switch (random)
            {
                case 1:
                    cluster1.pointsList.Add(child);
                    break;
                case 2:
                    cluster2.pointsList.Add(child);
                    break;
                case 3:
                    cluster3.pointsList.Add(child);
                    break;
            }
        }
        ComputeCenter();
    }

    private void ComputeCenter()
    {
        foreach(Cluster cluster in clusters)
        {
            Vector3 midpoint = GetVectorMean(cluster.pointsList);
            cluster.meanPosition.transform.position = GetClosestPoint(cluster.pointsList, midpoint);
            foreach (Point point in cluster.pointsList)
            {
                point.SetColor(cluster.clusterColor);
            }
        }
    }

    private void IterateMedoidClusters()
    {
        foreach (Cluster cluster in clusters)
        {
            cluster.pointsList.Clear();
        }
        Point[] points = GetComponentsInChildren<Point>();
        foreach (Point child in points)
        {
            GetClosestMean(child);
        }
        ComputeCenter();
    }

    private void CalculateMeans()
    {
        foreach(Cluster cluster in clusters)
        {
            cluster.meanPosition.transform.position = GetVectorMean(cluster.pointsList);
            foreach(Point point in cluster.pointsList)
            {
                point.SetColor(cluster.clusterColor);
            }
        }
    }

    private Vector3 GetVectorMean(List<Point> clusterPoints)
    {
        Vector3 meanVector = Vector3.zero;

        foreach (Point point in clusterPoints)
        {
            meanVector += point.pos;
        }

        return (meanVector / clusterPoints.Count);
    }

    private void IterateClusters()
    {
        foreach (Cluster cluster in clusters)
        {
            cluster.pointsList.Clear();
        }
        Point[] points = GetComponentsInChildren<Point>();
        foreach (Point child in points)
        {
            GetClosestMean(child);
        }
        CalculateMeans();
    }

    private void GetClosestMean(Point point)
    {
        float closestDistanceSqr = Mathf.Infinity;
        int bestClusterID = 0;
        for(int i=0; i < clusters.Count; i++)
        {
            Vector3 directionToTarget = clusters[i].meanPosition.transform.position - point.pos;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if(dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestClusterID = i;
            }
        }
        clusters[bestClusterID].pointsList.Add(point);
    }

    private Vector3 GetClosestPoint(List<Point> points, Vector3 meanPoint)
    {
        Vector3 closestPoint = Vector3.zero;
        float closestDistanceSqr = Mathf.Infinity;
        foreach(Point point in points)
        {
            Vector3 directionToTarget = point.pos - meanPoint;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                closestPoint = point.pos;
            }
        }
        return closestPoint;
    }

    private void DBScan()
    {
        Point[] points = GetComponentsInChildren<Point>();
        int clusterID = 1;
        foreach (Point child in points)
        {
            if (child.cluster == 0)
            {
                Collider[] hitColliders = Physics.OverlapSphere(child.pos, distanceToNeighbour);
                if(hitColliders.Length > minimumPoints)
                {
                    child.SetColor(colors[clusterID]);
                    child.cluster = clusterID;
                    DBScanIterate(child, clusterID);
                    clusterID = clusterID + 1;
                }
            }
        }
    }

    private void DBScanIterate(Point point, int clusterID)
    {
        Collider[] hitColliders = Physics.OverlapSphere(point.pos, distanceToNeighbour);
        foreach (var hitCollider in hitColliders)
        {
            Point newPoint = hitCollider.gameObject.GetComponent<Point>();
            if (newPoint.cluster == 0)
            {
                newPoint.cluster = clusterID;
                newPoint.SetColor(colors[clusterID]);
                DBScanIterate(newPoint, clusterID);
            }
        }
    }
}
