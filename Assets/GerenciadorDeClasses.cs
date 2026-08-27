using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GerenciadorDeClasses : MonoBehaviour
{
    [Header("Interface")]
    public GameObject telaDeSelecao;   // O painel com os botões
    public GameObject hudDoJogador;    // As barras do jogador
    public GameObject barraDeFuria;    // A barra de fúria (exclusiva do Guerreiro!)

    [Header("Jogador")]
    public PlayerController controleMovimento;

    [Header("Classes (Armas/Scripts)")]
    public GameObject classeGuerreiro; // A sua Espada atual
    public GameObject classeMago;      // Cajado/mãos mágicas
    public GameObject classeRanger;    // O Arco e Cimitarras
    public GameObject classeDruida;    // Garras/magia da natureza

    // Controle de Pause
    private bool jogoIniciado = false;
    private bool jogoPausado = false;
    private GameObject painelMenuPause = null;

    void Start()
    {
        // 1. Pausa o tempo do jogo para os inimigos não atacarem
        Time.timeScale = 0f;

        // 2. Garante que o mouse esteja visível para clicar nos botões
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 3. Desativa o movimento e o HUD provisoriamente
        if (controleMovimento != null) controleMovimento.enabled = false;
        if (hudDoJogador != null) hudDoJogador.SetActive(false);
        if (telaDeSelecao != null) telaDeSelecao.SetActive(true);

        // 4. Esconde todas as armas/classes por padrão
        if (classeGuerreiro != null) classeGuerreiro.SetActive(false);
        if (classeMago != null) classeMago.SetActive(false);
        if (classeRanger != null) classeRanger.SetActive(false);
        if (classeDruida != null) classeDruida.SetActive(false);

        // 5. Garante que os botões da interface chamem os métodos corretos
        ConfigurarBotoesMenu();
    }

    void Update()
    {
        // Aciona o Menu de Pause ao pressionar ESC durante o jogo
        if (jogoIniciado && Input.GetKeyDown(KeyCode.Escape))
        {
            AlternarPause();
        }
    }

    void ConfigurarBotoesMenu()
    {
        UnityEngine.UI.Button[] botoes = FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None);
        foreach (var btn in botoes)
        {
            string nome = btn.gameObject.name.ToLower();
            if (nome.Contains("mago"))
            {
                btn.onClick.RemoveListener(EscolherMago);
                btn.onClick.AddListener(EscolherMago);
            }
            else if (nome.Contains("ranger") || nome.Contains("arco"))
            {
                btn.onClick.RemoveListener(EscolherRanger);
                btn.onClick.AddListener(EscolherRanger);
            }
            else if (nome.Contains("guerreiro") || nome.Contains("espada"))
            {
                btn.onClick.RemoveListener(EscolherGuerreiro);
                btn.onClick.AddListener(EscolherGuerreiro);
            }
            else if (nome.Contains("druida"))
            {
                btn.onClick.RemoveListener(EscolherDruida);
                btn.onClick.AddListener(EscolherDruida);
            }
        }
    }

    // --- FUNÇÕES DOS BOTÕES DE CLASSE ---

    public void EscolherGuerreiro()
    {
        // Esconde as outras armas
        if (classeRanger != null) classeRanger.SetActive(false);
        if (classeMago != null) classeMago.SetActive(false);
        if (classeDruida != null) classeDruida.SetActive(false);

        if (classeGuerreiro == null)
        {
            Transform parentCam = controleMovimento != null ? controleMovimento.playerCamera : Camera.main?.transform;
            GameObject guerreiroObj = new GameObject("ClasseGuerreiro");
            if (parentCam != null)
            {
                guerreiroObj.transform.SetParent(parentCam);
                guerreiroObj.transform.localPosition = Vector3.zero;
                guerreiroObj.transform.localRotation = Quaternion.identity;
            }
            guerreiroObj.AddComponent<ModeloEspadaBerserker>();
            guerreiroObj.AddComponent<AtaqueEspada>();
            classeGuerreiro = guerreiroObj;
        }
        else
        {
            if (classeGuerreiro.GetComponent<ModeloEspadaBerserker>() == null)
                classeGuerreiro.AddComponent<ModeloEspadaBerserker>();
            if (classeGuerreiro.GetComponent<AtaqueEspada>() == null)
                classeGuerreiro.AddComponent<AtaqueEspada>();
        }

        classeGuerreiro.SetActive(true);

        // Mostra a barra de Fúria (exclusiva do Guerreiro)
        if (barraDeFuria != null) barraDeFuria.SetActive(true);

        // Define o modo Guerreiro no PlayerController (Investida)
        if (controleMovimento != null) controleMovimento.DefinirClasse(PlayerController.TipoClasse.Guerreiro);

        Debug.Log("⚔️ Guerreiro Berserker selecionado com sucesso!");
        IniciarJogo();
    }

    public void EscolherRanger()
    {
        // Esconde as outras armas
        if (classeGuerreiro != null) classeGuerreiro.SetActive(false);
        if (classeMago != null) classeMago.SetActive(false);
        if (classeDruida != null) classeDruida.SetActive(false);

        // Oculta a barra de Fúria (Ranger não usa Fúria!)
        if (barraDeFuria != null) barraDeFuria.SetActive(false);

        // Se o objeto classeRanger não foi vinculado no Inspector, cria-o automaticamente na câmera do jogador
        if (classeRanger == null)
        {
            Transform parentCam = controleMovimento != null ? controleMovimento.playerCamera : Camera.main?.transform;
            GameObject rangerObj = new GameObject("ClasseRanger");
            if (parentCam != null)
            {
                rangerObj.transform.SetParent(parentCam);
                rangerObj.transform.localPosition = Vector3.zero;
                rangerObj.transform.localRotation = Quaternion.identity;
            }

            rangerObj.AddComponent<ModeloRangerProcedural>();
            rangerObj.AddComponent<ControleRanger>();
            rangerObj.AddComponent<HudRanger>();
            classeRanger = rangerObj;
        }
        else
        {
            // Remove o script antigo AtaqueArco caso exista no objeto legado
            AtaqueArco antigoArco = classeRanger.GetComponent<AtaqueArco>();
            if (antigoArco != null) Destroy(antigoArco);

            if (classeRanger.GetComponent<ModeloRangerProcedural>() == null)
                classeRanger.AddComponent<ModeloRangerProcedural>();
            if (classeRanger.GetComponent<ControleRanger>() == null)
                classeRanger.AddComponent<ControleRanger>();
            if (classeRanger.GetComponent<HudRanger>() == null)
                classeRanger.AddComponent<HudRanger>();
        }

        classeRanger.SetActive(true);

        // Define o modo Ranger no PlayerController
        if (controleMovimento != null) controleMovimento.DefinirClasse(PlayerController.TipoClasse.Ranger);

        Debug.Log("🏹 Patrulheiro (Ranger) selecionado com sucesso!");
        IniciarJogo();
    }

    public void EscolherMago()
    {
        // Esconde as outras armas
        if (classeGuerreiro != null) classeGuerreiro.SetActive(false);
        if (classeRanger != null) classeRanger.SetActive(false);
        if (classeDruida != null) classeDruida.SetActive(false);

        // Oculta a barra de Fúria (Mago não usa Fúria!)
        if (barraDeFuria != null) barraDeFuria.SetActive(false);

        // Se o objeto classeMago não foi vinculado no Inspector, cria-o automaticamente na câmera do jogador
        if (classeMago == null)
        {
            Transform parentCam = controleMovimento != null ? controleMovimento.playerCamera : Camera.main?.transform;
            GameObject magoObj = new GameObject("ClasseMago");
            if (parentCam != null)
            {
                magoObj.transform.SetParent(parentCam);
                magoObj.transform.localPosition = Vector3.zero;
                magoObj.transform.localRotation = Quaternion.identity;
            }

            magoObj.AddComponent<ModeloMagoProcedural>();
            magoObj.AddComponent<ControleMago>();
            magoObj.AddComponent<HudMago>();
            classeMago = magoObj;
        }
        else
        {
            // Garante que os componentes essenciais estejam anexados
            if (classeMago.GetComponent<ModeloMagoProcedural>() == null)
                classeMago.AddComponent<ModeloMagoProcedural>();
            if (classeMago.GetComponent<ControleMago>() == null)
                classeMago.AddComponent<ControleMago>();
            if (classeMago.GetComponent<HudMago>() == null)
                classeMago.AddComponent<HudMago>();
        }

        classeMago.SetActive(true);

        // Define o modo Mago no PlayerController (Salto Etéreo)
        if (controleMovimento != null) controleMovimento.DefinirClasse(PlayerController.TipoClasse.Mago);

        Debug.Log("🧙‍♂️ Mago selecionado com sucesso!");
        IniciarJogo();
    }

    public void EscolherDruida()
    {
        // Esconde as outras armas
        if (classeGuerreiro != null) classeGuerreiro.SetActive(false);
        if (classeRanger != null) classeRanger.SetActive(false);
        if (classeMago != null) classeMago.SetActive(false);

        // Esconde a barra de Fúria (exclusiva do Guerreiro)
        if (barraDeFuria != null) barraDeFuria.SetActive(false);

        // Configura a classe Druida
        if (classeDruida == null)
        {
            Transform parentCam = controleMovimento != null ? controleMovimento.playerCamera : Camera.main?.transform;
            GameObject druidaObj = new GameObject("ClasseDruida");
            if (parentCam != null)
            {
                druidaObj.transform.SetParent(parentCam);
                druidaObj.transform.localPosition = Vector3.zero;
                druidaObj.transform.localRotation = Quaternion.identity;
            }
            druidaObj.AddComponent<ModeloDruidaProcedural>();
            druidaObj.AddComponent<ModeloCervoEspectral>();
            druidaObj.AddComponent<ControleDruida>();
            druidaObj.AddComponent<HudDruida>();
            classeDruida = druidaObj;
        }
        else
        {
            if (classeDruida.GetComponent<ModeloDruidaProcedural>() == null)
                classeDruida.AddComponent<ModeloDruidaProcedural>();
            if (classeDruida.GetComponent<ModeloCervoEspectral>() == null)
                classeDruida.AddComponent<ModeloCervoEspectral>();
            if (classeDruida.GetComponent<ControleDruida>() == null)
                classeDruida.AddComponent<ControleDruida>();
            if (classeDruida.GetComponent<HudDruida>() == null)
                classeDruida.AddComponent<HudDruida>();
        }

        classeDruida.SetActive(true);

        // Define o modo Druida no PlayerController (Passo do Cervo)
        if (controleMovimento != null) controleMovimento.DefinirClasse(PlayerController.TipoClasse.Druida);

        Debug.Log("🌿 Druida Guardião da Natureza selecionado com sucesso!");
        IniciarJogo();
    }

    // --- LIBERA O JOGO ---
    void IniciarJogo()
    {
        jogoIniciado = true;
        jogoPausado = false;

        if (telaDeSelecao != null) telaDeSelecao.SetActive(false); // Some com o menu de classes
        if (hudDoJogador != null) hudDoJogador.SetActive(true);     // Mostra as barras
        if (painelMenuPause != null) painelMenuPause.SetActive(false); // Esconde o menu de pause

        Time.timeScale = 1f; // Despausa o jogo

        // Trava e esconde o mouse novamente para o estilo FPS
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (controleMovimento != null) controleMovimento.enabled = true; // Devolve o controle ao jogador
    }

    // ==================== SISTEMA DE MENU DE PAUSE (ESC) ====================

    public void AlternarPause()
    {
        if (jogoPausado)
        {
            ContinuarJogo();
        }
        else
        {
            PausarJogo();
        }
    }

    public void PausarJogo()
    {
        jogoPausado = true;
        Time.timeScale = 0f; // Congela a simulação

        // Libera o mouse para interagir com o menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (controleMovimento != null) controleMovimento.enabled = false;

        // Cria o menu de pause se ainda não existir
        if (painelMenuPause == null)
        {
            ConstruirMenuPauseProcedural();
        }

        if (painelMenuPause != null)
        {
            painelMenuPause.transform.SetAsLastSibling(); // Renderiza à frente de todos os elementos
            painelMenuPause.SetActive(true);
        }

        Debug.Log("⏸️ Jogo Pausado!");
    }

    public void ContinuarJogo()
    {
        jogoPausado = false;

        if (painelMenuPause != null)
        {
            painelMenuPause.SetActive(false);
        }

        Time.timeScale = 1f; // Descongela a simulação

        // Trava o mouse no modo FPS
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (controleMovimento != null) controleMovimento.enabled = true;

        Debug.Log("▶️ Jogo Despausado / Retomado!");
    }

    public void TrocarClasse()
    {
        Time.timeScale = 1f; // Garante que a escala de tempo volte a 1 ao recarregar a cena
        Debug.Log("🔄 Reiniciando cenário para troca de classe...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SairDoJogo()
    {
        Debug.Log("🚪 Saindo do Jogo...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    Canvas ObterCanvasPrincipal()
    {
        // 1. Tenta pegar o Canvas pai do menu de seleção ou do HUD do jogador
        if (telaDeSelecao != null)
        {
            Canvas c = telaDeSelecao.GetComponentInParent<Canvas>();
            if (c != null && c.renderMode != RenderMode.WorldSpace) return c;
        }

        if (hudDoJogador != null)
        {
            Canvas c = hudDoJogador.GetComponentInParent<Canvas>();
            if (c != null && c.renderMode != RenderMode.WorldSpace) return c;
        }

        // 2. Busca entre todos os Canvas da cena um que seja de tela cheia (Overlay ou ScreenSpaceCamera)
        Canvas[] todos = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (var c in todos)
        {
            if (c.renderMode != RenderMode.WorldSpace) return c;
        }

        return todos.Length > 0 ? todos[0] : null;
    }

    // --- CONSTRUÇÃO PROCEDURAL DO MENU DE PAUSE ---
    void ConstruirMenuPauseProcedural()
    {
        Canvas canvas = ObterCanvasPrincipal();
        if (canvas == null) return;

        // 1. Fundo Escurecido de Tela Cheia (Backdrop)
        painelMenuPause = new GameObject("PainelMenuPause");
        painelMenuPause.transform.SetParent(canvas.transform, false);
        painelMenuPause.transform.SetAsLastSibling();

        RectTransform rectBackdrop = painelMenuPause.AddComponent<RectTransform>();
        rectBackdrop.anchorMin = Vector2.zero;
        rectBackdrop.anchorMax = Vector2.one;
        rectBackdrop.offsetMin = Vector2.zero;
        rectBackdrop.offsetMax = Vector2.zero;

        Image imgBackdrop = painelMenuPause.AddComponent<Image>();
        imgBackdrop.color = new Color(0.03f, 0.05f, 0.08f, 0.90f); // Dark glass overlay

        // 2. Modal Central Card
        GameObject modalCard = new GameObject("ModalCard");
        modalCard.transform.SetParent(painelMenuPause.transform, false);

        RectTransform rectCard = modalCard.AddComponent<RectTransform>();
        rectCard.anchorMin = new Vector2(0.5f, 0.5f);
        rectCard.anchorMax = new Vector2(0.5f, 0.5f);
        rectCard.pivot = new Vector2(0.5f, 0.5f);
        rectCard.anchoredPosition = Vector2.zero;
        rectCard.sizeDelta = new Vector2(360f, 380f);

        Image imgCard = modalCard.AddComponent<Image>();
        imgCard.color = new Color(0.08f, 0.11f, 0.15f, 0.98f); // Slate Card

        // 3. Título do Menu
        GameObject objTitulo = new GameObject("TextoTituloPause");
        objTitulo.transform.SetParent(modalCard.transform, false);

        RectTransform rectTitulo = objTitulo.AddComponent<RectTransform>();
        rectTitulo.anchorMin = new Vector2(0f, 0.78f);
        rectTitulo.anchorMax = new Vector2(1f, 1f);
        rectTitulo.offsetMin = new Vector2(15f, 0f);
        rectTitulo.offsetMax = new Vector2(-15f, -10f);

        TextMeshProUGUI tmpTitulo = objTitulo.AddComponent<TextMeshProUGUI>();
        tmpTitulo.text = "⏸️ JOGO PAUSADO";
        tmpTitulo.fontSize = 24;
        tmpTitulo.fontStyle = FontStyles.Bold;
        tmpTitulo.alignment = TextAlignmentOptions.Center;
        tmpTitulo.color = new Color(1f, 0.85f, 0.35f); // Dourado brilhante

        // 4. Linha Divisória Decorativa
        GameObject linhaDiv = new GameObject("LinhaDivisoria");
        linhaDiv.transform.SetParent(modalCard.transform, false);
        RectTransform rectLinha = linhaDiv.AddComponent<RectTransform>();
        rectLinha.anchorMin = new Vector2(0.1f, 0.78f);
        rectLinha.anchorMax = new Vector2(0.9f, 0.78f);
        rectLinha.sizeDelta = new Vector2(0f, 2f);
        Image imgLinha = linhaDiv.AddComponent<Image>();
        imgLinha.color = new Color(1f, 0.85f, 0.35f, 0.35f);

        // 5. Botões de Ação
        CriarBotaoPause(modalCard.transform, "BtnContinuar", "CONTINUAR", 0.55f, new Color(0.16f, 0.52f, 0.28f), ContinuarJogo);
        CriarBotaoPause(modalCard.transform, "BtnTrocarClasse", "TROCAR CLASSE", 0.35f, new Color(0.20f, 0.38f, 0.65f), TrocarClasse);
        CriarBotaoPause(modalCard.transform, "BtnSair", "SAIR DO JOGO", 0.15f, new Color(0.68f, 0.20f, 0.20f), SairDoJogo);

        // Inicia desativado até ser chamado
        painelMenuPause.SetActive(false);
    }

    void CriarBotaoPause(Transform pai, string nomeObj, string texto, float ancoraY, Color corBase, UnityEngine.Events.UnityAction acao)
    {
        GameObject btnObj = new GameObject(nomeObj);
        btnObj.transform.SetParent(pai, false);

        RectTransform rectBtn = btnObj.AddComponent<RectTransform>();
        rectBtn.anchorMin = new Vector2(0.1f, ancoraY - 0.08f);
        rectBtn.anchorMax = new Vector2(0.9f, ancoraY + 0.08f);
        rectBtn.offsetMin = Vector2.zero;
        rectBtn.offsetMax = Vector2.zero;

        Image imgBtn = btnObj.AddComponent<Image>();
        imgBtn.color = corBase;

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = corBase;
        colors.highlightedColor = corBase * 1.25f;
        colors.pressedColor = corBase * 0.80f;
        colors.selectedColor = corBase;
        btn.colors = colors;
        btn.onClick.AddListener(acao);

        // Texto do Botão
        GameObject textoObj = new GameObject("Texto");
        textoObj.transform.SetParent(btnObj.transform, false);

        RectTransform rectTexto = textoObj.AddComponent<RectTransform>();
        rectTexto.anchorMin = Vector2.zero;
        rectTexto.anchorMax = Vector2.one;
        rectTexto.offsetMin = Vector2.zero;
        rectTexto.offsetMax = Vector2.zero;

        TextMeshProUGUI tmpTexto = textoObj.AddComponent<TextMeshProUGUI>();
        tmpTexto.text = texto;
        tmpTexto.fontSize = 16;
        tmpTexto.fontStyle = FontStyles.Bold;
        tmpTexto.alignment = TextAlignmentOptions.Center;
        tmpTexto.color = Color.white;
    }
}

