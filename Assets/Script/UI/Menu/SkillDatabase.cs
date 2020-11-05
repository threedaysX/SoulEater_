using UnityEngine;
using System.Linq;

public class SkillDatabase : MonoBehaviour
{
    public Player player;
    public Transform skillInventory;
    public SkillSlot[] skillSlots;
    public AttackType dbSkillType;

    private void Start()
    {
        skillSlots = skillInventory.GetComponentsInChildren<SkillSlot>();
        ResetSkillInventory();
    }

    private void Update()
    {
        if (player.isSkillFieldsModifiedTrigger)
        {
            ResetSkillInventory();
        }
    }

    public void ResetSkillInventory()
    {
        player.isSkillFieldsModifiedTrigger = false;
        Skill[] skills = player.skillFields.Where(x => x.skillType == this.dbSkillType).ToArray();
        for (int i = 0; i < skillSlots.Length; i++)
        {
            skillSlots[i].RemoveSlot();
            if (i < skills.Length)
            {
                skillSlots[i].AddSkill(skills[i].icon, skills[i]);
            }
        }
    }
}
