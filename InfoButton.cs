using UnityEngine;
using TMPro;

public class InfoButton : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI description;
    [SerializeField] string text;
    bool on = false;

    void Start()
    {
        description.text = text;
    }

    public void OnClicked()
    {
        on = !on;
        description.gameObject.transform.parent.gameObject.SetActive(on);
    }
}
