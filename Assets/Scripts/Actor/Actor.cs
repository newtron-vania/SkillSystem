using System.Collections.Generic;
using UnityEngine;

// [RequireComponent(typeof(Movement))]
public class Actor : MonoBehaviour
{
    public Define.ChampionName championName; // 챔피언명
    public Stat stat; // 챔피언의 기본 통계 정보 (체력, 공격력 등)

    public SkillManager SkillManager;
    [SerializeField] private List<int> skillIndices; // 각 스킬의 Index를 보유 (스킬 인덱스를 통해 SkillManager에서 스킬을 가져옴)
    public Dictionary<int, float> skillCooldowns = new();

    [SerializeField] private List<float> skillcooldownView;
    
    public Rigidbody rigid;
    public Actor target;

    public Vector3 targetPosition;

    // 상태 변수
    public bool isAirborne; // Airborne 상태
    public bool isStunned; // Stun 상태

    // 상태 지속 시간
    private float airborneDuration;
    private float stunnedDuration;

    private void Awake()
    {
        if(stat == null) stat = Stat.DefaultStat();
        rigid = GetComponentInChildren<Rigidbody>();
        SkillManager = FindObjectOfType<SkillManager>();
    }

    private void Update()
    {
        ImmobileCheck();

        UpdateCooldowns();
        
        
    }

    private void ImmobileCheck()
    {
        // 상태 지속 시간 체크
        if (isAirborne)
        {
            airborneDuration -= Time.deltaTime;
            if (airborneDuration <= 0)
            {
                isAirborne = false;
                // 공중에서 내려놓기
                Debug.Log($"{championName} is no longer airborne.");
            }
        }

        if (isStunned)
        {
            stunnedDuration -= Time.deltaTime;
            if (stunnedDuration <= 0)
            {
                isStunned = false;
                // 스턴 해제
                Debug.Log($"{championName} is no longer stunned.");
            }
        }
    }

    private void UpdateCooldowns()
    {
        int count = 0;
        foreach (int skillIndex in skillIndices)
        {
            // 쿨다운이 0 이상인 경우에만 감소
            if (skillCooldowns[skillIndex] > 0f)
            {
                skillCooldowns[skillIndex] -= Time.deltaTime;
                
                // 쿨다운이 끝났다면 0으로 설정 (스킬이 사용할 수 있도록)
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

    // Actor 초기화
    public void Initialize(Define.ChampionName champion, Stat stat, List<int> skillIndices)
    {
        championName = champion;
        this.stat = stat;
        this.skillIndices = skillIndices;
        for (int i = 0; i < skillIndices.Count; i++)
        {
            skillCooldowns.Add(skillIndices[i], 0);
            skillcooldownView.Add(0);
        } 
    }

    public void UseSkill(int skillIndex)
    {
        var skill = SkillManager.GetSkillForActorByIndex(this, skillIndex);
        if (skill != null && skillCooldowns[skill.SkillIndex] <= 0 && stat.mana >= skill.Cost)
            skill.ApplySkill(this); // 해당 스킬을 사용
        else
            Debug.LogError("Skill with index " + skillIndex + " not found for actor: " + championName);
    }

    public void ApplyAirborneEffect(float duration)
    {
        isAirborne = true;
        airborneDuration = airborneDuration <= 0 ? duration : airborneDuration + duration;
        Debug.Log($"{championName} is airborne for {duration} seconds.");
    }

    // Stun 상태 관리
    public void ApplyStunnedEffect(float duration)
    {
        isStunned = true;
        stunnedDuration = duration;
        // 여기서, Actor의 이동을 막는다.
        GetComponent<Movement>().enabled = false; // Movement 스크립트 비활성화
        Debug.Log($"{championName} is stunned for {duration} seconds.");
    }


    public List<int> GetSkillIndexs()
    {
        return skillIndices;
    }
}