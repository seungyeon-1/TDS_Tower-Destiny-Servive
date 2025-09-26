using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Following Target")]
    public Transform target; // 카메라가 따라다닐 대상 (타워)
    public Vector3 offset; // 타워로부터 떨어져 있을 거리 (Z값은 카메라의 고정된 Z위치로 사용)

    [Header("Stop Condition")]
    public Transform stopTrigger; // 카메라 추적을 멈출 경계선 위치

    void LateUpdate()
    {
        if (target == null) return;

        // 멈춤 조건 확인
        if (stopTrigger != null)
        {
            bool stopConditionMet = transform.position.x >= stopTrigger.position.x;
            if (stopConditionMet)
            {
                this.enabled = false; // 조건 충족 시 스크립트 비활성화
                return;
            }
        }

        // 목표 위치 계산 (타겟의 X, Y 위치 + 오프셋)
        Vector3 desiredPosition = new Vector3(target.position.x + offset.x, offset.y, offset.z);

        // 카메라 위치 업데이트
        transform.position = desiredPosition;
    }
}