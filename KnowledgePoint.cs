using System;
using System.Collections.Generic;

[Serializable]
public class KnowledgePoint
{
    // === 基本属性 ===
    public string id;
    public string content;
    public string category;
    public List<string> tags;

    // === 题目列表 - 关键修改 ===
    public List<QuestionBase> Questions;  // 改为 QuestionBase

    // === 序列化字段 ===
    public string questionJsons;

    // === 复习相关字段 ===
    public DateTime createTime;
    public int memoryStage;
    public DateTime nextReviewTime;

    // === 构造函数 ===
    public KnowledgePoint(string content, string category, List<string> tags)
    {
        id = Guid.NewGuid().ToString();
        this.content = content;
        this.category = category;
        this.tags = tags;
        Questions = new List<QuestionBase>();  // 改为 QuestionBase
        createTime = DateTime.Now;
        memoryStage = 0;
        nextReviewTime = DateTime.Now.AddMinutes(10);
    }

    // === 复习完成方法 ===
    public void CompleteReview(bool isAllCorrect)
    {
        if (isAllCorrect)
        {
            memoryStage = Math.Min(memoryStage + 1, 4);
            UpdateNextReviewTime();
        }
    }

    private void UpdateNextReviewTime()
    {
        switch (memoryStage)
        {
            case 0: nextReviewTime = DateTime.Now.AddMinutes(10); break;
            case 1: nextReviewTime = DateTime.Now.AddDays(1); break;
            case 2: nextReviewTime = DateTime.Now.AddDays(3); break;
            case 3: nextReviewTime = DateTime.Now.AddDays(7); break;
            case 4: nextReviewTime = DateTime.MaxValue; break; // 已掌握
        }
    }

    // 文档中的代码片段：
    // 判断是否需要复习（当前时间 ≥ 下次复习时间）
    public bool NeedReview()
    {
        if (memoryStage >= 4) return false; // 已掌握，无需复习
        return DateTime.Now >= nextReviewTime;
    }

}
