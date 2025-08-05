using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class ChargeBar : MonoBehaviour
{
    [Range(0f, 1f)]
    public float value;
    [SerializeField]
    RectTransform mask, bar;
    
    void Update()
    {
        mask.localPosition = new Vector3(210f * (1f - value), 0f, 0f);
        bar.localPosition = new Vector3(-140f - 210f * (1f - value), 0f, 0f);
    }
}
