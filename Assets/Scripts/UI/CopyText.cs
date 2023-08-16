using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CopyText : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IEndDragHandler
{
    private TMP_Text textComponent;
    private bool isDragging;
    private int startIndex;
    private int endIndex;

    private Color originalColor;
    public Color selectionColor;

    void Start()
    {
        textComponent = GetComponent<TMP_Text>();
        originalColor = textComponent.color;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        startIndex = TMP_TextUtilities.FindIntersectingCharacter(textComponent, eventData.position, null, true);
        isDragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        endIndex = TMP_TextUtilities.FindIntersectingCharacter(textComponent, eventData.position, null, true);
        isDragging = false;

        // 复制选中的文本到剪贴板
        string selectedText = textComponent.text.Substring(startIndex, endIndex - startIndex);
        GUIUtility.systemCopyBuffer = selectedText;
        Debug.Log("Text Copied: " + selectedText);

        // 恢复原始颜色
        ResetSelectionColor();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            endIndex = TMP_TextUtilities.FindIntersectingCharacter(textComponent, eventData.position, null, true);
            textComponent.ForceMeshUpdate();

            // 修改选中文本的颜色
            TMP_TextInfo textInfo = textComponent.textInfo;
            for (int i = 0; i < textInfo.characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                int charIndex = charInfo.index;

                if (charIndex >= startIndex && charIndex <= endIndex && charInfo.isVisible)
                {
                    int materialIndex = charInfo.materialReferenceIndex;
                    int vertexIndex = charInfo.vertexIndex;

                    Color32[] vertexColors = textInfo.meshInfo[materialIndex].colors32;
                    vertexColors[vertexIndex + 0] = selectionColor;
                    vertexColors[vertexIndex + 1] = selectionColor;
                    vertexColors[vertexIndex + 2] = selectionColor;
                    vertexColors[vertexIndex + 3] = selectionColor;
                }
            }

            // 更新文本网格
            textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        // 恢复原始颜色
        ResetSelectionColor();
    }

    // 恢复选中文本的颜色为原始颜色
    private void ResetSelectionColor()
    {
        TMP_TextInfo textInfo = textComponent.textInfo;
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            TMP_MeshInfo meshInfo = textInfo.meshInfo[i];
            Color32[] vertexColors = meshInfo.colors32;
            for (int j = 0; j < meshInfo.vertexCount; j++)
            {
                vertexColors[j] = originalColor;
            }
        }

        // 更新文本网格
        textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }
}