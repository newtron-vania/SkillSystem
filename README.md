# Unity Flexible Skill System

> **3일 간 개발한, 유연하고 확장 가능한 Unity 스킬 시스템**

본 프로젝트는 작은 모듈 단위의 `Effect`를 조합하여 다양한 형태의 스킬을 구성할 수 있도록 설계되었습니다.  
각 효과는 독립적으로 구현되어 있어 재사용성과 유지보수성이 뛰어나며, 다양한 스킬을 손쉽게 확장하거나 수정할 수 있습니다.
해당 프로젝트는 테스트를 위해 블리츠크랭크의 로켓 손이 구현되어있습니다.

---

## 📌 설계 목표

- **모듈화된 스킬 구성**  
  스킬은 `Effect` 단위로 쪼개어 설계되어, 여러 개의 이펙트를 순차적으로 실행하여 복합 스킬을 구성할 수 있습니다.

- **입력 방식 분리**  
  InputSystem을 통해 동적인 키 바인딩 시스템 구현

- **스킬 실행 조건 구분**  
  "Target, "NoneTarget", "Instant" 방식으로 실행 조건을 구분하여 다양한 스킬들의 기본 사용 조건을 파생 클래스로 구분하여 동적인 스킬 구현에 도움을 줍니다.

- **높은 유연성과 재사용성**  
  동일한 `Effect`를 여러 스킬에서 활용할 수 있으며, 스킬 구성은 단순한 조립 방식으로 이루어집니다.

---

## 🔧 핵심 클래스 설명

### `Effect`
- 스킬의 최소 실행 단위입니다.
- `Apply`, `Initialize`, `Clear`의 3단계로 구성됩니다.
- 예: `DamageEffect`, `CooldownEffect`, `GrabEffect`, `WaitForCheckEffect` 등

### `Skill`
- 여러 `Effect`들을 조합한 실행 단위입니다.
- 스킬 시전 방식에 따라 `InstantSkill`, `TargetSkill`, `NoneTargetSkill`로 분리됩니다.
- 스킬은 순차적으로 Effect를 실행하며, 실행 중인 상태를 `SkillInstance`로 관리합니다.

### `Actor`
- 스킬을 사용할 수 있는 대상입니다.
- `Stat`, `SkillManager`, 상태 이상(스턴, 에어본) 등을 포함합니다.
- Actor는 캐릭터의 고유 정보만을 보유하며, 이에 대한 추가 행동 등은 외부 컴포넌트를 통하여 실행됩니다.

### `SkillManager`
- Actor가 보유한 스킬들을 관리하고, 현재 활성화된 스킬을 프레임 단위로 실행합니다.

---

## 동작 과정 설명

### ▶ 기본 조작

- **우클릭 (Mouse Right Click)**  
  → 캐릭터를 마우스 방향으로 이동시킵니다.  
  → `Movement` 컴포넌트를 통해 처리되며, 스턴/에어본 상태에서는 이동이 제한됩니다.

---

### ▶ 스킬 사용: `RocketGrabSkill` (`Q` 키)

1. **`Q` 키 입력 → 스킬 준비 상태 진입**
   - `RocketGrabSkill`은 `NoneTargetSkill`로 분류됩니다.
   - 입력 시 `NoneTargetRangeViewEffect`가 실행되어:
     - 마우스 방향으로 **범위 원**과 **화살표**가 시각적으로 표시됩니다.
   - `TriggerManager`를 통해 `SkillActivated` 트리거가 활성화됩니다.

2. **마우스 좌클릭 → 시전 방향 지정**
   - 마우스 클릭 위치를 기준으로 **목표 방향**을 계산합니다.
   - 해당 방향으로 **투사체(Projectile)**가 발사됩니다.
   - `ShootProjectileEffect`가 실행되어 투사체가 날아갑니다.

3. **투사체 명중 시 → 효과 발동**
   - 적 `Actor`와 충돌 시 다음 이펙트들이 순차적으로 실행됩니다:
     - `DamageEffect`: 피해를 입힘
     - `GrabEffect`: 적을 시전자 방향으로 끌어당김
     - `WaitForCheckEffect`: 충돌 및 이동이 완료될 때까지 대기
   - 모든 이펙트가 완료되면 `SkillInstance`가 종료되고,
     `TriggerManager`는 트리거를 비활성화합니다.


---

### 🔄 기타 시스템 동작

- 모든 스킬은 `Effect` 리스트로 구성되어 순차적으로 실행됩니다.
- `SkillManager`는 활성화된 스킬 인스턴스(`SkillInstance`)를 매 프레임마다 `Tick()`으로 갱신합니다.
- `Skill.IsRunning` 플래그를 통해 스킬의 실행 완료 여부를 판단합니다.
- 스킬 시전 조건은 `SkillType`에 따라 분기됩니다:
  - `InstantSkill` → 즉시 실행
  - `TargetSkill` → 대상 클릭
  - `NoneTargetSkill` → 방향 클릭



# 테스트 영상
---
https://github.com/user-attachments/assets/cac3bb06-32cd-4e5b-b82b-69477d148e43

