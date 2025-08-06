using UnityEngine;

public class PlayerMovement : Movement
{
    public Vector3 targetPosition;  // 목표 위치 (타겟 위치)

    protected void FixedUpdate()
    {
        base.FixedUpdate();
    }

    protected override void Move()
    {
        // 목표 위치의 X, Y 값을 목표로 이동
        float step = moveSpeed * Time.deltaTime;  // 이동 속도에 따라 이동할 거리 계산

        // X, Y 축만 이동하고 Z 축은 고정
        Vector3 targetPositionWithFixedZ = new Vector3(targetPosition.x, transform.position.y, targetPosition.z);

        // 타겟 위치로 이동
        transform.position = Vector3.MoveTowards(transform.position, targetPositionWithFixedZ, step);
    }
}