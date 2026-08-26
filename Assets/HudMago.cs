using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HudMago : MonoBehaviour
{
    [Header("Referências")]
    public ControleMago controleMago;

    private GameObject painelHudMago;
    private TextMeshProUGUI textoEscolaAtiva;
    private TextMeshProUGUI textoDescricaoMagia;
    private TextMeshProUGUI textoCooldowns;
    private Image badgeEscola;

    void Awake()
    {
        if (controleMago == null)
        {
            controleMago = GetComponentInParent<ControleMago>();
            if (controleMago == null) controleMago = FindFirstObjectByType<ControleMago>();
        }

        CriarPainelHudSeNecessario();
    }

    void OnEnable()
    {
        if (painelHudMago != null) painelHudMago.SetActive(true);
    }

    void OnDisable()
    {
        if (painelHudMago != null) painelHudMago.SetActive(false);
    }

    void Update()
    {
        if (controleMago == null || !controleMago.gameObject.activeInHierarchy)
        {
            if (painelHudMago != null && painelHudMago.activeSelf) painelHudMago.SetActive(false);
            return;
        }

        if (painelHudMago != null && !painelHudMago.activeSelf)
        {
            painelHudMago.SetActive(true);
        }

        AtualizarInformacoes();
    }

    void AtualizarInformacoes()
    {
        if (textoEscolaAtiva == null || controleMago == null) return;

        EscolaMagica escola = controleMago.escolaAtual;
        string nomeEscola = "";
        string descMagia = "";
        Color corEscola = Color.white;

        switch (escola)
        {
            case EscolaMagica.Evocacao:
                nomeEscola = "⚡ EVOCAÇÃO: DARDO ARCANO";
                descMagia = "Disparo Único: Projétil de pura energia veloz e de longo alcance";
                corEscola = new Color(0.1f, 0.7f, 1f);
                break;
            case EscolaMagica.Necromancia:
                nomeEscola = "💀 NECROMANCIA: SIFÃO DE ALMA";
                descMagia = "Canalização: Feixe contínuo que drena vida e cura o Mago";
                corEscola = new Color(0.85f, 0.25f, 1f);
                break;
            case EscolaMagica.Conjuracao:
                nomeEscola = "🌀 CONJURAÇÃO: VÓRTICE DE TRAMA";
                float cdVort = controleMago.ObterCooldownVorticeRestante();
                descMagia = cdVort > 0
                    ? $"[Recarregando: {cdVort:F1}s] Fenda que puxa inimigos e desacelera 40%"
                    : "Alvo de Área: Cria uma fenda no chão que puxa inimigos e desacelera 40%";
                corEscola = new Color(0.2f, 1f, 0.8f);
                break;
        }

        textoEscolaAtiva.text = nomeEscola;
        textoEscolaAtiva.color = corEscola;
        textoDescricaoMagia.text = descMagia;

        // Status dos Cooldowns
        float cdSalto = controleMago.ObterCooldownSaltoRestante();
        float cdSuprema = controleMago.ObterCooldownSupremaRestante();

        string statusSalto = cdSalto > 0 ? $"<color=#FF6B6B>{cdSalto:F1}s</color>" : "<color=#4CD137>PRONTO</color>";
        string statusSuprema = cdSuprema > 0 ? $"<color=#FF6B6B>{cdSuprema:F1}s</color>" : "<color=#4CD137>PRONTO</color>";
        string statusBarreira = controleMago.EstaComBarreiraAtiva() ? "<color=#00D2D3>ESCUDO ATIVO</color>" : "<color=#4CD137>PRONTO (RMB)</color>";

        if (textoCooldowns != null)
        {
            textoCooldowns.text = $"[Shift] Salto Astral: {statusSalto}  |  [RMB] Barreira: {statusBarreira}  |  [F] Desintegração: {statusSuprema}\n" +
                                  $"<size=14><color=#DFE4EA>[Ctrl / Scroll] Folhear Grimório  |  [LMB] Disparar  |  Passiva: 3 Marcas causam Explosão Arcana</color></size>";
        }
    }

    void CriarPainelHudSeNecessario()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        Transform hudExistente = canvas.transform.Find("HudMagoPainel");
        if (hudExistente != null)
        {
            painelHudMago = hudExistente.gameObject;
            textoEscolaAtiva = painelHudMago.transform.Find("TextoEscola")?.GetComponent<TextMeshProUGUI>();
            textoDescricaoMagia = painelHudMago.transform.Find("TextoDescricao")?.GetComponent<TextMeshProUGUI>();
            textoCooldowns = painelHudMago.transform.Find("TextoCooldowns")?.GetComponent<TextMeshProUGUI>();
            return;
        }

        painelHudMago = new GameObject("HudMagoPainel");
        painelHudMago.transform.SetParent(canvas.transform, false);

        RectTransform rt = painelHudMago.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 25f);
        rt.sizeDelta = new Vector2(750f, 130f);

        Image fundo = painelHudMago.AddComponent<Image>();
        fundo.color = new Color(0.05f, 0.05f, 0.12f, 0.85f); // Fundo escuro translúcido moderno

        // 1. Título da Escola Ativa
        GameObject objEscola = new GameObject("TextoEscola");
        objEscola.transform.SetParent(painelHudMago.transform, false);
        textoEscolaAtiva = objEscola.AddComponent<TextMeshProUGUI>();
        textoEscolaAtiva.rectTransform.anchoredPosition = new Vector2(0f, 38f);
        textoEscolaAtiva.rectTransform.sizeDelta = new Vector2(720f, 32f);
        textoEscolaAtiva.fontSize = 22f;
        textoEscolaAtiva.fontStyle = FontStyles.Bold;
        textoEscolaAtiva.alignment = TextAlignmentOptions.Center;
        textoEscolaAtiva.text = "⚡ EVOCAÇÃO: DARDO ARCANO";

        // 2. Descrição da Magia
        GameObject objDesc = new GameObject("TextoDescricao");
        objDesc.transform.SetParent(painelHudMago.transform, false);
        textoDescricaoMagia = objDesc.AddComponent<TextMeshProUGUI>();
        textoDescricaoMagia.rectTransform.anchoredPosition = new Vector2(0f, 12f);
        textoDescricaoMagia.rectTransform.sizeDelta = new Vector2(720f, 24f);
        textoDescricaoMagia.fontSize = 15f;
        textoDescricaoMagia.alignment = TextAlignmentOptions.Center;
        textoDescricaoMagia.color = new Color(0.85f, 0.9f, 0.95f);

        // 3. Cooldowns e Controles
        GameObject objCd = new GameObject("TextoCooldowns");
        objCd.transform.SetParent(painelHudMago.transform, false);
        textoCooldowns = objCd.AddComponent<TextMeshProUGUI>();
        textoCooldowns.rectTransform.anchoredPosition = new Vector2(0f, -22f);
        textoCooldowns.rectTransform.sizeDelta = new Vector2(720f, 45f);
        textoCooldowns.fontSize = 15f;
        textoCooldowns.alignment = TextAlignmentOptions.Center;
    }
}
