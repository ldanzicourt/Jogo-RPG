using UnityEngine;
using UnityEngine.UI; // Necessário para mexer na barra de vida
using TMPro; // Necessário para o texto TextMeshPro

public class SistemaDeVida : MonoBehaviour
{
    public float vidaMaxima = 100f;
    private float vidaAtual;

    [Header("Interface")]
    public Image barraDeVida;        // A barra verde do inimigo
    public GameObject prefabDano;    // O molde do número flutuante

    [Header("Cores do Dano Flutuante")]
    public Color corDanoFuria = new Color(1f, 0.5f, 0f); // Laranja para dano dobrado (Fúria 100%)

    void Start()
    {
        vidaAtual = vidaMaxima;
        AtualizarBarra();
    }

    void Update()
    {
        // Faz o Canvas da barra de vida girar para olhar sempre para a câmera do jogador
        if (barraDeVida != null && Camera.main != null)
        {
            Transform canvas = barraDeVida.transform.parent;
            if (canvas != null)
            {
                canvas.LookAt(canvas.position + Camera.main.transform.forward);
            }
        }
    }

    public void ReceberDano(float quantidadeDano, bool golpeFuria = false)
    {
        vidaAtual -= quantidadeDano;
        AtualizarBarra();

        // 1. Instancia (cria) o número de dano acima do inimigo, levemente para o lado aleatório
        if (prefabDano != null)
        {
            Vector3 posicaoTexto = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), 1.5f, 0f);
            GameObject textoObj = Instantiate(prefabDano, posicaoTexto, Quaternion.identity);

            // 2. Configura o texto e a cor
            TextMeshPro tmp = textoObj.GetComponent<TextMeshPro>();
            if (tmp != null)
            {
                tmp.text = quantidadeDano.ToString();

                // Se foi um golpe em Fúria máxima (dano dobrado), pinta o texto de Laranja
                if (golpeFuria)
                {
                    tmp.color = corDanoFuria;
                }
            }
        }

        if (vidaAtual <= 0)
        {
            Morrer();
        }
    }

    void AtualizarBarra()
    {
        if (barraDeVida != null)
        {
            // O fillAmount vai de 0 a 1, então dividimos a vida atual pela máxima
            barraDeVida.fillAmount = vidaAtual / vidaMaxima;
        }
    }

    void Morrer()
    {
        Destroy(gameObject);
    }
}
