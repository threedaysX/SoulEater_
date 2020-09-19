using UnityEngine;

[CreateAssetMenu(menuName = "Character/AI/Action/BasicIdle")]
public class BasicIdle : AiAction
{
    private Character character;

    public override bool StartActHaviour()
    {
        if (character == null)
            character = AI<Character>();

        // 重置移動動畫
        character.StartMoveAnim(0);
        return true;
    }
}
