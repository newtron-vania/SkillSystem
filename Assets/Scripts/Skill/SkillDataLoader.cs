using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     SkillData는 단일 스킬의 정보(이름, 타입, 코스트, 쿨타임, 수치값 등)를 저장하는 클래스입니다.
///     JSON 파일로부터 직렬화되어 로드되며, skillType은 문자열이므로 변환 메서드를 통해 enum으로 변환합니다.
/// </summary>
[Serializable]
public class SkillData
{
    public int skillIndex;
    public string skillName;
    public string skillType; // JSON에서는 문자열로 되어 있으므로 string으로 받아옴
    public int cost;
    public int cooltime;
    public List<float> values;

    public Define.SkillType GetSkillType()
    {
        return SkillTypeConverter.ConvertToSkillType(skillType); // 문자열을 SkillType으로 변환
    }
}

/// <summary>
///     SkillDatabase는 모든 스킬 정보를 담고 있는 데이터베이스 클래스입니다.
///     JSON 파일에서 전체 스킬 리스트를 로드할 때 사용됩니다.
/// </summary>
[Serializable]
public class SkillDatabase
{
    public List<SkillData> skills;
}

/// <summary>
///     ChampionSkillInfo는 하나의 챔피언이 보유한 스킬 인덱스 정보를 저장하는 클래스입니다.
///     각 챔피언은 이름과 스킬 인덱스 리스트를 가집니다.
/// </summary>
[Serializable]
public class ChampionSkillInfo
{
    public string championName;
    public List<int> skillIndexes; // 각 챔피언의 스킬 인덱스
}

/// <summary>
///     ChampionDatabase는 모든 챔피언들의 스킬 인덱스 정보를 담은 클래스입니다.
///     JSON 파일로부터 직렬화되어 로드됩니다.
/// </summary>
[Serializable]
public class ChampionDatabase
{
    public List<ChampionSkillInfo> champions; // 각 챔피언의 SkillIndex 정보
}

public static class SkillTypeConverter
{
    public static Define.SkillType ConvertToSkillType(string skillType)
    {
        switch (skillType)
        {
            case "NoneTarget":
                return Define.SkillType.NoneTarget;
            case "Instant":
                return Define.SkillType.Instant;
            case "Target":
                return Define.SkillType.Target;
            default:
                throw new ArgumentException("Invalid skill type: " + skillType);
        }
    }
}

/// <summary>
///     SkillDataLoader는 게임 실행 시 Resources 폴더 내의 JSON 파일을 로드하여
///     스킬 데이터(SkillDatabase)와 챔피언 데이터(ChampionDatabase)를 불러오는 역할을 합니다.
/// </summary>
public class SkillDataLoader : MonoBehaviour
{
    public string skillJsonFileName = "SkillInfo"; // 'Resources/Json/SkillInfo.json' 경로로 가정
    public string championJsonFileName = "ChampionInfo"; // 'Resources/Json/ChampionInfo.json' 경로로 가정

    public SkillDatabase skillDatabase;
    public ChampionDatabase championDatabase;

    private void Awake()
    {
        LoadSkillData();
    }

    // JSON 데이터 로드 함수
    public void LoadSkillData()
    {
        var skillJsonText = Resources.Load<TextAsset>("Json/" + skillJsonFileName);
        var championJsonText = Resources.Load<TextAsset>("Json/" + championJsonFileName);

        if (skillJsonText != null && championJsonText != null)
        {
            Debug.Log("Skill data loaded successfully.");
            Debug.Log("Champion data loaded successfully.");

            skillDatabase = JsonUtility.FromJson<SkillDatabase>(skillJsonText.ToString());
            championDatabase = JsonUtility.FromJson<ChampionDatabase>(championJsonText.ToString());

            // 디버깅: 로드된 데이터의 첫 번째 스킬과 챔피언 출력
            if (skillDatabase.skills.Count > 0)
                Debug.Log($"First skill name: {skillDatabase.skills[0].skillName}");
            else
                Debug.LogWarning("No skills found in the loaded data.");

            if (championDatabase.champions.Count > 0)
                Debug.Log($"First champion name: {championDatabase.champions[0].championName}");
            else
                Debug.LogWarning("No champions found in the loaded data.");
        }
        else
        {
            Debug.LogError("Failed to load skill or champion data!");
        }
    }

    public ChampionSkillInfo GetChampionSkillInfo(string name)
    {
        foreach (var champion in championDatabase.champions)
        {
            Debug.Log($"Checking champion: {champion.championName}"); // 디버깅: 챔피언 이름 출력

            if (champion.championName.Equals(name))
            {
                Debug.Log($"Found champion: {name}"); // 찾은 챔피언 이름 출력
                return champion;
            }
        }

        Debug.LogWarning($"Champion with name {name} not found.");
        return null;
    }
}