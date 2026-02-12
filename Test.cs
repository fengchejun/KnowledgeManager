using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class Test : MonoBehaviour
{
    // 👇 替换成你从DeepSeek控制台新建的API Key
    [Header("请输入你的API Key")]
    public string apiKey = "sk-7b0de10e33124631929cf34351ff9d6e";

    // DeepSeek官方主端点（最稳定）
    private const string TEST_ENDPOINT = "https://api.deepseek.com/v1/chat/completions";

    // 和你原代码一致的保存路径（确保路径相同）
    private string savePath;

    void Start()
    {
        // 初始化保存路径（和你原代码保持一致）
        savePath = Path.Combine(Application.persistentDataPath, "knowledge_data.json");

        // 执行完整测试
      
    }

    /// <summary>
    /// 完整测试流程：检查文件是否存在 → 读取文件内容 → 解析并打印 → 验证数据
    /// </summary>
    //void StartTest()
    //{
    //    Debug.Log("===== 知识点保存验证测试开始 =====");
    //    Debug.Log($"保存文件路径：{savePath}\n");

    //    // 1. 检查文件是否存在
    //    if (!File.Exists(savePath))
    //    {
    //        Debug.LogError("❌ 保存文件不存在！保存失败");
    //        Debug.Log("===== 测试结束 =====");
    //        return;
    //    }
    //    Debug.Log("✅ 文件存在");

    //    // 2. 读取文件完整内容并打印
    //    string fileContent = "";
    //    try
    //    {
    //        // 读取文件（解决中文乱码问题）
    //        using (StreamReader sr = new StreamReader(savePath, Encoding.UTF8))
    //        {
    //            fileContent = sr.ReadToEnd();
    //        }

    //        Debug.Log("\n📄 文件完整内容：");
    //        Debug.Log(fileContent); // 打印原始JSON，直观看到保存的所有数据
    //    }
    //    catch (System.Exception e)
    //    {
    //        Debug.LogError($"❌ 读取文件失败：{e.Message}");
    //        Debug.Log("===== 测试结束 =====");
    //        return;
    //    }

    //    // 3. 解析JSON并验证数据（和你原代码的解析逻辑一致）
    //    try
    //    {
    //        // 解析外层包装类（和你原代码的KnowledgePointWrapper一致）
    //        var wrapper = JsonUtility.FromJson<KnowledgePointWrapper>(fileContent);
    //        if (wrapper == null || wrapper.points == null || wrapper.points.Count == 0)
    //        {
    //            Debug.LogWarning("⚠️ 文件存在但无知识点数据！");
    //            Debug.Log("===== 测试结束 =====");
    //            return;
    //        }

    //        Debug.Log($"\n📊 解析结果：共读取到 {wrapper.points.Count} 个知识点");

    //        // 遍历每个知识点，打印详细信息
    //        for (int i = 0; i < wrapper.points.Count; i++)
    //        {
    //            var kp = wrapper.points[i];
    //            Debug.Log($"\n----- 知识点 {i + 1} -----");
    //            Debug.Log($"ID：{kp.id}");
    //            Debug.Log($"内容：{kp.content}");
    //            Debug.Log($"分类：{kp.category}");
    //            Debug.Log($"标签：{(kp.tags != null ? string.Join(",", kp.tags) : "无")}");
    //            Debug.Log($"题目数量：{(kp.Questions != null ? kp.Questions.Count : 0)}");
    //            Debug.Log($"序列化的题目JSON：{kp.questionJsons}");

    //            // 额外验证：解析questionJsons，确认题目数据是否正确
    //            if (!string.IsNullOrEmpty(kp.questionJsons))
    //            {
    //                var strWrapper = JsonUtility.FromJson<StringListWrapper>(kp.questionJsons);
    //                var questionList = strWrapper?.list ?? new List<string>();
    //                Debug.Log($"解析出的题目JSON数量：{questionList.Count}");
    //                for (int j = 0; j < questionList.Count; j++)
    //                {
    //                    Debug.Log($"题目 {j + 1} JSON：{questionList[j]}");
    //                }
    //            }
    //        }

    //        Debug.Log("\n✅ 所有数据验证完成！保存成功");
    //    }
    //    catch (System.Exception e)
    //    {
    //        Debug.LogError($"❌ 解析JSON失败：{e.Message}\n{e.StackTrace}");
    //    }

    //    Debug.Log("\n===== 测试结束 =====");
    //}


    IEnumerator TestApiKey()
    {
        Debug.Log("=== 开始测试DeepSeek API Key ===");

        // 1. 清理API Key（移除空格/换行）
        string cleanKey = apiKey.Trim().Replace("\n", "").Replace("\r", "").Replace("\t", "");
        if (string.IsNullOrEmpty(cleanKey) || !cleanKey.StartsWith("sk-"))
        {
            Debug.LogError("❌ API Key格式错误！必须以sk-开头");
            yield break;
        }

        // 2. 构建最简请求数据（可序列化）
        var requestData = new TestRequestData
        {
            model = "deepseek-chat",
            messages = new List<TestMessage>
            {
                new TestMessage { role = "user", content = "你好，请回复'Key有效'即可" }
            },
            temperature = 0.1f,
            max_tokens = 10 // 最小返回长度，节省额度
        };

        // 3. 序列化请求数据
        string requestJson = JsonUtility.ToJson(requestData);
        if (string.IsNullOrEmpty(requestJson))
        {
            Debug.LogError("❌ 请求数据序列化失败");
            yield break;
        }

        // 4. 发送POST请求
        using (UnityWebRequest request = new UnityWebRequest(TEST_ENDPOINT, "POST"))
        {
            // 设置请求体
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(requestJson);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            // 设置请求头（核心）
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + cleanKey); // Bearer后必须有空格

            // 设置超时
            request.timeout = 30;

            Debug.Log($"📤 发送请求到：{TEST_ENDPOINT}");
            Debug.Log($"🔑 使用的API Key：{cleanKey.Substring(0, 8)}...（已隐藏后半段）");

            // 发送请求并等待响应
            yield return request.SendWebRequest();

            // 5. 处理响应
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✅ 请求成功！");
                Debug.Log($"📥 响应内容：{request.downloadHandler.text}");

                // 解析响应，验证是否返回正常内容
                var response = JsonUtility.FromJson<TestApiResponse>(request.downloadHandler.text);
                if (response != null && response.choices != null && response.choices.Count > 0)
                {
                    string content = response.choices[0].message.content.Trim();
                    Debug.Log($"🎉 API Key有效！AI回复：{content}");
                }
            }
            else
            {
                // 打印详细错误信息
                Debug.LogError($"❌ 请求失败！");
                Debug.LogError($"   错误类型：{request.error}");
                Debug.LogError($"   响应码：{request.responseCode}");
                Debug.LogError($"   响应内容：{request.downloadHandler.text}");

                // 针对401错误给出明确提示
                if (request.responseCode == 401)
                {
                    Debug.LogError($"💡 401错误原因：API Key无效/过期/额度不足，请去DeepSeek控制台重新生成！");
                    Debug.LogError($"   控制台地址：https://platform.deepseek.com/");
                }
            }
        }

        Debug.Log("=== 测试结束 ===");
    }

    #region 极简序列化模型
    [System.Serializable]
    private class TestRequestData
    {
        public string model;
        public List<TestMessage> messages;
        public float temperature;
        public int max_tokens;
    }

    [System.Serializable]
    private class TestMessage
    {
        public string role;
        public string content;
    }

    [System.Serializable]
    private class TestApiResponse
    {
        public List<TestChoice> choices;
    }

    [System.Serializable]
    private class TestChoice
    {
        public TestMessage message;
    }
    #endregion

    IEnumerator TestDeepSeekConnectivity()
    {
        // 测试1：访问DeepSeek官方域名（验证是否能通境外）
        using (UnityWebRequest request = UnityWebRequest.Head("https://api.deepseek.com"))
        {
            request.timeout = 10;
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✅ 能访问DeepSeek域名");
            }
            else
            {
                Debug.LogError($"❌ 无法访问DeepSeek域名：{request.error}");
                Debug.LogWarning("⚠️ 核心原因：国内网络需代理/科学上网才能访问");
                yield break; // 网络不通，后续测试无意义
            }
        }
    }
        IEnumerator TestIPs()
    {
        string[] testIPs = {
            "8.219.83.66",
            "8.219.203.127",
            "47.251.11.235",
            "47.251.12.123"
        };

        foreach (string ip in testIPs)
        {
            string url = $"https://{ip}/";
            Debug.Log($"测试IP：{ip}");

            using (var request = UnityWebRequest.Head(url))
            {
                request.SetRequestHeader("Host", "api.deepseek.com");
                request.timeout = 10;

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"✅ IP {ip} 可用");
                    // 保存到PlayerPrefs
                    PlayerPrefs.SetString("BestDeepSeekIP", ip);
                    break;
                }
                else
                {
                    Debug.Log($"❌ IP {ip} 不可用：{request.error}");
                }
            }

            yield return new WaitForSeconds(1);
        }
    }
}
