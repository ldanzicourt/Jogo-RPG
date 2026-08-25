using UnityEngine;
using UnityEngine.UI;

public class StatusJogador : MonoBehaviour
{
    [Header("Vida")]
    public float vidaMaxima = 100f;
    private float vidaAtual;
    public Image imagemBarraVida;

    [Header("Fúria")]
    public float furiaMaxima = 100f;
    private float furiaAtual = 0f;
    public Image imagemBarraFuria;

    [Header("Decaimento de Fúria")]
    public float tempoParaComecarPerder = 3f; // Segundos sem bater antes de perder Fúria
    public float perdaDeFuriaPorSegundo = 15f;
    private float tempoUltimoAcerto;

    void Start()
    {
        vidaAtual = vidaMaxima;
        furiaAtual = 0f; // Bárbaro começa calmo
        AtualizarBarras();
    }

    void Update()
    {
        // Se a Fúria for maior que zero E já passou o tempo desde o último acerto...
        if (furiaAtual > 0 && Time.time > tempoUltimoAcerto + tempoParaComecarPerder)
        {
            // Começa a esvaziar a barra gradativamente
            furiaAtual -= perdaDeFuriaPorSegundo * Time.deltaTime;

            // Impede que fique negativa
            if (furiaAtual < 0) furiaAtual = 0;

            AtualizarBarras();
        }
    }

    public void GanharFuria(float quantidade)
    {
        furiaAtual += quantidade;

        // Trava no máximo
        if (furiaAtual > furiaMaxima) furiaAtual = furiaMaxima;

        // Reseta o cronômetro para não perder fúria imediatamente
        tempoUltimoAcerto = Time.time;

        AtualizarBarras();
    }

    public void ReceberDano(float quantidade)
    {
        vidaAtual -= quantidade;

        // Bárbaro também ganha um pouquinho de fúria ao apanhar (opcional, como no seu design!)
        GanharFuria(quantidade / 2);

        if (vidaAtual <= 0)
        {
            Debug.Log("O Bárbaro caiu!");
            // Futuramente colocamos o Game Over aqui
        }
        AtualizarBarras();
    }

    void AtualizarBarras()
    {
        if (imagemBarraVida != null)
            imagemBarraVida.fillAmount = vidaAtual / vidaMaxima;

        if (imagemBarraFuria != null)
            imagemBarraFuria.fillAmount = furiaAtual / furiaMaxima;
    }
}