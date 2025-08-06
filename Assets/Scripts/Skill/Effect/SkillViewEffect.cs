using UnityEngine;

public class TargetRangeViewEffect : Effect
{
    private float _range;
    private GameObject _rangeCircle;
    public override void Initialize(params object[] objects)
    {
        if (objects[0] is float range) this._range = range;
    }

    public override bool Apply(Actor source, Actor target)
    {
        // 사용자의 위치를 가져옴
        var position = source.transform.position;

        // 반투명한 파란색 원을 그리기
        if(_rangeCircle == null) CreateRangeCircle(source, _range);

        return true; // 성공적으로 실행됨
    }

    public override void Clear()
    {
        ResourceManager.Instance.Destroy(_rangeCircle);
        TriggerManager.Instance.DeactivateTrigger(GameTrigger.SkillActivated);
    }

    // 사거리 원을 그리기 위한 메서드
    private void CreateRangeCircle(Actor source, float range)
    {
        // 원을 그릴 GameObject 생성
        GameObject rangeCircle = ResourceManager.Instance.Instantiate("RangeCircle");
        rangeCircle.transform.SetParent(source.transform); // Actor의 위치를 기준으로 설정

        // 원의 크기 및 위치 설정
        rangeCircle.transform.position = source.transform.position + Vector3.up * 0.5f;
        rangeCircle.transform.rotation = new Quaternion(90, 0, 0, 0);
        rangeCircle.transform.localScale = new Vector3(range * 2, 1, range * 2); // 반지름에 맞게 크기 조정
        _rangeCircle = rangeCircle;
    }
}


public class NoneTargetRangeViewEffect : Effect
{
    private float _range;
    private GameObject _rangeCircle;
    private GameObject _arrow;

    public override void Initialize(params object[] objects)
    {
        if (objects[0] is float range) this._range = range;
    }

    public override bool Apply(Actor source, Actor target)
    {
        // 사용자의 위치를 가져옴
        var position = source.transform.position;

        // 반투명한 파란색 원을 그리기
        if(_rangeCircle == null) CreateRangeCircle(source, _range);

        // 마우스 방향에 따른 화살표를 생성
        if(_arrow == null) CreateArrow(source, _range);

        SetArrowLine(source, _range);

        return true; // 성공적으로 실행됨
    }

    public override void Clear()
    {
        ResourceManager.Instance.Destroy(_rangeCircle);
        ResourceManager.Instance.Destroy(_arrow);
        TriggerManager.Instance.DeactivateTrigger(GameTrigger.SkillActivated);
    }

    private void CreateRangeCircle(Actor source, float range)
    {
        // 원을 그릴 GameObject 생성
        GameObject rangeCircle = ResourceManager.Instance.Instantiate("RangeCircle");
        rangeCircle.transform.SetParent(source.transform); // Actor의 위치를 기준으로 설정

        // 원의 크기 및 위치 설정
        rangeCircle.transform.position = source.transform.position + Vector3.up * 0.5f;
        rangeCircle.transform.rotation = new Quaternion(90, 0, 0, 0);
        rangeCircle.transform.localScale = new Vector3(range * 2, 1, range * 2); // 반지름에 맞게 크기 조정
        
        _rangeCircle = rangeCircle;
    }

    private void CreateArrow(Actor source, float range)
    {
        // 화살표를 그릴 GameObject 생성
        var arrow = new GameObject("Arrow");
        arrow.transform.SetParent(source.transform); // Actor의 위치를 기준으로 설정

        // LineRenderer 컴포넌트 추가
        var lineRenderer = arrow.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // 기본 재질 사용
        lineRenderer.startWidth = 1f; // 선의 시작 너비
        lineRenderer.endWidth = 1f; // 선의 끝 너비
        lineRenderer.startColor = new Color(0, 0, 1, 0.5f); // 선의 시작 색 (반투명한 파란색)
        lineRenderer.endColor = new Color(0, 0, 1, 0.5f); // 선의 끝 색 (반투명한 파란색)

        _arrow = arrow;
    }

    private void SetArrowLine(Actor source, float range)
    {
        LineRenderer lineRenderer = _arrow.GetComponent<LineRenderer>();
        
        var startPos = source.transform.position + Vector3.up * 0.5f; // Actor의 위치를 시작점으로 설정
        var mousePosition = Util.GetMouseGroundPosition(); // 마우스 위치
        mousePosition.y = 0.5f; // 3D 공간에서 y 값 고정

        // 방향 벡터 구하기
        var direction = (mousePosition - startPos).normalized;

        // 화살표 몸통 (LineRenderer) 설정
        lineRenderer.positionCount = 2; // 시작점과 끝점
        lineRenderer.SetPosition(0, startPos); // 시작점
        lineRenderer.SetPosition(1, startPos + direction * range); // 끝점 (마우스 방향으로 이동)
    }
}