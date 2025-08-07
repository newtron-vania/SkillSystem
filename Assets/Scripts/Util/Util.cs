using UnityEngine;

public static class Util
{
    // Util: 마우스 위치를 기반으로 지면(Ground)의 월드 좌표를 반환합니다.
    public static Vector3 GetMouseGroundPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        int groundLayer = LayerMask.GetMask("Ground");

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            return hit.point;
        }

        // 기본 위치로 fallback (예: source 위치)
        return Vector3.zero;
    }
}