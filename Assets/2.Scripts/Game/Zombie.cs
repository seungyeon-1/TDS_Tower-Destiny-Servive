using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum ZombieState
{
    Moving,   // 이동 중
    Climbing, // 다른 좀비를 올라가는 중
    Attacking, // 좀비가 트럭을 공격하는 상태
    Stacked   // 쌓여서 발판이 된 상태
}

public class Zombie : MonoBehaviour
{
    [Header("기본 설정")]
    public ZombieState currentState = ZombieState.Moving;
    public float moveSpeed = 5.0f;

    [Header("쌓기 설정")]
    public LayerMask obstacleLayer;
    public LayerMask zombieLayer; // 이 레이어는 OverlapCircleAll 등에서 모든 좀비를 감지할 때 사용
    public float checkDistance = 0.3f;
    [Tooltip("램프 한 칸의 높이")]
    public float rampSegmentHeight = 1.0f;
    [Tooltip("램프 한 칸의 너비")]
    public float rampSegmentWidth = 0.7f;
    public float climbSpeed = 5.0f;
    [Tooltip("앞 좀비가 쌓인 후, 등반을 시작하기까지의 대기 시간")]
    public float climbDelay = 0.5f;

    // 내부 상태 변수
    private Rigidbody2D rb;
    private Collider2D col;
    private Animator animator;
    private const float horizontalDirection = -1f; // 항상 왼쪽으로 이동
    private float stackedTimer = 0f; // 쌓인 상태가 된 후 시간을 재는 타이머
    public int index = 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        GoToMovingState();
    }

    void FixedUpdate()
    {
        switch (currentState)
        {
            case ZombieState.Moving:
                ExecuteMoving();
                break;
            case ZombieState.Stacked:
                // 쌓여있는 동안 시간 측정
                stackedTimer += Time.fixedDeltaTime;
                break;
        }
    }

    /// <summary>
    /// '이동' 상태일 때의 행동: 앞으로 달리며 장애물/발판을 확인합니다.
    /// </summary>
    private void ExecuteMoving()
    {
        rb.velocity = new Vector2(horizontalDirection * moveSpeed, rb.velocity.y);

        Vector2 rayOrigin = (Vector2)transform.position + (Vector2.right * horizontalDirection * col.bounds.extents.x);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * horizontalDirection, checkDistance, obstacleLayer | zombieLayer);
        Debug.DrawRay(rayOrigin, Vector2.right * horizontalDirection * checkDistance, Color.red);

        if (hit.collider == null) return;

        // 1. 장애물(트럭) 감지
        if (((1 << hit.collider.gameObject.layer) & obstacleLayer) != 0)
        {
            GoToStackedState(transform.position, true);
        }
        // 2. 이미 쌓인 다른 좀비(발판) 감지
        else if (((1 << hit.collider.gameObject.layer) & zombieLayer) != 0)
        {
            // 주변의 모든 쌓인 좀비들을 검색하여 가장 높은 램프 조각을 찾습니다.
            // OverlapCircleAll의 중심을 현재 좀비의 머리 위에서부터 윗층 좀비들을 감지하도록 조정
            Vector2 overlapCenter = (Vector2)transform.position + Vector2.up * (col.bounds.extents.y + rampSegmentHeight * 0.5f); // 현재 좀비 머리 위 + 반 칸 높이
            float overlapRadius = rampSegmentHeight * 2.5f; // 2~3칸 높이까지 감지하도록 반지름 조정

            LayerMask currentZombieLayerMask = 1 << gameObject.layer;
            Collider2D[] nearbyRampPieces = Physics2D.OverlapCircleAll(overlapCenter, overlapRadius, currentZombieLayerMask);
            
            Zombie highestRampPiece = null;
            float highestY = -Mathf.Infinity;
            int attackCount = 0;
            foreach (var col in nearbyRampPieces)
            {
                Zombie rampZombie = col.GetComponent<Zombie>();
                // 현재 좀비보다 앞에 있고, 쌓인 상태이며, 가장 높은 좀비를 찾습니다.
                if (rampZombie != null && (rampZombie.currentState == ZombieState.Moving || rampZombie.currentState == ZombieState.Attacking))
                {
                    // 수평적으로도 현재 좀비보다 앞에 있어야 함
                    if (Mathf.Sign(rampZombie.transform.position.x - transform.position.x) == horizontalDirection)
                    {
                        if(rampZombie.currentState == ZombieState.Moving)
                        {
                            if(rampZombie.transform.position.y == transform.position.y)
                            {
                                highestRampPiece = rampZombie;
                                Vector3 targetPos = highestRampPiece.transform.position + new Vector3(0, rampSegmentHeight, 0);
                                StartCoroutine(ClimbSequence(targetPos));
                                return;
                            }
                        }
                        else
                        {
                            highestY = rampZombie.transform.position.y;
                            highestRampPiece = rampZombie;
                            attackCount++;
                        }
                    }
                }
            }



            if (highestRampPiece != null && attackCount == 1)
            {
                Vector3 targetPos = highestRampPiece.transform.position + new Vector3(0, rampSegmentHeight, 0);
                StartCoroutine(ClimbSequence(targetPos));
            }
        }
    }

    /// <summary>
    /// 지정된 위치로 올라가는 시퀀스
    /// </summary>
    private IEnumerator ClimbSequence(Vector3 targetPosition)
    {
        currentState = ZombieState.Climbing;
        rb.velocity = Vector2.zero;

        Vector3 startPos = transform.position;
        float journey = 0f;
        float duration = Vector3.Distance(startPos, targetPosition) / climbSpeed;
        if (duration <= 0) duration = 0.1f;
        rb.constraints &= ~RigidbodyConstraints2D.FreezePositionY;
        while (journey < duration)
        {
            journey += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPosition, journey / duration);
            yield return null;
        }
        rb.constraints |= RigidbodyConstraints2D.FreezePositionY;
        Vector2 rayOrigin = (Vector2)transform.position + (Vector2.right * horizontalDirection * col.bounds.extents.x);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * horizontalDirection, checkDistance, obstacleLayer | zombieLayer);
        Debug.DrawRay(rayOrigin, Vector2.right * horizontalDirection * checkDistance, Color.red);

        if (((1 << hit.collider.gameObject.layer) & obstacleLayer) != 0)
        {
            GoToStackedState(transform.position, true);
        }
        else
        {
            GoToStackedState(targetPosition);
        }
    }

    #region 상태 변경 함수

    private void GoToMovingState()
    {
        currentState = ZombieState.Moving;
        col.isTrigger = false; // 이제 좀비는 기본적으로 단단한 충돌체
        stackedTimer = 0f;
        transform.rotation = Quaternion.Euler(0, 180, 0);
        animator.SetBool("IsAttacking", false);
        index = PoolManager.Instance.index;
        PoolManager.Instance.index++;
        StopAllCoroutines();

        // Y축 위치에 따라 레이어 할당 (PoolManager/ZombieSpawner에서 처리하므로 여기서는 제거)
        // gameObject.layer = LayerMask.NameToLayer("Zombie_Y0_0"); // 예시
    }

    private void GoToStackedState(Vector3 finalPosition, bool isAttack = false)
    {
        transform.position = finalPosition;
        if (isAttack == true)
        {
            currentState = ZombieState.Attacking;
        }
        else
        {
            currentState = ZombieState.Moving;
        }
        animator.SetBool("IsAttacking", isAttack);
        rb.velocity = Vector2.zero;
    }

    #endregion
}
