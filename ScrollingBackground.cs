using UnityEngine;
using UnityEngine.UI;

public class ScrollingBackground : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] Vector2 direction;
    Image image;

    void Start()
    {
        image = GetComponent<Image>();
    }
    
    void Update()
    {
        image.material.mainTextureOffset += -direction.normalized * Time.deltaTime * speed;
    }
}
