using System.Collections.Generic;
using UnityEngine;
using TMPro;
using RetroChess.Core;

public partial class BoardView2D : MonoBehaviour
{
    [Header("Board Root")]
    public Transform boardRoot;

    [Header("Tiles")]
    public Sprite tileLight;
    public Sprite tileDark;

    [Header("Pieces")]
    public Sprite wPawn, wKnight, wBishop, wRook, wQueen, wKing;
    public Sprite bPawn, bKnight, bBishop, bRook, bQueen, bKing;

    [Header("Highlights")]
    public Sprite moveDotWhite;
    public Sprite moveDotBlack;

    [Header("Hover Indicators")]
    public Sprite indicatorFront;    // 32x32, Pivot Center
    public Sprite indicatorTopBack;  // 18x5, Pivot Top Center

    [Header("Shadow")]
    public Sprite shadowSprite;

    [Header("Board Labels")]
    public Color labelColor = new Color(1f, 1f, 1f, 0.92f);
    public bool bottomIsWhite = true;
    public bool showLabels = true;
    [Range(0f, 1f)] public float fileMargin = 0.20f; // 아래(a~h)
    [Range(0f, 1f)] public float rankMargin = 0.20f; // 왼쪽(1~8)
    [Range(1f, 16f)] public float fileFontSize = 2f;
    [Range(1f, 16f)] public float rankFontSize = 2f;
    public int labelSortingOrder = 3;
    public bool labelPixelSnap = true;
    public float ppu = 16f;

    [Header("Piece Placement")]
    [Range(0f, 1f)] public float pieceBottomY01 = 0.25f; // 말 밑부분 높이(0=칸 아래, 0.5=중앙)

    [Header("Timeline Preview Overlay")]
    public bool usePreviewOverlay = true;
    public Color previewOverlayColor = new Color(1f, 1f, 1f, 0.18f); // 알파로 밝기 조절
    GameObject previewOverlayGO;

    const int N = 8;
    const float tileSize = 1f;

    // 내부 상태
    GameObject[,] pieceObjs = new GameObject[N, N];

    GameObject selectionHL;
    readonly List<GameObject> moveHL = new();
    readonly List<GameObject> capHL = new();

    GameObject checkHL;
    GameObject resultGO;
    TextMeshPro resultTMP;

    GameObject hoverFront;
    GameObject hoverTop;

    GameObject dragGO;
    Vector2Int? dragFromSq;

    GameObject labelsRoot;
    readonly List<GameObject> labelObjs = new();

    // 정렬(숫자 클수록 앞)
    int OrderPiece(int r) => 200 - r * 2;
    int OrderOverPiece(int r) => OrderPiece(r) + 1;
    int OrderUnderPiece(int r) => OrderPiece(r) - 1;

    void Awake()
    {
        if (boardRoot == null) boardRoot = transform;
    }

    public void RenderAll(Board b)
    {
        ClearBoard();
        RenderTiles();
        RenderLabels();
        RenderPieces(b);
        EnsurePreviewOverlay(); // 오버레이 오브젝트 생성(초기 비활성)
    }

    void DestroySafe(GameObject go)
    {
        if (Application.isPlaying) Destroy(go);
        else DestroyImmediate(go);
    }

    void ClearLabels()
    {
        if (labelsRoot == null) return;
        if (Application.isPlaying) Destroy(labelsRoot);
        else DestroyImmediate(labelsRoot);
        labelsRoot = null;
        labelObjs.Clear();
    }

    void ClearBoard()
    {
        if (boardRoot == null) return;
        for (int i = boardRoot.childCount - 1; i >= 0; i--)
        {
            var child = boardRoot.GetChild(i).gameObject;
            if (Application.isPlaying) Destroy(child);
            else DestroyImmediate(child);
        }
        for (int f = 0; f < N; f++) for (int r = 0; r < N; r++) pieceObjs[f, r] = null;

        selectionHL = null;
        moveHL.Clear(); capHL.Clear();

        checkHL = null;
        resultGO = null; resultTMP = null;

        hoverFront = null; hoverTop = null;

        dragGO = null; dragFromSq = null;

        labelsRoot = null; labelObjs.Clear();

        previewOverlayGO = null; // 자식 전부 파괴되므로 레퍼런스만 비움
    }
}