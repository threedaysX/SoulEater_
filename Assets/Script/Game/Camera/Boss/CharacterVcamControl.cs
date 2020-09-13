using Cinemachine;

public class CharacterVcamControl : Singleton<CharacterVcamControl>
{
    // Note: may need to update current Vcam?
    public CharacterVcamData<Ifrit> ifrit;
    public CharacterVcamData<Player> player;
}

[System.Serializable]
public struct CharacterVcamData<T> where T : Character
{
    public T character;
    public CinemachineVirtualCamera vcam;
}
