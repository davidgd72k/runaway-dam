using UnityEngine;

public class PlayerAbilities : MonoBehaviour
{
    public static PlayerAbilities instance;

    /// <summary>
    /// Desbloqueo del arból de habilidades.
    /// </summary>
    public bool unlockedSkillTree = false;
    /// <summary>
    /// Desbloqueo del dash.
    /// </summary>
    public bool unlockedDash = false;
    /// <summary>
    /// Desbloqueo del doble salto.
    /// </summary>
    public bool unlockedDoubleJump = false;
    public int extraJumps = 1;
    /// <summary>
    /// Cantidad de vidas extras compradas.
    /// </summary>
    public int extraLifes = 0;
    /// <summary>
    /// Mejora del cooldown del dash.
    /// </summary>
    public float improveCooldownDash = 1f;
    
    #region Class singleton.
    private void Awake()
    {
        // Si no existe instancia, esta se convierte en la principal
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // no se destruye al cambiar de escena
        }
        else
        {
            Destroy(gameObject); // evita duplicados
        }
    }
    #endregion

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
