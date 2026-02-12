// 引入必要的命名空间
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 这个类负责知识点录入的UI交互
public class KnowledgeInputUI : MonoBehaviour
{
    // 【第一步】定义需要绑定的UI组件
    public TMP_InputField contentInput; // 知识点内容输入框
    public TMP_InputField categoryInput; // 分类输入框
    public TMP_InputField tagsInput; // 标签输入框
    public Button addButton; // 新增按钮
    public TMP_Text tipText; // 提示文本

    // 【第二步】游戏启动时初始化：给按钮绑定点击事件
    private void Start()
    {
        // 当用户点击addButton按钮时，自动执行OnAddButtonClick方法
        addButton.onClick.AddListener(OnAddButtonClick);
    }

    // 【第三步】按钮点击后的核心逻辑（重点）
    private void OnAddButtonClick()
    {
        // 1. 获取用户输入的内容，并去除首尾空格
        string content = contentInput.text.Trim();
        string category = categoryInput.text.Trim();
        // 标签处理：把用户输入的字符串按逗号拆分，转成List<string>
        List<string> tags = tagsInput.text.Trim().Split(',').Select(t => t.Trim()).ToList();

        // 2. 禁用按钮，避免用户重复点击（防止重复生成知识点）
        addButton.interactable = false;
        // 显示提示：告诉用户正在处理
        tipText.text = "正在生成题目...";

        // 3. 调用KnowledgeManager的AddKnowledgePoint方法，新增知识点
        KnowledgeManager.Instance.AddKnowledgePoint(content, category, tags, (success) =>
        {
            // 这个lambda表达式是【回调函数】—— 等AddKnowledgePoint和API调用完成后才会执行

            // 3.1 不管成功失败，都重新启用按钮
            addButton.interactable = true;

            // 3.2 根据结果更新提示文本
            if (success)
            {
                // 成功：提示用户，清空输入框
                tipText.text = "知识点添加成功！";
                contentInput.text = "";
                categoryInput.text = "";
                tagsInput.text = "";
            }
            else
            {
                // 失败：提示用户原因
                tipText.text = "添加失败：题目生成失败";
            }
        });
    }
}