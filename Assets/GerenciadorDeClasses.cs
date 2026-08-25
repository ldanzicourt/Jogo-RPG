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
    }

    // --- FUNÇÕES DOS BOTÕES ---

    public void EscolherGuerreiro()
    {
        // Ativa a espada do Guerreiro e esconde as outras armas
        if (classeGuerreiro != null) classeGuerreiro.SetActive(true);
        if (classeRanger != null) classeRanger.SetActive(false);
        if (classeMago != null) classeMago.SetActive(false);
        if (classeDruida != null) classeDruida.SetActive(false);

        // Mostra a barra de Fúria (exclusiva do Guerreiro)
        if (barraDeFuria != null) barraDeFuria.SetActive(true);

        // Define o modo Guerreiro no PlayerController (Investida)
        if (controleMovimento != null) controleMovimento.DefinirClasse(PlayerController.TipoClasse.Guerreiro);

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
        if (classeMago != null) classeMago.SetActive(true);
        Debug.Log("Mago selecionado! (Em construção)");
        IniciarJogo();
    }

    public void EscolherDruida()
    {
        if (classeDruida != null) classeDruida.SetActive(true);
        Debug.Log("Druida selecionado! (Em construção)");
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
