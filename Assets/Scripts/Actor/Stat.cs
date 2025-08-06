using System.Collections.Generic;
using Unity.VisualScripting;

[System.Serializable]
public class Stat
{
    public float health;
    public float ad;
    public float ap;
    public float defense;
    public float mana;
    public float movespeed;

    public Stat(float health, float ad, float ap, float defense, float mana, float movespeed)
    {
        this.health = health;
        this.ad = ad;
        this.ap = ap;
        this.defense = defense;
        this.mana = mana;
        this.movespeed = movespeed;
    }

    public Stat DeepCopy()
    {
        Stat copy = new Stat(health, ad, ap, defense, mana, movespeed);
        return copy;
    }

    public static Stat DefaultStat()
    {
        Stat stat = new Stat(200, 50, 50, 5, 100, 1);
        return stat;
    }
    
    // 특정 스탯에 대해 값을 수정하는 메서드 (배수와 고정 값 리스트를 사용)
    public void ApplyStatModification(List<float> multiplyValues)
    {
        // 모든 스탯에 대해 배수와 고정 값 적용
        health = health * multiplyValues[0];
        ad = ad * multiplyValues[1];
        ap = ap * multiplyValues[2];
        mana = mana * multiplyValues[3];
        defense = defense * multiplyValues[4];
        movespeed = movespeed * multiplyValues[5];
    }
}