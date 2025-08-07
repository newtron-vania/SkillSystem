using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Actor 클래스는 게임 내 캐릭터(챔피언)를 나타냅니다.
/// 각 Actor는 고유의 스탯, 스킬, 상태이상(에어본, 스턴) 등을 관리하며,
/// 스킬 사용, 쿨다운 갱신, 상태 이상 처리 등을 담당합니다.
/// </summary>
public class Actor : MonoBehaviour
{
    public Define.ChampionName championName; // 챔피언 이름 (열거형으로 정의된 이름 사용)
    public Stat stat; // 챔피언의 기본 능력치 (체력, 공격력, 마나 등)

    public SkillManager SkillManager; // 스킬 매니저 참조 (스킬 적용 및 조회에 사용)

    [SerializeField] private List<int> skillIndices; // 해당 Actor가 보유한 스킬의 인덱스 목록
    public Dictionary<int, float> skillCooldowns = new(); // 스킬 인덱스별 남은 쿨다운 시간

    [SerializeField] private List<float> skillcooldownView; // 쿨다운 UI 표시용 뷰 리스트

    public Rigidbody rigid; // 캐릭터 물리 적용용 Rigidbody
    public Actor target; // 현재 타겟팅 중인 Actor
    public Vector3 targetPosition; // 목표 지점 위치

    // 상태 이상 여부
    public bool isAirborne; // 에어본 상태 여부
    public bool isStunned;  // 스턴 상태 여부

    // 상태 이상 지속 시간
    private float airborneDuration; // 에어본 남은 시간
    private float stunnedDuration;  // 스턴 남은 시간

    /// <summary>
    /// 컴포넌트 초기화. Stat이 비어있다면 기본값 설정. Rigidbody 및 SkillManager 초기화.
    /// </summary>
    private void Awake()
    {
        if(stat == null) stat = Stat.DefaultStat();
        rigid = GetComponentInChildren<Rigidbody>();
        SkillManager = FindObjectOfType<SkillManager>();
    }

    /// <summary>
    /// 매 프레임마다 상태 이상 체크 및 스킬 쿨다운 갱신을 수행.
    /// </summary>
    private void Update()
    {
        ImmobileCheck();
        UpdateCooldowns();
    }

    /// <summary>
    /// 에어본 및 스턴 상태의 지속 시간을 감소시키고, 시간이 종료되면 해제 처리.
    /// </summary>
    private void ImmobileCheck()
    {
        if (isAirborne)
        {
            airborneDuration -= Time.deltaTime;
            if (airborneDuration <= 0)
            {
                isAirborne = false;
                Debug.Log($"{championName} is no longer airborne.");
            }
        }

        if (isStunned)
        {
            stunnedDuration -= Time.deltaTime;
            if (stunnedDuration <= 0)
            {
                isStunned = false;
                Debug.Log($"{championName} is no longer stunned.");
            }
        }
    }

    /// <summary>
    /// 각 스킬의 쿨다운 시간을 감소시키고, 쿨다운이 끝났을 경우 0으로 설정.
    /// UI 표시를 위한 뷰 리스트도 함께 갱신.
    /// </summary>
    private void UpdateCooldowns()
    {
        int count = 0;
        foreach (int skillIndex in skillIndices)
        {
            if (skillCooldowns[skillIndex] > 0f)
            {
                skillCooldowns[skillIndex] -= Time.deltaTime;
                if (skillCooldowns[skillIndex] <= 0f)
                {
                    skillCooldowns[skillIndex] = 0f;
                    Debug.Log($"{skillIndex} is ready to use!");
                }
            }
            skillcooldownView[count] = skillCooldowns[skillIndex];
            count++;
        }
    }

    /// <summary>
    /// Actor의 기본 정보와 스킬 목록을 초기화.
    /// </summary>
    /// <param name="champion">챔피언 이름</param>
    /// <param name="stat">스탯 정보</param>
    /// <param name="skillIndices">보유 스킬 인덱스 리스트</param>
    public void Initialize(Define.ChampionName champion, Stat stat, List<int> skillIndices)
    {
        championName = champion;
        this.stat = stat;
        this.skillIndices = skillIndices;
        for (int i = 0; i < skillIndices.Count; i++)
        {
            skillCooldowns.Add(skillIndices[i], 0); // 초기 쿨다운은 0
            skillcooldownView.Add(0);               // 뷰도 초기화
        } 
    }

    /// <summary>
    /// 지정된 인덱스의 스킬을 사용합니다. 쿨다운이 없고 마나가 충분할 경우에만 사용 가능.
    /// </summary>
    /// <param name="skillIndex">사용할 스킬의 인덱스</param>
    public void UseSkill(int skillIndex)
    {
        var skill = SkillManager.GetSkillForActorByIndex(this, skillIndex);
        if (skill != null && skillCooldowns[skill.SkillIndex] <= 0 && stat.mana >= skill.Cost)
        {
            skill.ApplySkill(this);
        }
        else
        {
            Debug.LogError("Skill with index " + skillIndex + " not found for actor: " + championName);
        }
    }

    /// <summary>
    /// 에어본 효과를 적용. 기존 에어본 상태와 누적 가능.
    /// </summary>
    /// <param name="duration">지속 시간 (초)</param>
    public void ApplyAirborneEffect(float duration)
    {
        isAirborne = true;
        airborneDuration = airborneDuration <= 0 ? duration : airborneDuration + duration;
        Debug.Log($"{championName} is airborne for {duration} seconds.");
    }

    /// <summary>
    /// 스턴 효과를 적용. 이동을 막기 위해 Movement 스크립트 비활성화.
    /// </summary>
    /// <param name="duration">지속 시간 (초)</param>
    public void ApplyStunnedEffect(float duration)
    {
        isStunned = true;
        stunnedDuration = duration;
        GetComponent<Movement>().enabled = false;
        Debug.Log($"{championName} is stunned for {duration} seconds.");
    }

    /// <summary>
    /// 보유한 스킬 인덱스 리스트를 반환.
    /// </summary>
    /// <returns>스킬 인덱스 리스트</returns>
    public List<int> GetSkillIndexs()
    {
        return skillIndices;
    }
}
