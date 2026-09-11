using UnityEngine;
using UnityEngine.UI; // Necessário para mexer na barra de vida
using TMPro; // Necessário para o texto TextMeshPro

public class SistemaDeVida : MonoBehaviour
{
    public float vidaMaxima = 100f;
    public float vidaAtual;

    [Header("Interface")]
    public Image barraDeVida;        // A barra verde do inimigo
    public GameObject prefabDano;    // O molde do número flutuante

    [Header("Cores do Dano Flutuante")]
    public Color corDanoFuria = new Color(1f, 0.5f, 0f); // Laranja para dano dobrado (Fúria 100%)

    void Start()
    {
        vidaAtual = vidaMaxima;
        AtualizarBarra();

        // Configura amortecimento no Rigidbody para que os inimigos recebam impacto sem sair deslizando
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearDamping = 4.5f;
            rb.angularDamping = 3f;
        }
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

    public float ObterPorcentagemVida()
    {
        return vidaAtual / vidaMaxima;
    }

    public float ObterVidaAtual()
    {
        return vidaAtual;
    }

    public void Curar(float quantidade)
    {
        vidaAtual = Mathf.Min(vidaMaxima, vidaAtual + quantidade);
        AtualizarBarra();
    }

    public void ReceberDano(float quantidadeDano, Color corPersonalizada)
    {
        vidaAtual -= quantidadeDano;
        AtualizarBarra();

        if (prefabDano != null)
        {
            Vector3 posicaoTexto = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), 1.5f, 0f);
            GameObject textoObj = Instantiate(prefabDano, posicaoTexto, Quaternion.identity);

            TextMeshPro tmp = textoObj.GetComponent<TextMeshPro>();
            if (tmp != null)
            {
                tmp.text = Mathf.RoundToInt(quantidadeDano).ToString();
                tmp.color = corPersonalizada;
            }
        }

        if (vidaAtual <= 0)
        {
            Morrer();
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
                tmp.text = Mathf.RoundToInt(quantidadeDano).ToString();

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

    public void Desintegrar()
    {
        // Cria texto especial de desintegração
        if (prefabDano != null)
        {
            Vector3 posicaoTexto = transform.position + new Vector3(0f, 1.8f, 0f);
            GameObject textoObj = Instantiate(prefabDano, posicaoTexto, Quaternion.identity);
            TextMeshPro tmp = textoObj.GetComponent<TextMeshPro>();
            if (tmp != null)
            {
                tmp.text = "DESINTEGRADO!";
                tmp.color = new Color(0.9f, 0.2f, 1f); // Roxo fluorescente
                tmp.fontSize *= 1.3f;
            }
        }

        // Cria efeito de dissolução/poeira de partículas
        GameObject efeito = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        efeito.name = "EfeitoDesintegracao";
        efeito.transform.position = transform.position + Vector3.up * 1f;
        efeito.transform.localScale = Vector3.one * 0.8f;
        Collider col = efeito.GetComponent<Collider>();
        if (col != null) Destroy(col);

        Renderer rend = efeito.GetComponent<Renderer>();
        if (rend != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse"));
            mat.color = new Color(0.8f, 0.1f, 1f, 0.8f);
            rend.material = mat;
        }

        // Animação rápida de dissipação
        MonoBehaviour coroutineRunner = Camera.main != null ? Camera.main.GetComponent<MonoBehaviour>() : null;
        if (coroutineRunner != null)
        {
            coroutineRunner.StartCoroutine(DissiparEfeito(efeito));
        }
        else
        {
            Destroy(efeito, 1f);
        }

        Morrer();
    }

    private static System.Collections.IEnumerator DissiparEfeito(GameObject obj)
    {
        float tempo = 0f;
        Vector3 escalaInicial = obj.transform.localScale;
        while (tempo < 0.6f)
        {
            if (obj == null) yield break;
            tempo += Time.deltaTime;
            obj.transform.localScale = Vector3.Lerp(escalaInicial, Vector3.one * 2.5f, tempo / 0.6f);
            yield return null;
        }
        if (obj != null) Destroy(obj);
    }

    public void AplicarLentidao(float porcentagemLentidao, float duracao)
    {
        StartCoroutine(RotinaLentidao(porcentagemLentidao, duracao));
    }

    private System.Collections.IEnumerator RotinaLentidao(float porcentagemLentidao, float duracao)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearDamping += 2f;
        }

        yield return new WaitForSeconds(duracao);

        if (rb != null)
        {
            rb.linearDamping = Mathf.Max(0f, rb.linearDamping - 2f);
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
