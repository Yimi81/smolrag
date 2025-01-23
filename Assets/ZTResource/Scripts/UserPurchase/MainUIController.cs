using System.Collections.Generic;
using UnityEngine;

public class MainUIController : MonoBehaviour
{
    public UserInfoUI userInfoUI;
    public Transform batchParent;
    public GameObject batchPrefab;

    private string csvFilePath; // 新增存储CSV文件路径的变量

    public void SetupPurchaseRecords(string userName, string userAvatar, List<(string batchName, string batchTime, List<CardInfo> cardInfos)> batches, string csvFilePath)
    {
        this.csvFilePath = csvFilePath; // 设置CSV文件路径

        // 清空现有的批次UI
        foreach (Transform child in batchParent)
        {
            Destroy(child.gameObject);
        }

        // 设置用户信息
        userInfoUI.SetupUserInfo(userName, userAvatar);

        // 创建新的批次UI
        foreach (var batch in batches)
        {
            GameObject batchObject = Instantiate(batchPrefab, batchParent);
            BatchUI batchUI = batchObject.GetComponent<BatchUI>();

            batchUI.SetupBatch(batch.batchName, batch.batchTime, batch.cardInfos, csvFilePath); // 传递CSV文件路径

        }
    }
}
