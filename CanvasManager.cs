using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    private GameObject currentCanvas;
    [SerializeField] private GameObject addKnowledgePointsCanvas;
    [SerializeField] private GameObject showKnowledgePointsCanvas;

    // Start is called before the first frame update
    void Start()
    {
        currentCanvas = GameObject.Find("AddKnowledge_Canvas");
        if (currentCanvas == null)
            currentCanvas = addKnowledgePointsCanvas;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenShowKnowledgePointsCanvas()
    {
        currentCanvas.SetActive(false);
        showKnowledgePointsCanvas.SetActive(true);
    }


}
