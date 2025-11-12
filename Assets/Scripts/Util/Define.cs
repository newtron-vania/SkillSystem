public class Define
{
// SkillType: 스킬의 시전 방식
    public enum SkillType
    {
        None,
        Instant,
        Target,
        NoneTarget
    }

// ChampionName: 챔피언의 이름
    public enum ChampionName
    {
        None,
        Blitzcrank,
        Sona
    }

// GameTrigger: 게임 내 트리거 이벤트 타입
    public enum GameTrigger
    {
        None,
        SkillActivated,
        SkillCompleted,
        DamageDealt,
        TargetAcquired,
        ProjectileHit,
        EffectApplied
    }
}