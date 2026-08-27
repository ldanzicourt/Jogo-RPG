using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum TipoClasse
    {
        Guerreiro,
        Ranger,
        Mago,
        Druida
    }

    [Header("Classe Ativa")]
    public TipoClasse classeAtual = TipoClasse.Guerreiro;

    [Header("Movimentação Base")]
    public float velocidadeAndar = 12f;
    public float gravity = -20f;
    public float jumpHeight = 2.5f;

    [Header("Habilidades do Guerreiro (Investida)")]
    public float velocidadeInvestida = 24f;    // Velocidade do atropelamento
    public float duracaoInvestida = 3f;        // Tempo máximo correndo
    public float danoBatidaParede = 50f;       // Dano ao esmagar o alvo na parede
    public float distanciaEmpurrao = 1.5f;     // Distância que o inimigo fica preso à sua frente

    [Header("Habilidades do Ranger (Corrida e Escalada)")]
    public float velocidadeSprint = 18f;       // Velocidade de corrida rápida ao segurar Shift
    public float velocidadeEscalada = 8f;      // Velocidade subindo a parede
    public float distanciaChecagemParede = 0.9f;// Distância do raio para detectar a parede

    [Header("Habilidades do Mago (Salto Etéreo)")]
    public float distanciaSaltoEtereo = 8.5f;   // Distância do teleporte astral

    [Header("Câmera")]
    public float mouseSensitivity = 300f;
    public Transform playerCamera;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float xRotation = 0f;

    // Variáveis de controle do Guerreiro (Investida)
    private bool emInvestida = false;
    private float tempoFimInvestida = 0f;
    private GameObject inimigoPrendido = null;

    // Variáveis de controle do Ranger (Escalada)
    private bool escalandoParede = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    public void DefinirClasse(TipoClasse novaClasse)
    {
        classeAtual = novaClasse;
        PararInvestida();
        escalandoParede = false;
    }

    void Update()
    {
        if (Time.timeScale <= 0.001f) return;

        // --- 1. VISÃO DO MOUSE ---
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        // --- 2. VERIFICAÇÃO DE CHÃO ---
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0 && !escalandoParede)
        {
            velocity.y = -2f;
        }

        // --- 3. CONTROLE ESPECÍFICO DE CADA CLASSE ---
        float velocidadeAtual = velocidadeAndar;
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        if (classeAtual == TipoClasse.Guerreiro)
        {
            AtaqueEspada ataqueGuerreiro = GetComponentInChildren<AtaqueEspada>();
            if (ataqueGuerreiro == null) ataqueGuerreiro = GetComponent<AtaqueEspada>();

            // Ativação da Investida do Guerreiro (Shift)
            if (Input.GetKeyDown(KeyCode.LeftShift) && !emInvestida)
            {
                emInvestida = true;
                tempoFimInvestida = Time.time + duracaoInvestida;
            }

            if (emInvestida)
            {
                velocidadeAtual = velocidadeInvestida;

                if (Time.time >= tempoFimInvestida)
                {
                    PararInvestida();
                }
                else if (inimigoPrendido != null)
                {
                    ArrastarInimigo();
                }
            }
            else if (ataqueGuerreiro != null)
            {
                if (ataqueGuerreiro.EstaEmFrenesi())
                {
                    velocidadeAtual = velocidadeAndar * 1.15f; // Leve aceleração no Frenesi
                }
                else
                {
                    if (ataqueGuerreiro.EstaEmGuardaBruta())
                    {
                        velocidadeAtual *= 0.60f; // -40% de velocidade ao erguer a Guarda Bruta
                    }

                    if (ataqueGuerreiro.EstaEmExaustao())
                    {
                        velocidadeAtual *= 0.70f; // -30% de velocidade durante a Exaustão pós-frenesi
                    }
                }
            }
        }
        else if (classeAtual == TipoClasse.Ranger)
        {
            // Ranger: Segurar Shift ativa Corrida Ágil (Sprint), enquanto o toque aciona o Gancho de Escalada
            bool segurandoShift = Input.GetKey(KeyCode.LeftShift);
            if (segurandoShift)
            {
                velocidadeAtual = velocidadeSprint;
            }
        }
        else if (classeAtual == TipoClasse.Mago)
        {
            // Mago: Shift ativa o Salto Etéreo
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                ExecutarSaltoEtereo(x, z);
            }
        }
        else if (classeAtual == TipoClasse.Druida)
        {
            // Druida: Shift ativa o Passo do Cervo com transformação em 3ª pessoa
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                ExecutarPassoDoCervo(x, z);
            }
        }

        // --- 4. MOVIMENTAÇÃO (WASD) ---
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * velocidadeAtual * Time.deltaTime);

        // --- 5. PULO / ESCALADA / GRAVIDADE ---
        if (escalandoParede && classeAtual == TipoClasse.Ranger)
        {
            // Sobe continuamente a parede sem gravidade
            velocity.y = velocidadeEscalada;
        }
        else
        {
            // Pulo padrão
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            // Aplica gravidade normal
            velocity.y += gravity * Time.deltaTime;
        }

        controller.Move(velocity * Time.deltaTime);
    }

    // Checa se há uma parede vertical na frente do Ranger
    bool ChecarParedeParaEscalada()
    {
        Vector3 origem = transform.position + Vector3.up * 0.8f;
        RaycastHit hit;

        if (Physics.Raycast(origem, transform.forward, out hit, distanciaChecagemParede))
        {
            // Se a superfície for vertical (parede), a normal do eixo Y é próxima de zero
            if (Mathf.Abs(hit.normal.y) < 0.3f && !hit.collider.isTrigger)
            {
                return true;
            }
        }
        return false;
    }

    // --- SISTEMA DE COLISÃO DO GUERREIRO ---
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (classeAtual != TipoClasse.Guerreiro || !emInvestida) return;

        // 1. CAPTURAR O INIMIGO
        if (inimigoPrendido == null && hit.gameObject.GetComponent<SistemaDeVida>() != null)
        {
            inimigoPrendido = hit.gameObject;

            Rigidbody rb = inimigoPrendido.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
        }

        // 2. ESMAGAR NA PAREDE
        if (inimigoPrendido != null && hit.gameObject != inimigoPrendido)
        {
            if (Mathf.Abs(hit.normal.y) < 0.1f)
            {
                SistemaDeVida vidaInimigo = inimigoPrendido.GetComponent<SistemaDeVida>();
                if (vidaInimigo != null)
                {
                    vidaInimigo.ReceberDano(danoBatidaParede);
                    Debug.Log("Inimigo esmagado na parede!");
                }
                PararInvestida();
            }
        }
    }

    void ArrastarInimigo()
    {
        Vector3 posicaoAlvo = transform.position + (transform.forward * distanciaEmpurrao);
        posicaoAlvo.y = inimigoPrendido.transform.position.y;
        inimigoPrendido.transform.position = posicaoAlvo;

        if (inimigoPrendido == null || inimigoPrendido.GetComponent<SistemaDeVida>() == null)
        {
            PararInvestida();
        }
    }

    void PararInvestida()
    {
        emInvestida = false;

        if (inimigoPrendido != null)
        {
            Rigidbody rb = inimigoPrendido.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = false;
            inimigoPrendido = null;
        }
    }

    // --- HABILIDADE DO MAGO: SALTO ETÉREO ---
    void ExecutarSaltoEtereo(float x, float z)
    {
        ControleMago mago = GetComponentInChildren<ControleMago>();
        if (mago == null) mago = GetComponent<ControleMago>();

        if (mago != null && !mago.PodeUsarSalto())
        {
            return;
        }

        // Determina a direção do salto (se estiver pressionando WASD, salta nessa direção; senão salta para frente)
        Vector3 direcao = (transform.right * x + transform.forward * z);
        if (direcao.sqrMagnitude < 0.01f)
        {
            direcao = transform.forward;
        }
        direcao.y = 0f;
        direcao.Normalize();

        Vector3 posicaoOrigem = transform.position;
        Vector3 posicaoDestino = posicaoOrigem + direcao * distanciaSaltoEtereo;

        // Checagem de colisão com paredes sólidas (ignora inimigos, gatilhos e projéteis)
        RaycastHit[] hits = Physics.RaycastAll(posicaoOrigem + Vector3.up * 0.5f, direcao, distanciaSaltoEtereo);
        float menorDistanciaParede = distanciaSaltoEtereo;

        foreach (var hit in hits)
        {
            if (hit.collider.isTrigger) continue;
            if (hit.transform.root == transform.root) continue;
            if (hit.collider.GetComponent<SistemaDeVida>() != null || hit.collider.GetComponentInParent<SistemaDeVida>() != null) continue; // Atravessa inimigos!

            if (hit.distance < menorDistanciaParede && hit.distance > 0.4f)
            {
                menorDistanciaParede = hit.distance;
                posicaoDestino = posicaoOrigem + direcao * (menorDistanciaParede - 0.5f);
            }
        }

        // Efeito visual no ponto de saída
        CriarEfeitoFlashAstral(posicaoOrigem + Vector3.up * 0.8f);

        // Teleporte imediato (atravessando colisores de inimigos)
        if (controller != null) controller.enabled = false;
        transform.position = posicaoDestino;
        if (controller != null) controller.enabled = true;

        // Efeito visual no ponto de chegada
        CriarEfeitoFlashAstral(posicaoDestino + Vector3.up * 0.8f);

        if (mago != null)
        {
            mago.DispararCooldownSalto();
        }
    }

    void CriarEfeitoFlashAstral(Vector3 pos)
    {
        GameObject flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flash.name = "FlashAstral";
        flash.transform.position = pos;
        flash.transform.localScale = Vector3.one * 0.4f;

        Collider c = flash.GetComponent<Collider>();
        if (c != null) Destroy(c);

        Renderer r = flash.GetComponent<Renderer>();
        if (r != null)
        {
            Material m = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse"));
            m.color = new Color(0.3f, 0.8f, 1f, 0.7f); // Azul/Ciano Astral
            r.material = m;
        }

        StartCoroutine(DesvanecerFlash(flash));
    }

    IEnumerator DesvanecerFlash(GameObject flash)
    {
        float t = 0f;
        while (t < 0.25f)
        {
            if (flash == null) yield break;
            t += Time.deltaTime;
            flash.transform.localScale = Vector3.Lerp(Vector3.one * 0.4f, Vector3.one * 1.8f, t / 0.25f);
            yield return null;
        }
        if (flash != null) Destroy(flash);
    }

    // --- MOBILIDADE DO DRUIDA (Passo do Cervo com 3ª Pessoa) ---
    private float proximoPassoCervoPermitido = 0f;
    private bool emPassoCervo = false;

    public void ExecutarPassoDoCervo(float inputX, float inputZ)
    {
        if (emPassoCervo || Time.time < proximoPassoCervoPermitido) return;

        proximoPassoCervoPermitido = Time.time + 4.0f; // 4s de recarga

        Vector3 direcao = transform.forward;
        if (Mathf.Abs(inputX) > 0.1f || Mathf.Abs(inputZ) > 0.1f)
        {
            direcao = (transform.right * inputX + transform.forward * inputZ).normalized;
        }

        StartCoroutine(RotinaSaltoDoCervo(direcao));
    }

    IEnumerator RotinaSaltoDoCervo(Vector3 direcao)
    {
        emPassoCervo = true;
        float duracaoSalto = 1.2f;

        ModeloCervoEspectral cervo = GetComponentInChildren<ModeloCervoEspectral>();
        if (cervo != null)
        {
            StartCoroutine(cervo.RotinaPassoDoCervo(duracaoSalto, this, direcao));
        }

        float t = 0f;
        while (t < duracaoSalto)
        {
            t += Time.deltaTime;
            float velocidadeSalto = velocidadeAndar * 2.5f;
            controller.Move(direcao * velocidadeSalto * Time.deltaTime);
            yield return null;
        }

        emPassoCervo = false;
    }
}
