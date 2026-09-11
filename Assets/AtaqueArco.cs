using System.Collections;
using UnityEngine;

public class AtaqueArco : MonoBehaviour
{
    [Header("Configurações de Disparo")]
    public GameObject prefabFlecha;
    public Transform pontoDisparo;
    public Camera cameraJogador;
    public float tempoEntreDisparos = 0.33f; // ~3 flechas por segundo
    public float velocidadeFlecha = 50f;

    [Header("Efeito Visual de Recuo")]
    public float forcaRecuo = 0.08f;
    public float tempoRecuo = 0.1f;

    private float proximoDisparoPermitido = 0f;
    private Vector3 posicaoOriginal;
    private bool recuando = false;
    private Collider playerCollider;

    void Start()
    {
        posicaoOriginal = transform.localPosition;
        if (cameraJogador == null)
        {
            cameraJogador = Camera.main;
        }

        // Obtém o colisor do jogador para ignorar colisões com as próprias flechas
        Transform playerRoot = transform.root;
        if (playerRoot != null)
        {
            playerCollider = playerRoot.GetComponent<Collider>();
            if (playerCollider == null)
            {
                playerCollider = playerRoot.GetComponent<CharacterController>();
            }
        }
    }

    void Update()
    {
        // Atira continuamente enquanto segurar o botão esquerdo do mouse (LMB)
        bool segurandoCtrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        bool querAtirar = (Input.GetMouseButton(0) || (!segurandoCtrl && Input.GetButton("Fire1"))) && !segurandoCtrl;

        if (querAtirar && Time.time >= proximoDisparoPermitido)
        {
            proximoDisparoPermitido = Time.time + tempoEntreDisparos;
            DispararFlecha();

            if (!recuando)
            {
                StartCoroutine(AnimarRecuo());
            }
        }
    }

    void DispararFlecha()
    {
        if (prefabFlecha == null || cameraJogador == null) return;

        // 1. Raycast a partir do centro da tela para saber onde a mira está apontando
        Ray ray = cameraJogador.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit[] hits = Physics.RaycastAll(ray, 300f);

        Vector3 pontoAlvo = ray.origin + ray.direction * 300f; // Padrão: horizonte aberto (céu / espaço aberto)

        // Encontra o primeiro objeto atingido que NÃO seja o próprio jogador
        float menorDistancia = float.MaxValue;
        foreach (var hit in hits)
        {
            // Ignora o jogador e os filhos do jogador
            if (hit.transform.root == transform.root) continue;
            if (hit.collider.isTrigger) continue;

            if (hit.distance < menorDistancia && hit.distance > 0.5f)
            {
                menorDistancia = hit.distance;
                pontoAlvo = hit.point;
            }
        }

        // 2. Ponto de saída da flecha (ligeiramente à frente da câmera)
        Vector3 posicaoSaida = cameraJogador.transform.position 
                             + cameraJogador.transform.forward * 0.8f 
                             + cameraJogador.transform.right * 0.25f 
                             - cameraJogador.transform.up * 0.15f;

        if (pontoDisparo != null)
        {
            posicaoSaida = pontoDisparo.position + cameraJogador.transform.forward * 0.2f;
        }

        // 3. Direção em direção ao ponto de mira
        Vector3 direcao = (pontoAlvo - posicaoSaida).normalized;

        // 4. Instancia a flecha
        GameObject flechaObj = Instantiate(prefabFlecha, posicaoSaida, Quaternion.LookRotation(direcao));

        // 5. Ignora colisão física com o jogador
        Collider flechaCol = flechaObj.GetComponent<Collider>();
        if (flechaCol != null && playerCollider != null)
        {
            Physics.IgnoreCollision(flechaCol, playerCollider, true);
        }

        // 6. Aplica velocidade na flecha
        Rigidbody rb = flechaObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.linearVelocity = direcao * velocidadeFlecha;
        }
    }

    IEnumerator AnimarRecuo()
    {
        recuando = true;
        Vector3 posicaoRecuo = posicaoOriginal - Vector3.forward * forcaRecuo;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / (tempoRecuo / 2);
            transform.localPosition = Vector3.Lerp(posicaoOriginal, posicaoRecuo, t);
            yield return null;
        }

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / (tempoRecuo / 2);
            transform.localPosition = Vector3.Lerp(posicaoRecuo, posicaoOriginal, t);
            yield return null;
        }

        transform.localPosition = posicaoOriginal;
        recuando = false;
    }
}
