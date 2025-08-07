using UnityEngine;

public class EnemyMovement : Movement
{
    public Vector3 direction = Vector3.forward; // 기본 이동 방향 (일직선: Z축 방향)

    private void Start()
    {
        // 적은 항상 일정한 방향으로 이동하도록 설정
        // 예시로 Z축 방향 (Vector3.forward)으로 설정
        direction = Vector3.forward; // 일직선으로 이동하도록 설정
    }


    // 일직선으로 1씩 이동
    protected override void Move()
    {
        // Move() 메서드를 오버라이드하여 적 캐릭터가 일직선으로 이동하도록 설정
        transform.Translate(1f * Time.deltaTime * direction); // 1씩 이동
    }
}