using UnityEngine;


public class Stick : MonoBehaviour
{
    float speed;
    float life;
    float time = 0;
    float size;

    RectTransform rect;
    float startSize;

    public void Initialize(
        int rotate,
        float speed,
        float life,
        float size
    ) {
        transform.rotation = Quaternion.Euler(0, 0, rotate);
        this.speed = speed;
        this.life = life;
        this.size = size;
    }

    void Start()
    {
        rect = GetComponent<RectTransform>();

        startSize = rect.sizeDelta.y;

        transform.position += transform.right * speed * 0.2f;
        rect.sizeDelta = new Vector2(size, rect.sizeDelta.y);
    }

    void Update()
    {
        time += Time.deltaTime;
        float t = Mathf.Clamp01(time / life);

        //Position(1);
        //Size_x(t);
        Size_y(t);

        if(life <= time) Destroy(gameObject);
    }

    private void Position(float n)
    {
        transform.position += transform.right * speed * n * Time.deltaTime;
    }

    private void Size_x(float t)
    {
        
    }

    private void Size_y(float t)
    {
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, Mathf.Lerp(startSize, 0f, t));
    }
}
