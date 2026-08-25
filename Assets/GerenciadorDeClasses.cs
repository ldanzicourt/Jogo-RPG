using UnityEngine;

public class GerenciadorDeClasses : MonoBehaviour
{
    [Header("Interface")]
    public GameObject telaDeSelecao;   // O painel com os botões
    public GameObject hudDoJogador;    // As barras de vida e fúria

    [Header("Jogador")]
    public PlayerController controleMovimento;

    [Header("Classes (Armas/Scripts)")]
    public GameObject classeGuerreiro; // A sua Espada atual
    public GameObject classeMago;      // Futuro cajado/mãos mágicas
    public GameObject classeRanger;    // Futuro arco
    public GameObject classeDruida;    // Futuras garras/magia da natureza

    void Start()
    {
        // 1. Pausa o tempo do jogo para os inimigos não atacarem
        Time.timeScale = 0f;

        // 2. Garante que o mouse esteja visível para clicar nos botões
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 3. Desativa o movimento e o HUD provisoriamente
        controleMovimento.enabled = false;
        hudDoJogador.SetActive(false);
        telaDeSelecao.SetActive(true);

        // 4. Esconde todas as armas/classes por padrão
        if (classeGuerreiro != null) classeGuerreiro.SetActive(false);
        if (classeMago != null) classeMago.SetActive(false);
        if (classeRanger != null) classeRanger.SetActive(false);
        if (classeDruida != null) classeDruida.SetActive(false);
    }

    // --- FUNÇÕES DOS BOTÕES ---

    public void EscolherGuerreiro()
    {
        if (classeGuerreiro != null) classeGuerreiro.SetActive(true);
        IniciarJogo();
    }

    public void EscolherMago()
    {
        if (classeMago != null) classeMago.SetActive(true);
        Debug.Log("Mago selecionado! (Em construção)");
        IniciarJogo();
    }

    public void EscolherRanger()
    {
        if (classeRanger != null) classeRanger.SetActive(true);
        Debug.Log("Ranger selecionado! (Em construção)");
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
        telaDeSelecao.SetActive(false); // Some com o menu
        hudDoJogador.SetActive(true);   // Mostra as barras de vida/fúria

        Time.timeScale = 1f; // Despausa o jogo

        // Trava e esconde o mouse novamente para o estilo FPS
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        controleMovimento.enabled = true; // Devolve o controle ao jogador
    }
}