using UnityEngine;

public class CameraController3P : MonoBehaviour
{
    [Header("Alvo")]
    public Transform alvo;
    public Vector3 offsetOmbro = new Vector3(0.55f, 1.35f, -2.8f);
    public float distanciaMinimaColisao = 0.6f;

    [Header("Sensibilidade e Limites")]
    public float sensibilidadeX = 2.5f;
    public float sensibilidadeY = 2.0f;
    public float limitePitchMin = -35f;
    public float limitePitchMax = 65f;
    public float suavizacaoRotacao = 18f;

    [Header("Colisão da Câmera")]
    public LayerMask camadasColisao = ~0; // Colide com ambiente
    public float raioEsferaColisao = 0.25f;

    private float yaw = 0f;
    private float pitch = 10f;
    private float distanciaDesejada;
    private float distanciaAtual;

    void Start()
    {
        distanciaDesejada = offsetOmbro.magnitude;
        distanciaAtual = distanciaDesejada;

        if (alvo != null)
        {
            yaw = alvo.eulerAngles.y;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (alvo == null) return;

        // Se o cursor estiver liberado (ex: UI), não gira a câmera
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            yaw += Input.GetAxis("Mouse X") * sensibilidadeX;
            pitch -= Input.GetAxis("Mouse Y") * sensibilidadeY;
            pitch = Mathf.Clamp(pitch, limitePitchMin, limitePitchMax);
        }

        // Rotação da Câmera
        Quaternion rotacaoDesejada = Quaternion.Euler(pitch, yaw, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotacaoDesejada, Time.deltaTime * suavizacaoRotacao);

        // Ponto de foco no alvo (ombros/cabeça)
        Vector3 pontoFoco = alvo.position + Vector3.up * offsetOmbro.y;

        // Posição ideal com offset lateral e para trás
        Vector3 direcaoParaTras = transform.rotation * new Vector3(offsetOmbro.x, 0f, offsetOmbro.z);
        Vector3 posicaoIdeal = pontoFoco + direcaoParaTras;

        // Prevenção de clipping com paredes/obstáculos
        Vector3 direcaoDoRaio = (posicaoIdeal - pontoFoco);
        float distanciaCalculada = direcaoDoRaio.magnitude;
        RaycastHit hit;

        if (Physics.SphereCast(pontoFoco, raioEsferaColisao, direcaoDoRaio.normalized, out hit, distanciaCalculada, camadasColisao, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform.root != alvo.root)
            {
                distanciaAtual = Mathf.Clamp(hit.distance - 0.1f, distanciaMinimaColisao, distanciaCalculada);
                transform.position = pontoFoco + direcaoDoRaio.normalized * distanciaAtual;
                return;
            }
        }

        distanciaAtual = Mathf.Lerp(distanciaAtual, distanciaCalculada, Time.deltaTime * 10f);
        transform.position = pontoFoco + direcaoDoRaio.normalized * distanciaAtual;
    }

    // Retorna a direção em que a mira em 3ª pessoa está apontando (para os golpes e magias)
    public Vector3 ObterDirecaoMira(float alcanceMax, out Vector3 pontoMira)
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, alcanceMax, camadasColisao, QueryTriggerInteraction.Ignore))
        {
            if (alvo == null || hit.transform.root != alvo.root)
            {
                pontoMira = hit.point;
                return (hit.point - (alvo != null ? alvo.position + Vector3.up : transform.position)).normalized;
            }
        }

        pontoMira = transform.position + transform.forward * alcanceMax;
        return transform.forward;
    }

    public Vector3 ObterDirecaoMira(out Vector3 pontoMira)
    {
        return ObterDirecaoMira(80f, out pontoMira);
    }
}
