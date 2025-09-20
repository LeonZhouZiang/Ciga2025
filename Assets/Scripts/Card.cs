using System;
using System.Linq;
using System.Threading.Tasks;
using Basics;
using DG.Tweening;
using QFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Slider = UnityEngine.UI.Slider;

[RequireComponent(typeof(SpriteRenderer))]
public class Card : MonoBehaviour, IDragable, IConsumable, IHoverable,IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public CardSO cardSO;
    public LayerMask cardLayer;
    public float dragScale = 1.2f;

    private SpriteRenderer spriteRenderer;
    protected Collider2D cardCollider;

    protected float originalScale;
    // 拖拽状态变量
    private Vector3 offset;
    private bool isDragging = false;

    private bool isRunning = false;
    
    public string description;

    private float moveTimer = 0;
    private float moveCD = 5;
    private float moveCDOffset = 0;

    private Slider progressBar;
    
    public virtual void Awake()
    {
        progressBar = transform.GetComponentInChildren<Slider>(true);
        progressBar.gameObject.SetActive(false);
    }
    
    public async Task StartProgress(float time)
    {
        progressBar.gameObject.SetActive(true);
        isRunning = true;
        
        progressBar.value = 0;
        float elapsedTime = 0f;

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            progressBar.value = Mathf.Clamp01(elapsedTime / time); // 更新进度条 (0~1)
            await Task.Yield(); // 每帧等待（替代 Task.）
        }

        progressBar.value = 1; // 确保最终值为 1
        isRunning = false;
        progressBar.gameObject.SetActive(false);
    }
    
    public virtual void Update()
    {
        if (!isDragging)
        {
            moveTimer += Time.deltaTime;
            if (moveTimer >= moveCD + moveCDOffset)
            {
                moveTimer -= moveCD;
                moveCDOffset = Random.Range(-2f,2f);
                Move();
            }
                
        }

        RestrictPosition();
    }

    private void RestrictPosition()
    {
        float screenAspect = (float)Screen.width / Screen.height;
        float cameraHeight = Camera.main.orthographicSize * 2;
        Vector2 screenBounds = new Vector2(cameraHeight * screenAspect, cameraHeight) / 2f;
        
        float clampedX = Mathf.Clamp(
            transform.position.x,
            -screenBounds.x + 0.5f, 
            screenBounds.x - 0.5f  
        );
        float clampedY = Mathf.Clamp(
            transform.position.y,
            -screenBounds.y + 0.5f, 
            screenBounds.y - 0.5f 
        );

        // 应用限制后的位置
        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }
    
    /// <summary>
    /// Check if the target can be consumed
    /// </summary>
    /// <param name="targetCard"></param>
    /// <returns></returns>
    public virtual bool TryToEat(Card targetCard)
    {
        ConsumeMethod c = cardSO.consumeMethods.FirstOrDefault(x => x.inputCard == targetCard.cardSO);
        if (c != null)
        {
            if(c.ChooseOutput(out ConsumeMethod.Outcome output))
            {
                targetCard.OnBeingConsumed();
                OnEating(output.outputCard, output.number, output.time, true);
                return true;
            }
            else
            {
                targetCard.OnBeingConsumed();
                OnEating(null, 0, output.time, false);
                Debug.Log("No card generated");
            }
        }
        Debug.Log("Unable to eat");
        
        return false;
    }
    
    #region IDragable implementation
    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(eventData.position);
        offset = transform.position - new Vector3(worldPoint.x, worldPoint.y, 0);

        transform.DOKill();
        // 禁用碰撞体避免阻挡射线检测
        if (cardCollider != null) cardCollider.enabled = false;
        else Debug.Log("wtf");
        
        isDragging = true;
        
        CardManager.instance.HandleSortingLayer(transform.GetChild(0).GetComponent<SpriteRenderer>());
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        
        // 将鼠标位置转换为世界坐标
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(eventData.position);
        transform.position = new Vector3(worldPoint.x + offset.x, worldPoint.y + offset.y, transform.position.z);
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        isDragging = false;
        
        int cardLayerMask = 1 << LayerMask.NameToLayer("Card");
        int uiLayerMask = 1 << LayerMask.NameToLayer("UI");
        
        // 检测下方是否有卡
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position, 
            Vector2.zero, 
            Mathf.Infinity, 
            cardLayerMask & ~uiLayerMask);
        
        if (hit.collider != null)
        {
            Debug.Log(hit.collider.gameObject.name + " try to eat");
            if (hit.collider.gameObject.TryGetComponent<Card>(out Card c))
            {
                if(!isRunning)
                    c.TryToEat(this);
            }
            else
            {
                Debug.LogError("No card component found");
            }
        }
        // 恢复碰撞体
        if (cardCollider != null) cardCollider.enabled = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //scale
        transform.DOScale(dragScale, 0.3f);
        TooltipController.instance.EndShow();
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        //scale to original
        transform.DOScale(originalScale, 0.3f);
    }
    
    #endregion

    protected virtual async Task OnEating(CardSO targetCardSO, int number, float time, bool shit)
    {
        PlayEatSound();
        try
        {
            await StartProgress(time);
            // 延迟结束后执行
            if(targetCardSO != null)
                ExecuteEffect(targetCardSO, number);
            if (shit)
                PlayShitSound();
        }
        catch (Exception e) when (e is TaskCanceledException || e is OperationCanceledException)
        {
            Debug.Log("效果执行被取消");
        }

    }

    public void PlayEatSound()
    {
        AudioKit.PlaySound("resources://吃");
    }

    public void PlayShitSound()
    {
        AudioKit.PlaySound("resources://拉");
    }

    public virtual void ExecuteEffect(CardSO targetCardSo, int number)
    {
        CardManager.instance.SpawnCard(targetCardSo, transform.position, number);
        Destroy(gameObject);
    }
    
    public virtual void OnBeingConsumed()
    {
        
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        DOTween.Kill(this); 
    }

    public virtual void Initialize()
    {
        Transform content = transform.Find("Content");
        
        spriteRenderer = content.GetComponentInChildren<SpriteRenderer>();
        cardCollider = GetComponent<Collider2D>();
        originalScale = transform.localScale.x;
        spriteRenderer.sprite = cardSO.cardSprite;
        description = cardSO.description;
        cardLayer = LayerMask.GetMask("Card");
    }

    public void SpawnMove()
    {
        // 确保初始状态
        transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        transform.rotation = Quaternion.identity;
        
        // 随机方向参数
        float distance = Random.Range(0.6f, 1.2f); // 弹出距离
        float angle = Random.Range(0, 360);       // 随机角度
        Vector3 direction = new Vector3(
            Mathf.Sin(angle * Mathf.Deg2Rad), 
            Mathf.Cos(angle * Mathf.Deg2Rad), 
            0).normalized;
        
        // 动画序列
        Sequence spawnSequence = DOTween.Sequence();
        
        // 1. 初始弹出（快速放大+移动）
        spawnSequence.Append(transform.DOScale(1.2f, 0.3f).SetEase(Ease.Linear));
        spawnSequence.Join(transform.DOMove(transform.position + direction * distance, 0.4f)).OnComplete(
            ()=>transform.DOLocalMoveY(transform.position.y + 0.1f, 0.2f).OnComplete(() => transform.DOLocalMoveY(transform.position.y - 0.1f, 0.2f))
            );
        
        // 3. 最终微调
        spawnSequence.Append(transform.DOScale(1f, 0.1f));
    }
    
    public virtual void Move()
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        // 计算目标位置
        Vector2 targetPosition = (Vector2)transform.position + randomDirection * 1;
        transform.DOMove(targetPosition, 0.8f)
            .SetEase(Ease.InOutSine);
        //transform.DOJump(targetPosition, 1, 1, 0.8f);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipController.instance.StartShow(cardSO.cardName, description, transform.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipController.instance.EndShow();
    }

}
