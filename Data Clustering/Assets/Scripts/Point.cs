using UnityEngine;

public class Point : MonoBehaviour
{
    public Vector3 pos;
    public int cluster = 0;
    Renderer renderer;

    private void Start()
    {
        pos = this.transform.position;
    }

    public void SetColor(Color color)
    {
        renderer = this.gameObject.GetComponent<Renderer>();
        renderer.material.color = color;
    }
}
