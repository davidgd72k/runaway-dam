using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class skiltree : MonoBehaviour
{
    // Singleton
    public static skiltree instance;

    // Lazy instance accessor: si no existe, intenta encontrarlo (incluso inactivo)
    // o lo crea dinámicamente. Esto permite que otros scripts lo usen aunque
    // el GameObject en la escena esté desactivado al inicio.
    public static skiltree Instance
    {
        get
        {
            if (instance != null) return instance;

            // Buscar incluso objetos inactivos en la escena
            var found = Resources.FindObjectsOfTypeAll<skiltree>();
            if (found != null && found.Length > 0)
            {
                instance = found[0];
                // Si está inactivo, activarlo para que Awake/Start se ejecuten
                if (!instance.gameObject.activeInHierarchy)
                    instance.gameObject.SetActive(true);
                return instance;
            }

            // Si no existe, crear uno nuevo y marcar para no destruir al cargar escenas
            var go = new GameObject("skiltree");
            instance = go.AddComponent<skiltree>();
            DontDestroyOnLoad(go);
            return instance;
        }
    }

    // Forzar inicialización cuando se accede a cualquier miembro estático por primera vez
    // Static constructor removed: avoid calling Resources.FindObjectsOfTypeAll during type initialization

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    [Header("Skill Data")]
    public int[] skilllevel;          // nivel actual por skill
    public int[] skillcap;            // nivel máximo por skill
    public TMP_Text[] skillname;        // nombre
    public TMP_Text[] skilldescription; // descripción

    // Textos base inmutables usados para componer la UI sin duplicar
    public string[] baseSkillNames;
    public string[] baseSkillDescriptions;

    [Header("Runtime References")]
    public List<Skill> skillList;
    public GameObject skillholder;

    public List<GameObject> connectotrlist;
    public GameObject connectorholder;


    [Header("Player Data")]
    public double SkillPoints; // puntos globales del jugador

    [Header("Debug / Test")]
    [SerializeField] private bool forceZeroLevels = true; // activar para pruebas: forzar todos los niveles a 0

    private bool initialized = false;

    private void Start()
    {
        Initialize();
    }

    // Inicialización idempotente que puede llamarse desde fuera aunque el GameObject
    // original esté desactivado. Se encarga de poblar listas, asignar IDs y estados.
    public void Initialize()
    {
        if (initialized) return;
        initialized = true;

        // Inicializa colecciones en tiempo de ejecución
        skillList = new List<Skill>();
        if (connectotrlist == null) connectotrlist = new List<GameObject>();

        // Valores por defecto para límites y niveles (se ajustarán según el número de skills)
        skillcap = new[] { 1, 5, 5, 5 };
        skilllevel = new[] { 0, 0, 0, 0 };

        // Recoger también objetos inactivos para que la lista se pueble aunque la UI esté oculta
        if (skillholder != null)
        {
            foreach (var skill in skillholder.GetComponentsInChildren<Skill>(true))
            {
                skillList.Add(skill);
            }
        }

        // Asegurar que los arrays de textos existen y tienen exactamente el tamaño de skillList
        var count = skillList != null ? skillList.Count : 0;
        if (count > 0)
        {
            // Resize skillname to exactly count, preserving any existing entries
            if (skillname == null || skillname.Length != count)
            {
                var newNames = new TMP_Text[count];
                if (skillname != null)
                {
                    for (int i = 0; i < Mathf.Min(skillname.Length, newNames.Length); i++)
                        newNames[i] = skillname[i];
                }
                for (int i = 0; i < count; i++)
                {
                    if (newNames[i] == null && skillList[i] != null && skillList[i].skillName != null)
                        newNames[i] = skillList[i].skillName;
                }
                skillname = newNames;
            }

            // Resize skilldescription similarly
            if (skilldescription == null || skilldescription.Length != count)
            {
                var newDescs = new TMP_Text[count];
                if (skilldescription != null)
                {
                    for (int i = 0; i < Mathf.Min(skilldescription.Length, newDescs.Length); i++)
                        newDescs[i] = skilldescription[i];
                }
                for (int i = 0; i < count; i++)
                {
                    if (newDescs[i] == null && skillList[i] != null && skillList[i].skillDescription != null)
                        newDescs[i] = skillList[i].skillDescription;
                }
                skilldescription = newDescs;
            }

            // Defaults para nombres/descr (solo usados si no hay texto disponible en TMP o Skill)
            var defaultNames = new[] { "Raíz1", "Velocidad2", "Escudo3", "Doble salto4", "Sprint5" };
            var defaultDescriptions = new[]
            {
                "Desbloquea las habilidades 1",
                "Aumenta la velocidad de movimiento 2",
                "Desbloquea escudo 3",
                "Desbloquea doble salto 4",
                "descloquea sprint 5"
            };

            // Aplicar textos por defecto a los TMP_Text (solo si el TMP existe)
            for (int i = 0; i < count; i++)
            {
                if (skillname != null && i < skillname.Length && skillname[i] != null)
                {
                    // Si el TMP está vacío, aplicar default
                    if (string.IsNullOrEmpty(skillname[i].text) && i < defaultNames.Length)
                        skillname[i].SetText(defaultNames[i]);
                }

                if (skilldescription != null && i < skilldescription.Length && skilldescription[i] != null)
                {
                    if (string.IsNullOrEmpty(skilldescription[i].text) && i < defaultDescriptions.Length)
                        skilldescription[i].SetText(defaultDescriptions[i]);
                }
            }

            // Inicializar arrays base para nombres y descripciones con el mismo tamaño
            if (baseSkillNames == null || baseSkillNames.Length != count) baseSkillNames = new string[count];
            if (baseSkillDescriptions == null || baseSkillDescriptions.Length != count) baseSkillDescriptions = new string[count];

            for (int i = 0; i < count; i++)
            {
                // Nombre base: preferir skillname (TMP_Text), luego campo del Skill, luego default
                string nameBase = string.Empty;
                if (skillname != null && i < skillname.Length && skillname[i] != null && !string.IsNullOrEmpty(skillname[i].text))
                    nameBase = skillname[i].text;
                else if (skillList[i] != null && skillList[i].skillName != null && !string.IsNullOrEmpty(skillList[i].skillName.text))
                    nameBase = skillList[i].skillName.text;
                else if (i < defaultNames.Length)
                    nameBase = defaultNames[i];

                // Sanear sufijo "(x/y)" si existiera
                if (!string.IsNullOrEmpty(nameBase))
                {
                    var idx = nameBase.LastIndexOf('(');
                    if (idx >= 0)
                    {
                        var suffix = nameBase.Substring(idx);
                        if (suffix.Contains("/")) nameBase = nameBase.Substring(0, idx).TrimEnd();
                    }
                }

                baseSkillNames[i] = nameBase ?? string.Empty;

                // Descripción base: misma jerarquía
                string descBase = string.Empty;
                if (skilldescription != null && i < skilldescription.Length && skilldescription[i] != null && !string.IsNullOrEmpty(skilldescription[i].text))
                    descBase = skilldescription[i].text;
                else if (skillList[i] != null && skillList[i].skillDescription != null && !string.IsNullOrEmpty(skillList[i].skillDescription.text))
                    descBase = skillList[i].skillDescription.text;
                else if (i < defaultDescriptions.Length)
                    descBase = defaultDescriptions[i];

                // Eliminar bloque previo de "\n\nSkill Points:" si existe
                if (!string.IsNullOrEmpty(descBase))
                {
                    var marker = "\n\nSkill Points:";
                    var midx = descBase.IndexOf(marker);
                    if (midx >= 0) descBase = descBase.Substring(0, midx);
                }

                baseSkillDescriptions[i] = descBase?.TrimEnd() ?? string.Empty;
            }
        }
        else
        {
            // Asegurarse de no dejar nulls
            skillname = skillname ?? new TMP_Text[0];
            skilldescription = skilldescription ?? new TMP_Text[0];
            baseSkillNames = baseSkillNames ?? new string[0];
            baseSkillDescriptions = baseSkillDescriptions ?? new string[0];
        }

        // Asegura que los arrays de nivel y cap tengan al menos el tamaño de skillList
        if (skilllevel == null || skilllevel.Length < skillList.Count)
        {
            var newLevels = new int[skillList.Count];
            if (skilllevel != null)
                for (int i = 0; i < skilllevel.Length && i < newLevels.Length; i++) newLevels[i] = skilllevel[i];
            skilllevel = newLevels;
        }

        if (skillcap == null || skillcap.Length < skillList.Count)
        {
            var newCaps = new int[skillList.Count];
            // Copia valores existentes y rellena el resto con 0 por defecto
            if (skillcap != null)
                for (int i = 0; i < skillcap.Length && i < newCaps.Length; i++) newCaps[i] = skillcap[i];
            for (int i = 0; i < newCaps.Length; i++) if (newCaps[i] == 0) newCaps[i] = 1;
            skillcap = newCaps;
        }

        // Cargar niveles guardados si existen; si no, usar 0 por defecto.
        // Si forceZeroLevels está activado (útil para pruebas), forzamos 0.
        for (int i = 0; i < skilllevel.Length; i++)
        {
            var key = $"skilllevel_{i}";
            if (!forceZeroLevels && PlayerPrefs.HasKey(key))
                skilllevel[i] = PlayerPrefs.GetInt(key);
            else
                skilllevel[i] = 0;
        }

        // Asigna skillID a cada Skill encontrado para evitar valores residuales en el Inspector
        for (var i = 0; i < skillList.Count; i++)
        {
            if (skillList[i] != null) skillList[i].skillID = i;
        }

        // Rellena la lista de conectores (comprobar null)
        if (connectorholder != null)
        {
            foreach (var connector in connectorholder.GetComponentsInChildren<RectTransform>(true))
            {
                connectotrlist.Add(connector.gameObject);
            }
        }

        // Define conexiones entre skills antes de actualizar la UI inicial
        if (skillList.Count > 0) skillList[0].skillconexion = new[] { 1, 2, 3 };
        if (skillList.Count > 2) skillList[2].skillconexion = new[] { 4, 5 };

        // Inicialmente ocultar todas las skills excepto la primera, pero mostrar
        // aquellas que ya estuvieran compradas (niveles guardados > 0)
        for (int i = 0; i < skillList.Count; i++)
        {
            if (skillList[i] == null) continue;
            var active = (i == 0) || (i < skilllevel.Length && skilllevel[i] > 0);
            skillList[i].gameObject.SetActive(active);
        }

        // Inicialmente ocultar todos los conectores excepto el primero (si existe)
        for (int i = 0; i < connectotrlist.Count; i++)
        {
            if (connectotrlist[i] == null) continue;
            // Mantener el primer conector visible para que la raíz siempre tenga su conexión
            connectotrlist[i].SetActive(i == 0);
        }

        // Actualiza la UI teniendo ya las conexiones definidas
        updateallskillui();
    }

    public void updateallskillui()
    {
        foreach (var skill in skillList) {skill.UpdateUI();}
    }
}