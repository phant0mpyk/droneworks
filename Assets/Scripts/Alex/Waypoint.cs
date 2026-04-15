using System;
using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class Waypoint : MonoBehaviour
{
    private Canvas canvas;
    private Vector3 worldPosition;
    public Sprite sprite;
    private Image img;
    RectTransform rect;
    private Camera mainCam;
    public float scale = 1;

    void Start()
    {
        worldPosition = transform.position;
        canvas = FindFirstObjectByType<Canvas>();
        transform.SetParent(canvas.transform);
        if (!TryGetComponent(out rect))
        {
            rect = gameObject.AddComponent<RectTransform>();
        }
        mainCam = Camera.main;
        if(!TryGetComponent(out img))
        {
            img = gameObject.AddComponent<Image>();
            img.sprite = sprite;
        }        
        rect.localRotation = Quaternion.identity;
        rect.localScale = new Vector3(scale, scale, scale);

        
    }

    

   

    void UpdatePosition()
    {
        
       
        Vector3 viewportPosition = mainCam.WorldToViewportPoint(worldPosition);

        if (Vector3.Dot(mainCam.transform.forward, worldPosition - mainCam.transform.position) < 0)
        {
            if (viewportPosition.x < 0.5)
            {
                viewportPosition.x  = -Screen.width * 0.5f;
            }
            else
            {
                viewportPosition.x = Screen.width * 0.5f;
            }
        }

        if (Vector3.Dot(-mainCam.transform.up, worldPosition - mainCam.transform.position) > 0 &&
            viewportPosition.y > 1 || viewportPosition.y < 0)
        {
            viewportPosition.y = 0;
        }

        
        Vector3 screenPosition=new Vector3(
            (Mathf.Clamp((viewportPosition.x*Screen.width)-(Screen.width*0.5f), - Screen.width*0.5f, Screen.width*0.5f)), 
            (Mathf.Clamp((viewportPosition.y*Screen.height)-(Screen.height*0.5f), - Screen.height*0.5f, Screen.height*0.5f)), 0);
        rect.anchoredPosition3D = screenPosition;
    }
    
    // Update is called once per frame
    void Update()
    {
        UpdatePosition();
    }
    
}
