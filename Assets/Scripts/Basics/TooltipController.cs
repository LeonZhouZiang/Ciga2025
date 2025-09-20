using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Serialization;
using Sequence = DG.Tweening.Sequence;

public class TooltipController : MonoBehaviour
{
    public TMP_Text description;
    public TMP_Text name;
    public float showDelay = 1f;
    public Vector2 offset = new Vector2(20, 20);
    
    private GameObject currentTooltip;
    private RectTransform tooltipRect;
    
    public Canvas canvas;
    private bool isPointerOver;
    private float enterTime;
    
    public Sequence showSequence;

    public static TooltipController instance;

    private Vector3 position;
    private void Awake()
    {
        instance = this;
        currentTooltip = canvas.gameObject;
        tooltipRect = currentTooltip.GetComponent<RectTransform>();
    }
    
    public void StartShow(string name, string description, Vector2 position)
    {
        enterTime = Time.time;
        isPointerOver = true;
        
        this.name.text = name;
        this.description.text = description;
        this.position = position;
        
        // 使用DOTween延迟显示
        showSequence = DOTween.Sequence()
            .AppendInterval(showDelay)
            .OnComplete(ShowTooltip)
            .Play();
        
    }

    public void EndShow()
    {
        isPointerOver = false;
        showSequence?.Kill();
        HideTooltip();
    }

    private void ShowTooltip()
    {
        if (!isPointerOver) return;
        currentTooltip.transform.position = position;
        currentTooltip.SetActive(true);
    }

    private void UpdateTooltipPosition()
    {
        if (currentTooltip == null) return;
        
        // 获取鼠标位置
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            Input.mousePosition,
            canvas.worldCamera,
            out Vector2 localPoint);
        
        // 计算初始位置（带偏移）
        Vector2 tooltipPosition = localPoint + offset;
        
        // 获取Tooltip尺寸
        Vector2 tooltipSize = tooltipRect.sizeDelta;
        
        // 获取Canvas尺寸
        Rect canvasRect = (canvas.transform as RectTransform).rect;
        
        // 检查右边界
        if (tooltipPosition.x + tooltipSize.x > canvasRect.width / 2)
        {
            tooltipPosition.x = canvasRect.width / 2 - tooltipSize.x - 10;
        }
        
        // 检查左边界
        if (tooltipPosition.x < -canvasRect.width / 2)
        {
            tooltipPosition.x = -canvasRect.width / 2 + 10;
        }
        
        // 检查上边界
        if (tooltipPosition.y + tooltipSize.y > canvasRect.height / 2)
        {
            tooltipPosition.y = canvasRect.height / 2 - tooltipSize.y - 10;
        }
        
        // 检查下边界
        if (tooltipPosition.y < -canvasRect.height / 2)
        {
            tooltipPosition.y = -canvasRect.height / 2 + 10;
        }
        
        // 应用位置
        tooltipRect.anchoredPosition = tooltipPosition;
    }

    private void HideTooltip()
    {
        if (currentTooltip == null) return;
        
        currentTooltip.gameObject.SetActive(false);
    }

    private void Update()
    {
        // if (currentTooltip != null && isPointerOver)
        // {
        //     UpdateTooltipPosition();
        // }
    }

    private void OnDisable()
    {
        HideTooltip();
        isPointerOver = false;
        showSequence?.Kill();
    }
}