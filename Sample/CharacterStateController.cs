
public abstract class CharacterStateControllerBase : Monobehaviour
{
    [SerializeField]
    private SpriteRenderer _spriteRenderer;

    public void ChangeCharacterState(StateInt state)
    {
        Sprite stateAnimation = GetSprite(state);
        _spriteRenderer.sprite = stateAnimation;
    }
    protected abstract Sprite GetSprite(StateInt state);
}

pulic class CharacterStateController : CharacterStateControllerBase
{
    //実際はSerializedDictionaryなどを使ってください。
    //EnumをKeyにしたときのブロック化の問題は今回は割愛します。
    [SerializedField]
    private Dictionary<CharacterState,Sprite> _stateStandPictures;
    
    protected override Sprite GetSprite(StateInt state)
    {
        
    }
}