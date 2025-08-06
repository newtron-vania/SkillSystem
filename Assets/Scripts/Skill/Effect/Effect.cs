public abstract class Effect
{
    /// <summary>
    /// 효과에 필요한 초기 값을 설정하는 메서드입니다.
    /// 파라미터로 전달된 객체들을 내부 변수에 저장하거나 초기화할 때 사용합니다.
    /// 예: 지속 시간, 데미지 배수, 스킬 인덱스 등.
    /// </summary>
    public abstract void Initialize(params object[] objects);
    
    /// <summary>
    /// 실제 효과를 적용하는 메서드입니다.
    /// source(시전자)와 target(피대상) 정보를 기반으로 효과를 실행하며,
    /// 성공적으로 적용되면 true를 반환합니다.
    /// 일부 효과는 조건에 따라 false를 반환해 다음 효과 적용을 대기시킬 수 있습니다.
    /// </summary>
    public abstract bool Apply(Actor source, Actor target);

    /// <summary>
    /// 효과 적용 후 정리 작업을 수행하는 메서드입니다.
    /// 예: 생성한 오브젝트 제거, 상태 초기화, 트리거 비활성화 등.
    /// 지속성 효과나 UI 시각화 후 제거가 필요한 경우 반드시 호출됩니다.
    /// </summary>
    public abstract void Clear();
}