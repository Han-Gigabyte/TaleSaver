using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableUI : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 offset;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // �巡�� ���� �� Ŭ���� ��ġ�� ������Ʈ �߽��� �Ÿ� ����
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, eventData.position, eventData.pressEventCamera, out offset);
    }

    public RectTransform boundaryRect;  // BoardCanvas
    public float margin = 30f;          // ��� �ּ� ����

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localMousePosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, eventData.position, eventData.pressEventCamera, out localMousePosition))
        {
            Vector2 newPos = localMousePosition - offset;

            Rect boundary = boundaryRect.rect;
            float popupWidth = rectTransform.rect.width;
            float popupHeight = rectTransform.rect.height;

            float pivotX = rectTransform.pivot.x;
            float pivotY = rectTransform.pivot.y;

            //  ���� ���: �������� margin �̻� ���ƾ�
            float minX = boundary.xMin + margin - (1 - pivotX) * popupWidth;

            //  ���� ���: ������ margin �̻� ���ƾ�
            float maxX = boundary.xMax - margin - pivotX * popupWidth;

            //  �ϴ� ���: Top�� margin �̻� ������
            float minY = boundary.yMin + margin - (1 - pivotY) * popupHeight;

            //  ��� ���: Bottom�� margin �̻� ������
            float maxY = boundary.yMax - margin - pivotY * popupHeight;

            // ��ġ ����
            newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
            newPos.y = Mathf.Clamp(newPos.y, minY, maxY);

            rectTransform.anchoredPosition = newPos;
        }
    }



}
