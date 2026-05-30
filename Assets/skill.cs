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

        if (skillDescription != null)
        {
            skillDescription.SetText(descText + $"\n\nSkill Points: {tree.SkillPoints}");
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
        if (tree.SkillPoints < 1 || tree.skilllevel[skillID] >= tree.skillcap[skillID]) return;

        // Consumir punto y aumentar nivel
        tree.SkillPoints -= 1;
        tree.skilllevel[skillID]++;

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
    }
    
}