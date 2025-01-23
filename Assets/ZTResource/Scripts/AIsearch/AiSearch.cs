using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System;

public class AiSearch : MonoBehaviour
{
    public Button sendButton;
    public TMP_InputField inputField;
    public TMP_Text resultText;

    // 引用 ResourceFilterResult，用于访问搜索记录
    public ResourceFilterResult resourceFilterResult;

    // 定义事件
    public event Action<List<string>> OnQueryResultReceived;

    void Start()
    {
        sendButton.onClick.AddListener(OnSendButtonClick);
    }

    void OnSendButtonClick()
    {
        string query = inputField.text;

        // 添加搜索记录
        if (resourceFilterResult != null)
        {
            resourceFilterResult.AddSearchHistory(query);
        }

        StartCoroutine(SendQuery(query));
    }

    IEnumerator SendQuery(string query)
    {
        string url = "http://10.1.14.151:8005/vectory_query";
        string jsonData = $"{{\"query\": \"{query}\", \"numbers\": 500, \"return_fields\": [\"id\"]}}";

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string result = request.downloadHandler.text;

            // 解析返回的JSON数据
            JToken ids = JToken.Parse(result)["ids"];
            if (ids != null)
            {
                List<string> idList = ids.ToObject<List<string>>();
                resultText.text = string.Join(",", idList);

                // 触发事件
                OnQueryResultReceived?.Invoke(idList);
            }
            else
            {
                resultText.text = "No IDs found.";
            }
        }
        else
        {
            resultText.text = "Error: " + request.error;
        }
    }
}
