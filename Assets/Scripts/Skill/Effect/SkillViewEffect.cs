using UnityEngine;

/// <summary>
/// TargetRangeViewEffect는 지정된 사거리(range)를 시각화하여 원형 범위를 표시하는 클래스입니다.
/// 주로 대상 지정형 스킬의 범위 표시용으로 사용되며, 이펙트 종료 시 해당 표시를 제거합니다.
/// </summary>
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
        var position = source.transform.position;

        if (_rangeCircle == null) CreateRangeCircle(source, _range);

        return true;
    }

    public override void Clear()
    {
        ResourceManager.Instance.Destroy(_rangeCircle);
        TriggerManager.Instance.DeactivateTrigger(GameTrigger.SkillActivated);
    }

    private void CreateRangeCircle(Actor source, float range)
    {
        GameObject rangeCircle = ResourceManager.Instance.Instantiate("RangeCircle");
        rangeCircle.transform.SetParent(source.transform);
        rangeCircle.transform.position = source.transform.position + Vector3.up * 0.5f;
        rangeCircle.transform.rotation = new Quaternion(90, 0, 0, 0);
        rangeCircle.transform.localScale = new Vector3(range * 2, 1, range * 2);
        _rangeCircle = rangeCircle;
    }
}

/// <summary>
/// NoneTargetRangeViewEffect는 지정된 범위의 원형 시각화와 함께
/// 마우스 방향을 따라 화살표 형태의 선(LineRenderer)을 생성하여 비대상 스킬의 방향성을 시각화하는 클래스입니다.
/// </summary>
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
        var position = source.transform.position;

        if (_rangeCircle == null) CreateRangeCircle(source, _range);
        if (_arrow == null) CreateArrow(source, _range);

        SetArrowLine(source, _range);

        return true;
    }

    public override void Clear()
    {
        ResourceManager.Instance.Destroy(_rangeCircle);
        ResourceManager.Instance.Destroy(_arrow);
        TriggerManager.Instance.DeactivateTrigger(GameTrigger.SkillActivated);
    }

    private void CreateRangeCircle(Actor source, float range)
    {
        GameObject rangeCircle = ResourceManager.Instance.Instantiate("RangeCircle");
        rangeCircle.transform.SetParent(source.transform);
        rangeCircle.transform.position = source.transform.position + Vector3.up * 0.5f;
        rangeCircle.transform.rotation = new Quaternion(90, 0, 0, 0);
        rangeCircle.transform.localScale = new Vector3(range * 2, 1, range * 2);
        _rangeCircle = rangeCircle;
    }

    private void CreateArrow(Actor source, float range)
    {
        var arrow = new GameObject("Arrow");
        arrow.transform.SetParent(source.transform);

        var lineRenderer = arrow.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startWidth = 1f;
        lineRenderer.endWidth = 1f;
        lineRenderer.startColor = new Color(0, 0, 1, 0.5f);
        lineRenderer.endColor = new Color(0, 0, 1, 0.5f);

        _arrow = arrow;
    }

    private void SetArrowLine(Actor source, float range)
    {
        LineRenderer lineRenderer = _arrow.GetComponent<LineRenderer>();

        var startPos = source.transform.position + Vector3.up * 0.5f;
        var mousePosition = Util.GetMouseGroundPosition();
        mousePosition.y = 0.5f;

        var direction = (mousePosition - startPos).normalized;

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, startPos + direction * range);
    }
}