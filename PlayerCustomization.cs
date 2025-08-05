using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerCustomization : MonoBehaviour
{
    GameManager gm;
    Vector3 rotationSpeed = new Vector3(0f, 1f, 0f);

    [SerializeField]
    Transform mainCam, display, headZoom, bodyZoom, legZoom;
    [SerializeField]
    GameObject footSelection;

    public Vector3 targetPosition;
    public Quaternion targetRotation;

    void Start()
    {
        gm = GameManager.instance;
        targetPosition = bodyZoom.localPosition;
        targetRotation = bodyZoom.localRotation;
    }

    void Update()
    {
        mainCam.Rotate(rotationSpeed * Time.deltaTime, Space.World);

        display.localPosition = Vector3.Lerp(display.localPosition, targetPosition, Time.deltaTime * 6f);
        display.localRotation = Quaternion.Lerp(display.localRotation, targetRotation, Time.deltaTime * 10f);
    }

    public void SetDominantFoot(bool enabled)
    {
        string selected = EventSystem.current.currentSelectedGameObject.name;
        if (int.TryParse(selected, out int result))
            gm.dominantFoot = result;
    }

    public void SwitchView()
    {
        string selected = EventSystem.current.currentSelectedGameObject.name;
        if (selected == "Head")
        {
            footSelection.SetActive(false);
            targetPosition = headZoom.localPosition;
            targetRotation = headZoom.localRotation;
        }
        else if (selected == "Leg")
        {
            footSelection.SetActive(true);
            targetPosition = legZoom.localPosition;
            targetRotation = legZoom.localRotation;
        }
        else
        {
            footSelection.SetActive(false);
            targetPosition = bodyZoom.localPosition;
            targetRotation = bodyZoom.localRotation;
        }
    }
}
