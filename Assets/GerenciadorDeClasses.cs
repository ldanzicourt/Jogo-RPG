using UnityEngine;

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
    public GameObject classeMago;      // Futuro cajado/mãos mágicas
    public GameObject classeRanger;    // O Arco
    public GameObject classeDruida;    // Futuras garras/magia da natureza

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

    // --- FUNÇÕES DOS BOTÕES ---

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
        // Ativa o Arco do Ranger e esconde as outras armas
        if (classeGuerreiro != null) classeGuerreiro.SetActive(false);
        if (classeRanger != null) classeRanger.SetActive(true);
        if (classeMago != null) classeMago.SetActive(false);
        if (classeDruida != null) classeDruida.SetActive(false);

        // Oculta a barra de Fúria (Ranger não usa Fúria!)
        if (barraDeFuria != null) barraDeFuria.SetActive(false);

        // Define o modo Ranger no PlayerController (Sprint + Wall Climb)
        if (controleMovimento != null) controleMovimento.DefinirClasse(PlayerController.TipoClasse.Ranger);

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
        if (telaDeSelecao != null) telaDeSelecao.SetActive(false); // Some com o menu
        if (hudDoJogador != null) hudDoJogador.SetActive(true);     // Mostra as barras

        Time.timeScale = 1f; // Despausa o jogo

        // Trava e esconde o mouse novamente para o estilo FPS
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (controleMovimento != null) controleMovimento.enabled = true; // Devolve o controle ao jogador
    }
}
