using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SkillData
{
    public int skillIndex;
    public string skillName;
    public string skillType;  // JSON에서는 문자열로 되어 있으므로 string으로 받아옴
    public int cost;
    public int cooltime;
    public List<float> values;

    public Define.SkillType GetSkillType()
    {
        return SkillTypeConverter.ConvertToSkillType(skillType);  // 문자열을 SkillType으로 변환
    }
}
[Serializable]
public class SkillDatabase
{
    public List<SkillData> skills;
}

[System.Serializable]
public class ChampionSkillInfo
{
    public string championName;
    public List<int> skillIndexes;  // 각 챔피언의 스킬 인덱스
}

[System.Serializable]
public class ChampionDatabase
{
    public List<ChampionSkillInfo> champions;  // 각 챔피언의 SkillIndex 정보
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
        TextAsset skillJsonText = Resources.Load<TextAsset>($"Json/" + skillJsonFileName);
        TextAsset championJsonText = Resources.Load<TextAsset>("Json/" + championJsonFileName);

        if (skillJsonText != null && championJsonText != null)
        {
            Debug.Log("Skill data loaded successfully.");
            Debug.Log("Champion data loaded successfully.");
        
            skillDatabase = JsonUtility.FromJson<SkillDatabase>(skillJsonText.ToString());
            championDatabase = JsonUtility.FromJson<ChampionDatabase>(championJsonText.ToString());

            // 디버깅: 로드된 데이터의 첫 번째 스킬과 챔피언 출력
            if (skillDatabase.skills.Count > 0)
            {
                Debug.Log($"First skill name: {skillDatabase.skills[0].skillName}");
            }
            else
            {
                Debug.LogWarning("No skills found in the loaded data.");
            }

            if (championDatabase.champions.Count > 0)
            {
                Debug.Log($"First champion name: {championDatabase.champions[0].championName}");
            }
            else
            {
                Debug.LogWarning("No champions found in the loaded data.");
            }
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
        Debug.Log($"Checking champion: {champion.championName}");  // 디버깅: 챔피언 이름 출력
        
        if (champion.championName.Equals(name))
        {
            Debug.Log($"Found champion: {name}");  // 찾은 챔피언 이름 출력
            return champion;
        }
    }

    Debug.LogWarning($"Champion with name {name} not found.");
    return null;
}

}