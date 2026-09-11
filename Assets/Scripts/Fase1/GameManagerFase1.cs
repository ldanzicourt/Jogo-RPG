using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManagerFase1 : MonoBehaviour
{
    [Header("Status da Missão")]
    public string nomeFase = "Fase 1: O Despertar (Pradaria da Ferrugem)";
    public bool faseConcluida = false;
    public bool jogoFinalizado = false;

    private PlayerCombatD20 jogador;
    private TextMeshProUGUI textoHudVida;
    private TextMeshProUGUI textoHudObjetivo;
    private TextMeshProUGUI textoHudBots;
    private GameObject painelFimFase;
    private TextMeshProUGUI textoTituloFimFase;
    private TextMeshProUGUI textoDetalhesFimFase;

    void Start()
    {
        jogador = FindAnyObjectByType<PlayerCombatD20>();
        ConstruirInterfaceGrafica();
    }

    void Update()
    {
        AtualizarHud();
    }

    void AtualizarHud()
    {
        if (jogador != null && textoHudVida != null)
        {
            float porcentagem = Mathf.Clamp01(jogador.pvAtual / jogador.pvMaximo);
            string corHex = (porcentagem > 0.5f) ? "#44ff88" : ((porcentagem > 0.25f) ? "#ffaa33" : "#ff4444");
            textoHudVida.text = $"❤️ <b>HP DO HERÓI:</b> <color={corHex}>{jogador.pvAtual:F0} / {jogador.pvMaximo:F0} PV</color>  |  🛡️ <b>CA:</b> {jogador.classeArmadura}";
        }
    }

    public void ConcluirFaseComSucesso()
    {
        if (faseConcluida) return;
        faseConcluida = true;
        jogoFinalizado = true;

        Time.timeScale = 0.2f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (painelFimFase != null)
        {
            painelFimFase.SetActive(true);
            if (textoTituloFimFase != null)
            {
                textoTituloFimFase.text = "🏆 FASE 1 CONCLUÍDA: SOBREVIVENTES SALVOS!";
                textoTituloFimFase.color = new Color(0.2f, 1f, 0.5f);
            }
            if (textoDetalhesFimFase != null)
            {
                textoDetalhesFimFase.text = "A porta pesada da Sala Segura se selou atrás de vocês.\nAs Cascas de Vidro e Sucatas Animadas não conseguiram atravessar a blindagem.\n\n<b>Preparem-se para o Capítulo 2...</b>";
            }
        }
    }

    public void NotificarMorteJogador()
    {
        if (jogoFinalizado) return;
        jogoFinalizado = true;

        Time.timeScale = 0.2f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (painelFimFase != null)
        {
            painelFimFase.SetActive(true);
            if (textoTituloFimFase != null)
            {
                textoTituloFimFase.text = "💀 VOCÊ TOMBOU NA PRADARIA!";
                textoTituloFimFase.color = new Color(1f, 0.25f, 0.25f);
            }
            if (textoDetalhesFimFase != null)
            {
                textoDetalhesFimFase.text = "Seu corpo foi estilhaçado e tragado pelo abismo de metal flutuante.\nA horda alienígena consumiu os sobreviventes restantes.\n\n<i>Tente novamente com sua equipe!</i>";
            }
        }
    }

    public void ReiniciarFase()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void ConstruirInterfaceGrafica()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("CanvasFase1");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // 1. Painel HUD de Combate (Inferior Esquerdo)
        GameObject painelHud = new GameObject("PainelHUDCombate");
        painelHud.transform.SetParent(canvas.transform, false);
        RectTransform rHud = painelHud.AddComponent<RectTransform>();
        rHud.anchorMin = new Vector2(0f, 0f);
        rHud.anchorMax = new Vector2(0f, 0f);
        rHud.pivot = new Vector2(0f, 0f);
        rHud.anchoredPosition = new Vector2(25f, 25f);
        rHud.sizeDelta = new Vector2(360f, 54f);

        Image imgHud = painelHud.AddComponent<Image>();
        imgHud.color = new Color(0.08f, 0.10f, 0.14f, 0.85f);

        // Texto Vida
        GameObject txtVidaObj = new GameObject("TextoVida");
        txtVidaObj.transform.SetParent(painelHud.transform, false);
        RectTransform rVida = txtVidaObj.AddComponent<RectTransform>();
        rVida.anchorMin = Vector2.zero;
        rVida.anchorMax = Vector2.one;
        rVida.offsetMin = new Vector2(16f, 0f);
        rVida.offsetMax = new Vector2(-16f, 0f);

        textoHudVida = txtVidaObj.AddComponent<TextMeshProUGUI>();
        textoHudVida.fontSize = 17;
        textoHudVida.alignment = TextAlignmentOptions.MidlineLeft;

        if (jogador == null)
        {
            painelHud.SetActive(false);
        }

        // 2. Banner de Objetivo (Topo Central)
        GameObject objBanner = new GameObject("BannerObjetivo");
        objBanner.transform.SetParent(canvas.transform, false);
        RectTransform rBanner = objBanner.AddComponent<RectTransform>();
        rBanner.anchorMin = new Vector2(0.5f, 1f);
        rBanner.anchorMax = new Vector2(0.5f, 1f);
        rBanner.pivot = new Vector2(0.5f, 1f);
        rBanner.anchoredPosition = new Vector2(0f, -20f);
        rBanner.sizeDelta = new Vector2(580f, 48f);

        Image imgBanner = objBanner.AddComponent<Image>();
        imgBanner.color = new Color(0.1f, 0.12f, 0.16f, 0.88f);

        GameObject txtObj = new GameObject("TextoObjetivo");
        txtObj.transform.SetParent(objBanner.transform, false);
        RectTransform rTxt = txtObj.AddComponent<RectTransform>();
        rTxt.anchorMin = Vector2.zero;
        rTxt.anchorMax = Vector2.one;
        rTxt.offsetMin = new Vector2(10f, 0f);
        rTxt.offsetMax = new Vector2(-10f, 0f);

        textoHudObjetivo = txtObj.AddComponent<TextMeshProUGUI>();
        textoHudObjetivo.fontSize = 15;
        textoHudObjetivo.fontStyle = FontStyles.Bold;
        textoHudObjetivo.alignment = TextAlignmentOptions.Center;
        textoHudObjetivo.text = "🎯 <b>OBJETIVO:</b> Atravesse a Pradaria da Ferrugem e entre na <b>Sala Segura</b>!";

        // 3. Retículo Central de Mira (Crosshair)
        GameObject reticulo = new GameObject("Crosshair3P");
        reticulo.transform.SetParent(canvas.transform, false);
        RectTransform rRet = reticulo.AddComponent<RectTransform>();
        rRet.anchorMin = new Vector2(0.5f, 0.5f);
        rRet.anchorMax = new Vector2(0.5f, 0.5f);
        rRet.pivot = new Vector2(0.5f, 0.5f);
        rRet.sizeDelta = new Vector2(10f, 10f);

        Image imgRet = reticulo.AddComponent<Image>();
        imgRet.color = new Color(1f, 1f, 1f, 0.65f);

        // 4. Modal de Fim de Fase / Game Over (Inicialmente Oculto)
        painelFimFase = new GameObject("ModalFimDeFase");
        painelFimFase.transform.SetParent(canvas.transform, false);
        RectTransform rModal = painelFimFase.AddComponent<RectTransform>();
        rModal.anchorMin = Vector2.zero;
        rModal.anchorMax = Vector2.one;
        rModal.offsetMin = Vector2.zero;
        rModal.offsetMax = Vector2.zero;

        Image imgFundoModal = painelFimFase.AddComponent<Image>();
        imgFundoModal.color = new Color(0.04f, 0.05f, 0.07f, 0.94f);

        // Caixa de Diálogo
        GameObject caixa = new GameObject("CaixaDialogo");
        caixa.transform.SetParent(painelFimFase.transform, false);
        RectTransform rCaixa = caixa.AddComponent<RectTransform>();
        rCaixa.anchorMin = new Vector2(0.5f, 0.5f);
        rCaixa.anchorMax = new Vector2(0.5f, 0.5f);
        rCaixa.pivot = new Vector2(0.5f, 0.5f);
        rCaixa.sizeDelta = new Vector2(620f, 280f);

        // Título
        GameObject txtTitObj = new GameObject("TituloFim");
        txtTitObj.transform.SetParent(caixa.transform, false);
        RectTransform rTit = txtTitObj.AddComponent<RectTransform>();
        rTit.anchorMin = new Vector2(0f, 0.65f);
        rTit.anchorMax = new Vector2(1f, 1f);
        rTit.offsetMin = Vector2.zero;
        rTit.offsetMax = Vector2.zero;

        textoTituloFimFase = txtTitObj.AddComponent<TextMeshProUGUI>();
        textoTituloFimFase.fontSize = 24;
        textoTituloFimFase.fontStyle = FontStyles.Bold;
        textoTituloFimFase.alignment = TextAlignmentOptions.Center;

        // Detalhes
        GameObject txtDetObj = new GameObject("DetalhesFim");
        txtDetObj.transform.SetParent(caixa.transform, false);
        RectTransform rDet = txtDetObj.AddComponent<RectTransform>();
        rDet.anchorMin = new Vector2(0f, 0.25f);
        rDet.anchorMax = new Vector2(1f, 0.65f);
        rDet.offsetMin = Vector2.zero;
        rDet.offsetMax = Vector2.zero;

        textoDetalhesFimFase = txtDetObj.AddComponent<TextMeshProUGUI>();
        textoDetalhesFimFase.fontSize = 16;
        textoDetalhesFimFase.alignment = TextAlignmentOptions.Center;

        // Botão Reiniciar
        GameObject btnObj = new GameObject("BotaoReiniciar");
        btnObj.transform.SetParent(caixa.transform, false);
        RectTransform rBtn = btnObj.AddComponent<RectTransform>();
        rBtn.anchorMin = new Vector2(0.5f, 0f);
        rBtn.anchorMax = new Vector2(0.5f, 0f);
        rBtn.pivot = new Vector2(0.5f, 0f);
        rBtn.anchoredPosition = new Vector2(0f, 18f);
        rBtn.sizeDelta = new Vector2(240f, 48f);

        Image imgBtn = btnObj.AddComponent<Image>();
        imgBtn.color = new Color(0.2f, 0.45f, 0.75f);

        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(ReiniciarFase);

        GameObject txtBtn = new GameObject("TextoBtn");
        txtBtn.transform.SetParent(btnObj.transform, false);
        RectTransform rTb = txtBtn.AddComponent<RectTransform>();
        rTb.anchorMin = Vector2.zero;
        rTb.anchorMax = Vector2.one;

        TextMeshProUGUI tmpB = txtBtn.AddComponent<TextMeshProUGUI>();
        tmpB.text = "JOGAR NOVAMENTE";
        tmpB.fontSize = 16;
        tmpB.fontStyle = FontStyles.Bold;
        tmpB.alignment = TextAlignmentOptions.Center;

        painelFimFase.SetActive(false);
    }
}
