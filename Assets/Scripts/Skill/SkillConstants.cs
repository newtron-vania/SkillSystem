using UnityEngine;

/// <summary>
/// SkillConstants 클래스는 스킬 시스템에서 사용되는 상수값들을 정의합니다.
/// 매직 넘버를 제거하고 코드의 가독성과 유지보수성을 향상시킵니다.
/// </summary>
public static class SkillConstants
{
    // Effect indices
    public const int RANGE_VIEW_EFFECT_INDEX = 0;
    public const int FIRST_SKILL_EFFECT_INDEX = 1;

    // Values array indices
    public const int DAMAGE_VALUE_INDEX = 0;
    public const int DAMAGE_MULTIPLIER_INDEX = 1;
    public const int RANGE_VALUE_INDEX = 2;
    public const int PROJECTILE_SPEED_INDEX = 3;

    // Layer names
    public const string GROUND_LAYER = "Ground";
    public const string ACTOR_LAYER = "Actor";

    // Input bindings
    public const int LEFT_MOUSE_BUTTON = 0;

    // Default values
    public const float MIN_RANGE = 1.0f;
    public const float MAX_RANGE = 20.0f;
    public const float DEFAULT_RAYCAST_DISTANCE = Mathf.Infinity;

    // Validation constants
    public const int MIN_EFFECT_COUNT = 1;
    public const int MAX_VALUES_COUNT = 10;
}