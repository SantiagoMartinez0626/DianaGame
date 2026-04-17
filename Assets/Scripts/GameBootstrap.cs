using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// Construye la escena de juego al iniciar: placeholders (sprites generados), diana, arco, UI y referencias.
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    [SerializeField] bool buildOnAwake = true;
    bool _built;

    [Header("Fondo")]
    [Tooltip("Arrastra aquí el sprite importado desde Assets/Sprites/background.jpg (Texture Type: Sprite).")]
    [SerializeField] Sprite backgroundSprite;

    [SerializeField] [Range(1f, 1.1f)] float backgroundCoverPadding = 1.02f;

    [Header("Sprites de gameplay")]
    [Tooltip("Sprite de la diana completa (ej. Assets/Sprites/target.jpg).")]
    [SerializeField] Sprite targetSprite;
    [Tooltip("Sprite del arco (opcional). Si no existe, usa placeholder.")]
    [SerializeField] Sprite bowSprite;
    [SerializeField] float targetVisualScale = 1.35f;
    [Tooltip("Radio del centro que suma puntos, como fracción del semirradio de la cara (medido en target.jpg: oro ~87px / 256px).")]
    [SerializeField] [Range(0.08f, 0.55f)] float bullseyeRadiusFractionOfFaceHalf = 87f / 256f;
    [SerializeField] float bowVisualScale = 1.15f;
    [Tooltip("Sprite visual de la flecha disparada (ej. Assets/Sprites/arrow.png).")]
    [SerializeField] Sprite arrowShotSprite;
    [SerializeField] Vector3 arrowShotScale = new Vector3(0.48f, 0.48f, 1f);

    void Awake()
    {
        if (buildOnAwake)
            Build();
    }

    void Build()
    {
        if (_built)
            return;

        GameObject arrowTemplate = Resources.Load<GameObject>("Arrow");
        if (arrowTemplate == null)
        {
            Debug.LogError("Falta el prefab Resources/Arrow. Coloca Assets/Resources/Arrow.prefab.");
            return;
        }

        _built = true;

        var cam = Camera.main;
        if (cam != null)
        {
            cam.orthographicSize = 6f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.backgroundColor = new Color(0.15f, 0.2f, 0.28f);
        }

        // --- Fondo (imagen en Assets/Sprites o placeholder)
        var bg = new GameObject("Background");
        var bgSr = bg.AddComponent<SpriteRenderer>();
        bgSr.sortingOrder = -20;
        if (backgroundSprite != null)
        {
            bgSr.sprite = backgroundSprite;
            bgSr.color = Color.white;
            FitSpriteToOrthographicCamera(bgSr, cam, backgroundCoverPadding);
        }
        else
        {
            bgSr.sprite = MakeSpriteSquare(new Color(0.25f, 0.45f, 0.28f));
            bg.transform.localScale = new Vector3(24f, 16f, 1f);
        }

        // --- Diana
        var target = new GameObject("Target");
        target.transform.position = Vector3.zero;
        target.AddComponent<TargetMovement>().Configure(3.5f, 4.5f);

        // Anillo exterior (visual + trigger)
        var outer = new GameObject("OuterRing");
        outer.transform.SetParent(target.transform, false);
        outer.tag = "OuterRing";
        var outerSr = outer.AddComponent<SpriteRenderer>();
        outerSr.sprite = targetSprite != null ? targetSprite : MakeSpriteCircle(new Color(0.85f, 0.2f, 0.15f));
        outerSr.sortingOrder = 0;
        outer.transform.localScale = targetSprite != null ? Vector3.one * targetVisualScale : Vector3.one * 1.6f;
        var outerCol = outer.AddComponent<CircleCollider2D>();
        outerCol.isTrigger = true;
        if (targetSprite != null)
            outerCol.radius = Mathf.Max(outerSr.sprite.bounds.extents.x, 0.06f);
        else
            outerCol.radius = 0.5f;

        // Centro (bullseye): con textura, el collider sigue el círculo amarillo del arte (ver bullseyeRadiusFractionOfFaceHalf).
        var bull = new GameObject("Bullseye");
        bull.transform.SetParent(target.transform, false);
        bull.tag = "Bullseye";
        var bullSr = bull.AddComponent<SpriteRenderer>();
        bullSr.sprite = MakeSpriteCircle(new Color(1f, 0.92f, 0.2f));
        bullSr.sortingOrder = 1;
        var bullCol = bull.AddComponent<CircleCollider2D>();
        bullCol.isTrigger = true;
        float faceHalfWorld = outerCol.radius * Mathf.Abs(outer.transform.lossyScale.x);
        float bullFrac = targetSprite != null ? bullseyeRadiusFractionOfFaceHalf : 0.28f;
        if (targetSprite != null)
        {
            bull.transform.localScale = Vector3.one;
            bullCol.radius = Mathf.Max(faceHalfWorld * bullFrac, 0.04f);
            bullSr.enabled = false;
        }
        else
        {
            bull.transform.localScale = Vector3.one * 0.35f;
            float worldR = Mathf.Max(faceHalfWorld * bullFrac, 0.06f);
            bullCol.radius = worldR / Mathf.Max(Mathf.Abs(bull.transform.lossyScale.x), 1e-4f);
            bullSr.enabled = true;
        }

        // --- Arco / disparo
        var bow = new GameObject("Bow");
        bow.transform.position = new Vector3(-7.5f, -2.2f, 0f);
        var shoot = new GameObject("ShootPoint");
        shoot.transform.SetParent(bow.transform, false);
        shoot.transform.localPosition = new Vector3(0.4f, 0.35f, 0f);
        var bowSr = bow.AddComponent<SpriteRenderer>();
        bowSr.sprite = bowSprite != null ? bowSprite : MakeSpriteSquare(new Color(0.45f, 0.28f, 0.12f));
        bowSr.sortingOrder = 2;
        bow.transform.localScale = bowSprite != null ? new Vector3(bowVisualScale, bowVisualScale, 1f) : new Vector3(0.5f, 0.8f, 1f);

        var bowCtrl = bow.AddComponent<BowController>();

        // --- UI
        var canvasGo = new GameObject("Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGo.AddComponent<GraphicRaycaster>();

        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>();
        }

        Font uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (uiFont == null)
            uiFont = Font.CreateDynamicFontFromOSFont("Liberation Sans", 32);

        var scoreGo = new GameObject("ScoreText");
        scoreGo.transform.SetParent(canvasGo.transform, false);
        var scoreRt = scoreGo.AddComponent<RectTransform>();
        scoreRt.anchorMin = new Vector2(0, 1);
        scoreRt.anchorMax = new Vector2(0, 1);
        scoreRt.pivot = new Vector2(0, 1);
        scoreRt.anchoredPosition = new Vector2(40, -40);
        scoreRt.sizeDelta = new Vector2(420, 80);
        var scoreText = scoreGo.AddComponent<Text>();
        scoreText.font = uiFont;
        scoreText.fontSize = 36;
        scoreText.color = Color.white;
        scoreText.text = "Puntos: 0";

        var fbGo = new GameObject("FeedbackText");
        fbGo.transform.SetParent(canvasGo.transform, false);
        var fbRt = fbGo.AddComponent<RectTransform>();
        fbRt.anchorMin = new Vector2(0.5f, 0.5f);
        fbRt.anchorMax = new Vector2(0.5f, 0.5f);
        fbRt.pivot = new Vector2(0.5f, 0.5f);
        fbRt.anchoredPosition = Vector2.zero;
        fbRt.sizeDelta = new Vector2(600, 100);
        var fbText = fbGo.AddComponent<Text>();
        fbText.font = uiFont;
        fbText.fontSize = 42;
        fbText.alignment = TextAnchor.MiddleCenter;
        fbText.color = new Color(1f, 0.4f, 0.35f);
        fbText.gameObject.SetActive(false);

        Sprite uiSprite = MakeUiSprite();
        var barGo = new GameObject("PowerBar");
        barGo.transform.SetParent(canvasGo.transform, false);
        var barRt = barGo.AddComponent<RectTransform>();
        barRt.anchorMin = new Vector2(0.5f, 0);
        barRt.anchorMax = new Vector2(0.5f, 0);
        barRt.pivot = new Vector2(0.5f, 0);
        barRt.anchoredPosition = new Vector2(0, 80);
        barRt.sizeDelta = new Vector2(500, 36);
        var barBg = barGo.AddComponent<Image>();
        barBg.sprite = uiSprite;
        barBg.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

        var fillGo = new GameObject("Fill");
        fillGo.transform.SetParent(barGo.transform, false);
        var fillRt = fillGo.AddComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;
        var fillImg = fillGo.AddComponent<Image>();
        fillImg.sprite = uiSprite;
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImg.color = new Color(0.2f, 0.85f, 0.35f);
        fillImg.fillAmount = 0f;

        var mgr = new GameObject("Managers");
        var sm = mgr.AddComponent<ScoreManager>();
        sm.SetScoreText(scoreText);
        var gf = mgr.AddComponent<GameFeedback>();
        gf.SetFeedbackText(fbText);

        bowCtrl.Wire(arrowTemplate, shoot.transform, fillImg, bullCol);
        bowCtrl.SetShotTuning(16f, 14f, 40f);
        if (arrowShotSprite != null)
            bowCtrl.SetArrowVisual(arrowShotSprite, arrowShotScale);

        // Zona inferior: elimina flechas que caen
        var kill = new GameObject("KillZone");
        kill.tag = "KillZone";
        kill.transform.position = new Vector3(0f, -8f, 0f);
        var kc = kill.AddComponent<BoxCollider2D>();
        kc.isTrigger = true;
        kc.size = new Vector2(40f, 2f);
    }

    /// <summary>
    /// Escala el sprite para cubrir el rectángulo visible de la cámara ortográfica (modo “cover”).
    /// </summary>
    static void FitSpriteToOrthographicCamera(SpriteRenderer sr, Camera cam, float padding)
    {
        if (sr == null || sr.sprite == null)
            return;
        if (cam == null || !cam.orthographic)
        {
            sr.transform.localScale = Vector3.one * 12f;
            return;
        }

        float camH = cam.orthographicSize * 2f;
        float camW = camH * cam.aspect;
        var b = sr.sprite.bounds;
        float sx = camW / b.size.x;
        float sy = camH / b.size.y;
        float s = Mathf.Max(sx, sy) * padding;
        sr.transform.localScale = new Vector3(s, s, 1f);
        var p = cam.transform.position;
        sr.transform.position = new Vector3(p.x, p.y, 0f);
    }

    static Sprite MakeSpriteSquare(Color c)
    {
        var t = new Texture2D(4, 4);
        var arr = new Color[16];
        for (int i = 0; i < 16; i++)
            arr[i] = c;
        t.SetPixels(arr);
        t.Apply();
        return Sprite.Create(t, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
    }

    static Sprite MakeSpriteCircle(Color fill)
    {
        const int size = 64;
        var t = new Texture2D(size, size);
        float r = size / 2f - 0.5f;
        var c = new Vector2(r, r);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), c);
                t.SetPixel(x, y, d <= r ? fill : Color.clear);
            }
        }

        t.Apply();
        return Sprite.Create(t, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size / 2f);
    }

    static Sprite MakeUiSprite()
    {
        var t = new Texture2D(4, 4);
        var px = new Color[16];
        for (int i = 0; i < 16; i++)
            px[i] = Color.white;
        t.SetPixels(px);
        t.Apply();
        return Sprite.Create(t, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 100f);
    }
}
