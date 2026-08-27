using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HudRanger : MonoBehaviour
{
    private ControleRanger controleRanger;
    private GameObject painelHud;
    private TextMeshProUGUI textoEstilo;
    private TextMeshProUGUI textoHabilidades;
    private TextMeshProUGUI textoPassiva;
    private TextMeshProUGUI textoDicas;
    private Image imagemFundoEstilo;

    void Start()
    {
        controleRanger = GetComponentInParent<ControleRanger>();
        if (controleRanger == null) controleRanger = GetComponent<ControleRanger>();
        if (controleRanger == null) controleRanger = FindAnyObjectByType<ControleRanger>();

        ConstruirInterfaceHud();
    }

    void Update()
    {
        if (controleRanger == null) return;

        // 1. Atualiza Estilo de Luta
        if (textoEstilo != null && imagemFundoEstilo != null)
        {
            if (controleRanger.estiloAtual == EstiloLutaRanger.Arquearia)
            {
                textoEstilo.text = "🏹 ARQUEARIA\n<size=12><color=#88ffaa>+Disparos Balísticos de Arco Curto</color></size>";
                imagemFundoEstilo.color = new Color(0.12f, 0.22f, 0.16f, 0.85f); // Verde Floresta Escuro
            }
            else
            {
                textoEstilo.text = "⚔️ DUAS ARMAS\n<size=12><color=#ffcc66>+Cortes Rápidos (Garras de Rapina)</color></size>";
                imagemFundoEstilo.color = new Color(0.24f, 0.16f, 0.10f, 0.85f); // Couro Escuro Tático
            }
        }

        // 2. Atualiza Cooldowns das Habilidades
        if (textoHabilidades != null)
        {
            float tempoGancho = controleRanger.ObterTempoRestanteGancho();
            float tempoEsquiva = controleRanger.ObterTempoRestanteEsquiva();
            float tempoSuprema = controleRanger.ObterTempoRestanteSuprema();
            int armadilhasAtivas = controleRanger.ObterQtdArmadilhasAtivas();

            string statusGancho = tempoGancho > 0 ? "<color=#ff5555>" + Mathf.Ceil(tempoGancho) + "s</color>" : "<color=#55ff55>PRONTO</color>";
            string statusEsquiva = tempoEsquiva > 0 ? "<color=#ff5555>" + Mathf.Ceil(tempoEsquiva) + "s</color>" : "<color=#55ff55>PRONTO (" + armadilhasAtivas + "/2)</color>";
            string statusSuprema = tempoSuprema > 0 ? "<color=#ff5555>" + Mathf.Ceil(tempoSuprema) + "s</color>" : "<color=#55ff55>PRONTO</color>";

            textoHabilidades.text = $"🧗 Gancho [Shift]: {statusGancho}  |  🛡️ Armadilha [RMB]: {statusEsquiva}\n🏹 Saraivada [F]: {statusSuprema}";
        }

        // 3. Atualiza Passiva (Marca do Caçador)
        if (textoPassiva != null)
        {
            int acertos = controleRanger.ObterAcertosConsecutivos();
            if (acertos >= 3)
            {
                textoPassiva.text = "🎯 Marca do Caçador: <color=#ffd700><b>ATIVADA (+1d6 DANO BÔNUS)!</b></color>";
            }
            else if (acertos > 0)
            {
                textoPassiva.text = $"🎯 Marca do Caçador: <color=#ffaa44>Acertos Consecutivos [{acertos}/3]</color>";
            }
            else
            {
                textoPassiva.text = "🎯 Marca do Caçador: <color=#aaaaaa>Aguardando Alvo (3 acertos)</color>";
            }
        }
    }

    Canvas ObterCanvasPrincipal()
    {
        Canvas[] todos = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (var c in todos)
        {
            if (c.renderMode != RenderMode.WorldSpace) return c;
        }
        return todos.Length > 0 ? todos[0] : null;
    }

    void ConstruirInterfaceHud()
    {
        Canvas canvas = ObterCanvasPrincipal();
        if (canvas == null) return;

        // Painel no canto inferior direito
        painelHud = new GameObject("PainelRangerHUD");
        painelHud.transform.SetParent(canvas.transform, false);

        RectTransform rectPainel = painelHud.AddComponent<RectTransform>();
        rectPainel.anchorMin = new Vector2(1f, 0f);
        rectPainel.anchorMax = new Vector2(1f, 0f);
        rectPainel.pivot = new Vector2(1f, 0f);
        rectPainel.anchoredPosition = new Vector2(-20f, 20f);
        rectPainel.sizeDelta = new Vector2(330f, 150f);

        imagemFundoEstilo = painelHud.AddComponent<Image>();
        imagemFundoEstilo.color = new Color(0.12f, 0.22f, 0.16f, 0.85f);

        // 1. Texto do Estilo de Luta
        GameObject objEstilo = new GameObject("TextoEstilo");
        objEstilo.transform.SetParent(painelHud.transform, false);
        RectTransform rectEstilo = objEstilo.AddComponent<RectTransform>();
        rectEstilo.anchorMin = new Vector2(0f, 0.65f);
        rectEstilo.anchorMax = new Vector2(1f, 1f);
        rectEstilo.offsetMin = new Vector2(10f, 0f);
        rectEstilo.offsetMax = new Vector2(-10f, -5f);

        textoEstilo = objEstilo.AddComponent<TextMeshProUGUI>();
        textoEstilo.fontSize = 15;
        textoEstilo.fontStyle = FontStyles.Bold;
        textoEstilo.alignment = TextAlignmentOptions.Center;
        textoEstilo.text = "🏹 ARQUEARIA";

        // 2. Texto das Habilidades
        GameObject objHab = new GameObject("TextoHabilidades");
        objHab.transform.SetParent(painelHud.transform, false);
        RectTransform rectHab = objHab.AddComponent<RectTransform>();
        rectHab.anchorMin = new Vector2(0f, 0.35f);
        rectHab.anchorMax = new Vector2(1f, 0.65f);
        rectHab.offsetMin = new Vector2(10f, 0f);
        rectHab.offsetMax = new Vector2(-10f, 0f);

        textoHabilidades = objHab.AddComponent<TextMeshProUGUI>();
        textoHabilidades.fontSize = 12;
        textoHabilidades.alignment = TextAlignmentOptions.Center;
        textoHabilidades.text = "🧗 Gancho [Shift]: PRONTO  |  🛡️ Armadilha [RMB]: PRONTO\n🏹 Saraivada [F]: PRONTO";

        // 3. Texto da Passiva
        GameObject objPassiva = new GameObject("TextoPassiva");
        objPassiva.transform.SetParent(painelHud.transform, false);
        RectTransform rectPassiva = objPassiva.AddComponent<RectTransform>();
        rectPassiva.anchorMin = new Vector2(0f, 0.18f);
        rectPassiva.anchorMax = new Vector2(1f, 0.35f);
        rectPassiva.offsetMin = new Vector2(10f, 0f);
        rectPassiva.offsetMax = new Vector2(-10f, 0f);

        textoPassiva = objPassiva.AddComponent<TextMeshProUGUI>();
        textoPassiva.fontSize = 11;
        textoPassiva.alignment = TextAlignmentOptions.Center;
        textoPassiva.text = "🎯 Marca do Caçador: [0/3]";

        // 4. Dica de Comandos
        GameObject objDicas = new GameObject("TextoDicas");
        objDicas.transform.SetParent(painelHud.transform, false);
        RectTransform rectDicas = objDicas.AddComponent<RectTransform>();
        rectDicas.anchorMin = new Vector2(0f, 0f);
        rectDicas.anchorMax = new Vector2(1f, 0.18f);
        rectDicas.offsetMin = new Vector2(5f, 3f);
        rectDicas.offsetMax = new Vector2(-5f, 0f);

        textoDicas = objDicas.AddComponent<TextMeshProUGUI>();
        textoDicas.fontSize = 10;
        textoDicas.alignment = TextAlignmentOptions.Center;
        textoDicas.color = new Color(0.85f, 0.85f, 0.85f, 0.8f);
        textoDicas.text = "[Ctrl] Estilo de Luta | [Shift] Gancho | [RMB] Esquiva | [LMB] Atacar";
    }

    void OnDestroy()
    {
        if (painelHud != null) Destroy(painelHud);
    }
}
