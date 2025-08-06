using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Actor actor;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private SkillManager skillManager;
    [SerializeField] private SkillDataLoader skillDataLoader;

    private void Start()
    {
        GameManager.Instance.Player = actor;
        skillDataLoader = GetComponent<SkillDataLoader>();
        // 예시로 블리츠크랭크로 초기화
        var stat = new Stat(1000f, 80f, 200, 10f, 100f, 5f); // 예시 스탯 (체력, 공격력 등)

        var championSkillInfo = skillDataLoader.GetChampionSkillInfo("Blitzcrank");
        // 스킬 인덱스를 설정 (1번, 2번, 3번 스킬 사용)
        var skillIndices = championSkillInfo.skillIndexes;

        skillManager.InitializeActorSkills(actor, skillDataLoader.skillDatabase);

        actor.Initialize(Define.ChampionName.Blitzcrank, stat, skillIndices);

        movement = actor.GetComponent<PlayerMovement>();
        KeyBindings();
    }

    private void Update()
    {
        movement.moveSpeed = actor.stat.movespeed;
    }

    private void KeyBindings()
    {
        InputManager.Instance.AddKeyBinding(KeyCode.Q, () => UseActorSkill(actor.GetSkillIndexs()[0]));
        InputManager.Instance.AddKeyBinding(KeyCode.W, () => UseActorSkill(actor.GetSkillIndexs()[1]));
        InputManager.Instance.AddKeyBinding(KeyCode.E, () => UseActorSkill(actor.GetSkillIndexs()[2]));
        InputManager.Instance.AddMouseBinding(1, SetTargetPositionByClick);
        // InputManager.Instance.AddKeyBinding(KeyCode.P, () => OpenCloseEditor());
    }

    private void OpenCloseEditor()
    {
        throw new NotImplementedException();
    }

    private void UseActorSkill(int index)
    {
        if (!TriggerManager.Instance.IsTriggerActive(GameTrigger.SkillActivated)) actor.UseSkill(index);
    }

    private void SetTargetPositionByClick()
    {
        if (TriggerManager.Instance.IsTriggerActive(GameTrigger.SkillActivated) || actor.isStunned) return;
        // 화면에서 클릭한 위치를 월드 좌표로 변환
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // "Ground" 레이어만 레이캐스트가 충돌하도록 LayerMask 사용
        var groundLayer = LayerMask.GetMask("Ground");
        Debug.DrawRay(Camera.main.transform.position, ray.direction * 10, Color.red,
            1f); // 길이를 10으로 설정하여 시각적으로 더 잘 보이도록
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer)) // "Ground" 레이어에 대해서만 충돌
        {
            var targetPosition = new Vector3(hit.point.x, actor.transform.position.y, hit.point.z);
            movement.targetPosition = targetPosition;
            Debug.Log($"TargetPosition :  {targetPosition}");
        }
    }
}