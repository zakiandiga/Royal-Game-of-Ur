using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using IngameDebugConsole;
using UnityEngine.InputSystem;


public class DebugLogControlVR : MonoBehaviour
{
    [SerializeField] private InputActionReference logToggle, logScroll;

    [SerializeField] private RectTransform logContainer;

    private DebugLogManager logManager;

    private bool logManagerIsOpen = false;
    private bool toggleOnProgress = false;

    private Vector2 scrollDirection = Vector2.zero;
    private float scrollSpeed = 5f;

    private void Start()
    {
        logManager = GetComponent<DebugLogManager>();
    }

    private void OnEnable()
    {
        logToggle.action.Enable();
        logScroll.action.Enable();
    }

    private void OnDisable()
    {
        logToggle.action.Disable();
        logScroll.action.Disable();
    }

    private void LogManagerToggle()
    {
        if(!logManagerIsOpen)
        {
            logManager.ShowLogWindow();
            logManagerIsOpen = true;
            toggleOnProgress = false;
        }

        else if(logManagerIsOpen)
        {
            logManager.HideLogWindow();
            logManagerIsOpen = false;
            toggleOnProgress = false;
        }        
    }

    public void ScrollUpLogWindow()
    {
        logContainer.anchoredPosition = new Vector2 (logContainer.anchoredPosition.x, logContainer.anchoredPosition.y + scrollSpeed * Time.deltaTime);
    }

    public void ScrollDownLogWindow()
    {
        logContainer.anchoredPosition = new Vector2(logContainer.anchoredPosition.x, logContainer.anchoredPosition.y - scrollSpeed * Time.deltaTime);
    }

    private void Update()
    {
        if(logToggle.action.triggered)
        {
            if(!toggleOnProgress)
            {                
                LogManagerToggle();
                toggleOnProgress = true;
            }
            
        }

        scrollDirection = logScroll.action.ReadValue<Vector2>();
        
        if(scrollDirection.y > 0)
        {
            ScrollUpLogWindow();
        }    
        if(scrollDirection.y < 0)
        {
            ScrollDownLogWindow();
        }
    }
}
