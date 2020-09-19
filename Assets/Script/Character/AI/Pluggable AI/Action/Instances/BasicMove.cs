using UnityEngine;

[CreateAssetMenu(menuName = "Character/AI/Action/BasicMove")]
public class BasicMove : AiAction
{
    public float basicMoveSpeed = 2.5f;

    private Character character;

    public override bool StartActHaviour()
    {
        return Move();
    }

    private bool Move()
    {
        if (character == null)
            character = AI<Character>();

        if (!character.move.CanDo)
            return false;

        Vector3 chaseDirection = Vector3.Normalize(Ai.ChaseTarget.position - Ai.transform.position);
        chaseDirection.y = 0f;
        chaseDirection.z = 0f;
        Ai.transform.position += chaseDirection * basicMoveSpeed * character.data.moveSpeed.Value * Time.deltaTime;
        character.StartMoveAnim(Ai.transform.right.x);
        return true;
    }
}
