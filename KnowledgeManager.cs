using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;

public class KnowledgeManager : MonoBehaviour
{
    public static KnowledgeManager Instance;
    private List<KnowledgePoint> allKnowledgePoints = new List<KnowledgePoint>();
    private string savePath; // 数据保存路径（替代PlayerPrefs，更稳定）

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 初始化保存路径（建议用PersistentDataPath，避免数据丢失）
        savePath = Path.Combine(Application.persistentDataPath, "knowledge_data.json");
        LoadKnowledgePoints();
    }

    #region 知识点增删改查
    // 新增知识点
    public void AddKnowledgePoint(string content, string category, List<string> tags, Action<bool> onComplete = null)
    {
        if (string.IsNullOrEmpty(content))
        {
            Debug.LogError("知识点内容不能为空！");
            onComplete?.Invoke(false);
            return;
        }

        var newKP = new KnowledgePoint(content, category, tags);
        allKnowledgePoints.Add(newKP);

        // 调用DeepSeek生成题目（原有逻辑）
        StartCoroutine(DeepSeekQuestionGenerator.Instance.GenerateQuestionsForKnowledge(newKP, (success) =>
        {
            if (success)
            {
                SaveKnowledgePoints();
                Debug.Log("知识点添加并生成题目成功：" + content);
            }
            else
            {
                allKnowledgePoints.Remove(newKP);
                Debug.LogError("知识点添加失败：题目生成失败");
            }
            onComplete?.Invoke(success);
        }));
    }

    // 删除知识点
    public void DeleteKnowledgePoint(string id)
    {
        var target = allKnowledgePoints.FirstOrDefault(kp => kp.id == id);
        if (target != null)
        {
            allKnowledgePoints.Remove(target);
            SaveKnowledgePoints();
            Debug.Log("知识点删除成功");
        }
    }

    // 修改知识点
    public void UpdateKnowledgePoint(string id, string newContent, string newCategory, List<string> newTags)
    {
        var target = allKnowledgePoints.FirstOrDefault(kp => kp.id == id);
        if (target != null)
        {
            target.content = newContent;
            target.category = newCategory;
            target.tags = newTags;
            SaveKnowledgePoints();
            Debug.Log("知识点修改成功");
        }
    }

    // 搜索知识点
    public List<KnowledgePoint> SearchKnowledgePoints(string keyword = "", string category = "", List<string> tags = null)
    {
        var query = allKnowledgePoints.AsQueryable();
        if (!string.IsNullOrEmpty(keyword)) query = query.Where(kp => kp.content.Contains(keyword));
        if (!string.IsNullOrEmpty(category)) query = query.Where(kp => kp.category == category);
        if (tags != null && tags.Count > 0) query = query.Where(kp => kp.tags.Intersect(tags).Any());
        return query.ToList();
    }
    #endregion

    #region 数据持久化（新增复习字段的序列化）
    // 保存知识点到本地（用PersistentDataPath替代PlayerPrefs，支持更大数据）
    public void SaveKnowledgePoints()
    {
        try
        {
            // 先将题目列表序列化为JSON字符串
            foreach (var kp in allKnowledgePoints)
            {
                // 【关键修复】先初始化Questions，避免null
                if (kp.Questions == null)
                    kp.Questions = new List<QuestionBase>();

                List<string> questionJsons = new List<string>();
                foreach (var q in kp.Questions)
                {
                    // 【额外防护】跳过null的题目实例
                    if (q == null) continue;
                    questionJsons.Add(q.ToJson());
                }

                // 【修复】JsonUtility不支持直接序列化List<string>，套包装类
                var strWrapper = new StringListWrapper { list = questionJsons };
                kp.questionJsons = JsonUtility.ToJson(strWrapper);
            }

            // 序列化所有知识点
            string json = JsonUtility.ToJson(new KnowledgePointWrapper { points = allKnowledgePoints }, true);
            File.WriteAllText(savePath, json);
            Debug.Log("知识点数据保存成功：" + savePath);
        }
        catch (Exception e)
        {
            Debug.LogError("知识点保存失败：" + e.Message);
        }
    }

    // 从本地加载知识点
    private void LoadKnowledgePoints()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("无本地知识点数据，初始化空列表");
            allKnowledgePoints = new List<KnowledgePoint>();
            return;
        }

        try
        {
            string json = File.ReadAllText(savePath);
            var wrapper = JsonUtility.FromJson<KnowledgePointWrapper>(json);
            allKnowledgePoints = wrapper?.points ?? new List<KnowledgePoint>();

            // 反序列化题目列表
            foreach (var kp in allKnowledgePoints)
            {
                if (!string.IsNullOrEmpty(kp.questionJsons))
                {
                    var questionJsonList = JsonUtility.FromJson<List<string>>(kp.questionJsons);
                    kp.Questions = new List<QuestionBase>();  // 改为 QuestionBase

                    foreach (var qJson in questionJsonList)
                    {
                        // 先读取基础信息判断类型
                        var temp = JsonUtility.FromJson<QuestionBase>(qJson);

                        QuestionBase question = null;
                        switch (temp.Type)
                        {
                            case QuestionType.Choice:
                                question = JsonUtility.FromJson<ChoiceQuestion>(qJson);
                                break;
                            case QuestionType.TrueFalse:
                                question = JsonUtility.FromJson<TrueFalseQuestion>(qJson);
                                break;
                            case QuestionType.FillBlank:
                                question = JsonUtility.FromJson<FillBlankQuestion>(qJson);
                                break;
                        }

                        if (question != null)
                            kp.Questions.Add(question);
                    }
                }
            }

            Debug.Log("知识点数据加载成功，共" + allKnowledgePoints.Count + "个知识点");
        }
        catch (Exception e)
        {
            Debug.LogError("知识点加载失败：" + e.Message);
            allKnowledgePoints = new List<KnowledgePoint>();
        }
    }

    // 清空所有知识点
    public void ClearAllKnowledgePoints()
    {
        allKnowledgePoints.Clear();
        if (File.Exists(savePath)) File.Delete(savePath);
        Debug.Log("所有知识点已清空");
    }
    #endregion

    #region 辅助方法
    public List<KnowledgePoint> GetAllKnowledgePoints() => allKnowledgePoints;
    public List<string> GetAllCategories() => allKnowledgePoints.Select(kp => kp.category).Distinct().ToList();
    public List<string> GetAllTags() => allKnowledgePoints.SelectMany(kp => kp.tags).Distinct().ToList();
    #endregion

    #region 序列化包装类
    [Serializable]
    private class KnowledgePointWrapper
    {
        public List<KnowledgePoint> points;
    }

    [Serializable]
    private class QuestionTypeWrapper
    {
        public QuestionType type;
    }

    [Serializable]
    private class StringListWrapper
    {
        public List<string> list; // 字段必须是public，否则JsonUtility无法序列化
    }
    #endregion
}