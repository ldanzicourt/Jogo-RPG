using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HudDruida : MonoBehaviour
{
    private ControleDruida controleDruida;
    private GameObject painelHud;
    private TextMeshProUGUI textoEstacao;
    private TextMeshProUGUI textoSuprema;
    private TextMeshProUGUI textoDicas;
    private Image imagemFundoEstacao;

    void Start()
    {
        controleDruida = GetComponentInParent<ControleDruida>();
        if (controleDruida == null) controleDruida = GetComponent<ControleDruida>();
        if (controleDruida == null) controleDruida = FindAnyObjectByType<ControleDruida>();

        ConstruirInterfaceHud();
    }

    void Update()
    {
        if (controleDruida == null) return;

        // Atualiza texto da Estação
        if (textoEstacao != null && imagemFundoEstacao != null)
        {
            if (controleDruida.estacaoAtual == EstacaoDruida.Primavera)
            {
                textoEstacao.text = "🌸 PRIMAVERA\n<size=13><color=#88ff88>+Aura de Regeneração (5 HP/s)</color></size>";
                imagemFundoEstacao.color = new Color(0.1f, 0.45f, 0.18f, 0.75f);
            }
            else
            {
                textoEstacao.text = "🍂 OUTONO\n<size=13><color=#ffaa33>+25% Dano & Decomposição</color></size>";
                imagemFundoEstacao.color = new Color(0.55f, 0.28f, 0.08f, 0.75f);
            }
        }

        // Atualiza Suprema
        if (textoSuprema != null)
        {
            float tempoRestante = controleDruida.ObterTempoRestanteSuprema();
            if (tempoRestante > 0)
            {
                textoSuprema.text = "🌳 Bosque (F): <color=#ff4444>" + Mathf.Ceil(tempoRestante) + "s</color>";
            }
            else
            {
                textoSuprema.text = "🌳 Bosque (F): <color=#44ff44>PRONTO!</color>";
            }
        }
    }

    void ConstruirInterfaceHud()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        // Painel no canto inferior direito
        painelHud = new GameObject("PainelDruidaHUD");
        painelHud.transform.SetParent(canvas.transform, false);

        RectTransform rectPainel = painelHud.AddComponent<RectTransform>();
        rectPainel.anchorMin = new Vector2(1f, 0f);
        rectPainel.anchorMax = new Vector2(1f, 0f);
        rectPainel.pivot = new Vector2(1f, 0f);
        rectPainel.anchoredPosition = new Vector2(-20f, 20f);
        rectPainel.sizeDelta = new Vector2(280f, 130f);

        imagemFundoEstacao = painelHud.AddComponent<Image>();
        imagemFundoEstacao.color = new Color(0.1f, 0.45f, 0.18f, 0.75f);

        // Texto da Estação
        GameObject objEstacao = new GameObject("TextoEstacao");
        objEstacao.transform.SetParent(painelHud.transform, false);
        RectTransform rectEstacao = objEstacao.AddComponent<RectTransform>();
        rectEstacao.anchorMin = new Vector2(0f, 0.55f);
        rectEstacao.anchorMax = new Vector2(1f, 1f);
        rectEstacao.offsetMin = new Vector2(10f, 0f);
        rectEstacao.offsetMax = new Vector2(-10f, -5f);

        textoEstacao = objEstacao.AddComponent<TextMeshProUGUI>();
        textoEstacao.fontSize = 17;
        textoEstacao.fontStyle = FontStyles.Bold;
        textoEstacao.alignment = TextAlignmentOptions.Center;
        textoEstacao.text = "🌸 PRIMAVERA";

        // Texto da Suprema
        GameObject objSuprema = new GameObject("TextoSuprema");
        objSuprema.transform.SetParent(painelHud.transform, false);
        RectTransform rectSuprema = objSuprema.AddComponent<RectTransform>();
        rectSuprema.anchorMin = new Vector2(0f, 0.28f);
        rectSuprema.anchorMax = new Vector2(1f, 0.55f);
        rectSuprema.offsetMin = new Vector2(10f, 0f);
        rectSuprema.offsetMax = new Vector2(-10f, 0f);

        textoSuprema = objSuprema.AddComponent<TextMeshProUGUI>();
        textoSuprema.fontSize = 14;
        textoSuprema.alignment = TextAlignmentOptions.Center;
        textoSuprema.text = "🌳 Bosque (F): PRONTO!";

        // Dica de Controles
        GameObject objDicas = new GameObject("TextoDicas");
        objDicas.transform.SetParent(painelHud.transform, false);
        RectTransform rectDicas = objDicas.AddComponent<RectTransform>();
        rectDicas.anchorMin = new Vector2(0f, 0f);
        rectDicas.anchorMax = new Vector2(1f, 0.28f);
        rectDicas.offsetMin = new Vector2(5f, 5f);
        rectDicas.offsetMax = new Vector2(-5f, 0f);

        textoDicas = objDicas.AddComponent<TextMeshProUGUI>();
        textoDicas.fontSize = 11;
        textoDicas.alignment = TextAlignmentOptions.Center;
        textoDicas.color = new Color(0.9f, 0.9f, 0.9f, 0.9f);
        textoDicas.text = "[Ctrl] Alternar Estação | [Shift] Cervo 3ªP | [RMB] Casca";
    }

    void OnDestroy()
    {
        if (painelHud != null) Destroy(painelHud);
    }
}
