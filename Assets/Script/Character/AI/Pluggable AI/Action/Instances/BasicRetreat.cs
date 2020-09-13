using UnityEngine;

[CreateAssetMenu(menuName = "Character/AI/Action/BasicRetreat")]
public class BasicRetreat : AiAction
{
    private Character character;

    public override bool StartActHaviour()
    {
        return Retreat();
    }

    private bool Retreat()
    {
        if (character == null)
            character = AI<Character>();

        Vector3 retreatDirection = Vector3.Normalize(-(Ai.ChaseTarget.position - Ai.transform.position));
        Ai.transform.position += retreatDirection * character.data.moveSpeed.Value * Time.deltaTime;
        Debug.Log(character.characterName + "要撤退摟！！");
        return true;
    }
}
