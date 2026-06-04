using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using Image = UnityEngine.UI.Image;

public class Skill : MonoBehaviour
{
    public int skillID;

    [Header("UI Texts")]
    public TMP_Text skillName;
    public TMP_Text skillDescription;

    [Header("Tree Connections")]
    public int[] skillconexion;

    public void UpdateUI()
    {
        var tree = skiltree.instance;
        if (tree == null) return;

        // Seguridad: comprobar índices y arrays antes de usar
        var nameText = (tree.baseSkillNames != null && skillID >= 0 && skillID < tree.baseSkillNames.Length)
            ? tree.baseSkillNames[skillID]
            : string.Empty;

        var descText = (tree.baseSkillDescriptions != null && skillID >= 0 && skillID < tree.baseSkillDescriptions.Length)
            ? tree.baseSkillDescriptions[skillID]
            : string.Empty;

        var level = (tree.skilllevel != null && skillID >= 0 && skillID < tree.skilllevel.Length) ? tree.skilllevel[skillID] : 0;
        var cap = (tree.skillcap != null && skillID >= 0 && skillID < tree.skillcap.Length) ? tree.skillcap[skillID] : 0;

        if (skillName != null)
        {
            skillName.SetText($"{nameText} ({level}/{cap})");
        }

        // Mostrar descripci�n y coste junto a los puntos disponibles
        var cost = (tree.baseSkillCosts != null && skillID >= 0 && skillID < tree.baseSkillCosts.Length) ? tree.baseSkillCosts[skillID] : 0;
        if (skillDescription != null)
        {
            skillDescription.SetText(descText + $"\n\nCoste: {cost}");
        }

        var img = GetComponent<Image>();
        if (img != null)
        {
            img.color = level >= cap ? Color.yellow : tree.SkillPoints > 0 ? Color.green : Color.white;
        }

        // Si no hay conexiones, no hay nada que activar/desactivar
        if (skillconexion == null || skillconexion.Length == 0) return;

        // Activa/desactiva conectores y skills hijos con comprobaciones de rango
        foreach (int conexion in skillconexion)
        {
            var available = level > 0;
            if (tree.connectotrlist != null && conexion >= 0 && conexion < tree.connectotrlist.Count && tree.connectotrlist[conexion] != null)
                tree.connectotrlist[conexion].SetActive(available);

            if (tree.skillList != null && conexion >= 0 && conexion < tree.skillList.Count && tree.skillList[conexion] != null)
                tree.skillList[conexion].gameObject.SetActive(available);
        }
    }

    public void Buy()
    {
        var tree = skiltree.instance;
        if (tree == null) return;
        // Comprobaciones de seguridad
        if (skillID < 0) return;
        var cap = (tree.skillcap != null && skillID < tree.skillcap.Length) ? tree.skillcap[skillID] : 0;
        var level = (tree.skilllevel != null && skillID < tree.skilllevel.Length) ? tree.skilllevel[skillID] : 0;
        var cost = (tree.baseSkillCosts != null && skillID < tree.baseSkillCosts.Length) ? tree.baseSkillCosts[skillID] : 0;

        // Disponibilidad total mostrada en UI: SkillPoints guardados + score actual (coins)
        int availableFromSaved = (int)System.Math.Floor(tree.SkillPoints);
        int availableFromRun = 0;
        var gm = GameManager.instance;
        if (gm != null) availableFromRun = gm.coins;

        var totalAvailable = availableFromSaved + availableFromRun;

        if (totalAvailable < cost || level >= cap) return;

        // Consumir primero de SkillPoints guardados, luego del run (coins)
        int remaining = cost;
        if (availableFromSaved > 0)
        {
            var useSaved = System.Math.Min(availableFromSaved, remaining);
            tree.SkillPoints -= useSaved;
            remaining -= useSaved;
        }

        if (remaining > 0 && gm != null)
        {
            // Restar de las monedas del juego
            var takeFromCoins = System.Math.Min(gm.coins, remaining);
            gm.coins -= takeFromCoins;
            // Asegurar que score refleje coins (GameManager.Update normalmente lo hace)
            gm.score = gm.coins;
            remaining -= takeFromCoins;
        }

        // remaining debe ser 0 aquí
        tree.skilllevel[skillID]++;

        // Persistir SkillPoints actualizados
        PlayerPrefs.SetFloat("SkillPoints", (float)tree.SkillPoints);

        // Guardar el nuevo nivel
        PlayerPrefs.SetInt($"skilllevel_{skillID}", tree.skilllevel[skillID]);

        // Al comprar, activar las skills conectadas y sus conectores
        if (skillconexion != null)
        {
            foreach (var conexion in skillconexion)
            {
                if (conexion >= 0 && tree.skillList != null && conexion < tree.skillList.Count && tree.skillList[conexion] != null)
                    tree.skillList[conexion].gameObject.SetActive(true);

                if (conexion >= 0 && tree.connectotrlist != null && conexion < tree.connectotrlist.Count && tree.connectotrlist[conexion] != null)
                    tree.connectotrlist[conexion].SetActive(true);
            }
        }

        // Actualiza la UI de todo el árbol
        tree.updateallskillui();
        // Forzar sincronización inmediata con GameManager si existe
        if (gm != null)
        {
            Debug.Log($"[Skill] Bought skill {skillID}. shieldCooldownUpgrades now={tree.shieldCooldownUpgrades}");
            gm.RefreshSkillDerivedStats();
        }
    }
    
}