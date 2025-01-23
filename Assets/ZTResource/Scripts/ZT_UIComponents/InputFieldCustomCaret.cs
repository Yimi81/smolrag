using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputFieldCustomCaret : MonoBehaviour
{
    public TMP_InputField tmpInputField; // 关联到TMP_InputField组件
    public RectTransform customCaretImage; // 关联到自定义的光标Image的RectTransform
    public float yOffset = 0f; // Y轴偏移值
    public float blinkRate = 0.5f; // 闪烁频率（秒）

    private float blinkTimer;
    private bool isCaretVisible;

    private void Start()
    {
        // 禁用TMP_InputField自带的光标
        tmpInputField.caretBlinkRate = 0f;
        tmpInputField.caretWidth = 0;

        // 初始化自定义光标的闪烁状态
        isCaretVisible = true;
        blinkTimer = 0f;
    }

    private void Update()
    {
        if (tmpInputField.isFocused)
        {
            // 显示并更新光标位置
            customCaretImage.gameObject.SetActive(true);

            // 获取光标的位置
            int caretPosition = tmpInputField.stringPosition;
            Vector2 caretLocalPosition = GetCaretPosition(caretPosition);

            // 应用Y轴偏移
            caretLocalPosition.y += yOffset;

            // 更新光标位置
            customCaretImage.anchoredPosition = caretLocalPosition;

            // 处理光标闪烁
            HandleBlinking();
        }
        else
        {
            // 隐藏光标
            customCaretImage.gameObject.SetActive(false);
        }
    }

    private Vector2 GetCaretPosition(int caretPosition)
    {
        // 获取字符的信息
        TMP_Text textComponent = tmpInputField.textComponent;
        TMP_CharacterInfo charInfo = textComponent.textInfo.characterInfo[caretPosition];

        // 计算光标位置（基于字符的底部）
        float cursorPosX = charInfo.origin;
        float cursorPosY = charInfo.descender;

        return new Vector2(cursorPosX, cursorPosY);
    }

    private void HandleBlinking()
    {
        blinkTimer += Time.deltaTime;

        if (blinkTimer >= blinkRate)
        {
            isCaretVisible = !isCaretVisible;
            customCaretImage.gameObject.SetActive(isCaretVisible);
            blinkTimer = 0f;
        }
    }
}