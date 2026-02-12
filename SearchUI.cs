using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SearchUI : MonoBehaviour
{

    private List<KnowledgePoint> knowledgePoints = new List<KnowledgePoint>();
    public TextMeshProUGUI knowledgeContent;
    public TextMeshProUGUI knowledgeQuestion;
    // Start is called before the first frame update
    void Start()
    {
        knowledgePoints = KnowledgeManager.Instance.SearchKnowledgePoints();
        knowledgeContent.text = knowledgePoints[3].content;
        knowledgeQuestion.text = knowledgePoints[3].Questions[0].title;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
