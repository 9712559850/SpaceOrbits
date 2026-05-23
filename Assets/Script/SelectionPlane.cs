using System;
using UnityEngine;
using UnityEngine.UI;

public class SelectionPlane : MonoBehaviour
{
    public Transform PlaneList;
    public Transform SelectionIndicator;
    public Image leftPlaneImg, rightPlaneImg;
    Sprite selectedPlaneSprite;

    private void Start()
    {
        int index = PlayerPrefs.GetInt("SelectedPlane", 0);
        SelectPlane(index);
    }
    public void SelectPlane(int index)
    {
        PlayerPrefs.SetInt("SelectedPlane", index);
        Transform currentPlane = PlaneList.GetChild(index);
        currentPlane.gameObject.SetActive(true);
        SelectionIndicator.transform.SetParent(currentPlane);
        SelectionIndicator.transform.localPosition = Vector3.zero;

        selectedPlaneSprite = currentPlane.GetComponent<Image>().sprite;
        leftPlaneImg.sprite = selectedPlaneSprite;
        rightPlaneImg.sprite = selectedPlaneSprite;
    }
}
