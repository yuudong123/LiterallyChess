using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SfxManager : MonoBehaviour {
    public static SfxManager I;

    [Header("출력(선택)")]
    public AudioMixerGroup output;

    [Header("클립(복수 등록 시 랜덤)")]
    public AudioClip[] moveClips;
    public AudioClip[] captureClips;
    public AudioClip[] castleClips;
    public AudioClip[] promoteClips;
    public AudioClip[] checkClips;
    public AudioClip[] gameoverWinClips;
    public AudioClip[] gameoverDrawClips;

    [Header("풀링/볼륨")]
    [SerializeField] int initialSources = 8;
    [SerializeField, Range(0.0f, 1.0f)] float masterVolume = 1f;
    [SerializeField, Range(0f, 0.2f)] float pitchRand = 0.03f;

    readonly List<AudioSource> pool = new();

    void Awake() {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        CreatePool(initialSources);
    }

    void CreatePool(int count) {
        for (int i=0; i<count; i++) {
            var src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = false;
            src.spatialBlend = 0f;
            src.outputAudioMixerGroup = output;
            pool.Add(src);
        }
    }

    AudioSource GetSource() {
        foreach (var s in pool) if (!s.isPlaying) return s;
        var add = gameObject.AddComponent<AudioSource>();
        add.playOnAwake = false; add.loop = false; add.spatialBlend = 0f; add.outputAudioMixerGroup = output;
        pool.Add(add);
        return add;
    }

    void PlayRandom(AudioClip[] clips, float volume = 1f) {
        if (clips == null || clips.Length == 0) return;
        var clip = clips[Random.Range(0, clips.Length)];
        var src = GetSource();
        src.pitch = 1f + Random.Range(-pitchRand, pitchRand);
        src.volume = masterVolume * volume;
        src.PlayOneShot(clip);
    }

    // 오디오 전용(기존 API 유지)
    public static void PlayMove()            => I?.PlayRandom(I.moveClips);
    public static void PlayCapture()         => I?.PlayRandom(I.captureClips);
    public static void PlayCastle()          => I?.PlayRandom(I.castleClips);
    public static void PlayPromote()         => I?.PlayRandom(I.promoteClips);
    public static void PlayCheck()           => I?.PlayRandom(I.checkClips, 0.9f);
    public static void PlayGameOverWin()     => I?.PlayRandom(I.gameoverWinClips, 1f);
    public static void PlayGameOverDraw()    => I?.PlayRandom(I.gameoverDrawClips, 1f);

    // =========================
    // SFX + 애니메이션 통합 FX
    // =========================

    // 이동 확정(승진 대기 제외): 체크 > 캡처 > 캐슬링 > 일반 이동 순서로 사운드 + 간단 애니메이션
    public static void PlayMoveFx(BoardView2D view, Vector2Int from, Vector2Int to,
                                  bool didCapture, bool didCastle, bool isCheck) {
        if (I == null || view == null) return;

        // 오디오
        if (isCheck)            PlayCheck();
        else if (didCapture)    PlayCapture();
        else if (didCastle)     PlayCastle();
        else                    PlayMove();

        // 애니메이션(간단): 목적지 말에 펀치, 캡처면 타일 플래시
        // 주의: 렌더 직후 호출 전제(ChessGame.Move에서 view.RenderAll 이후)
        view.StartCoroutine(view.PunchPiece(to, 0.13f, 0.12f));
        if (didCapture) {
            view.StartCoroutine(view.FlashTile(to, new Color(1f, 0.25f, 0.25f, 0.35f), 0.18f));
        }
        if (didCastle) {
            // 캐슬링은 왕/룩 칸을 함께 가볍게 펀치(룩 위치 추정이 어렵다면 왕만으로도 충분)
            view.StartCoroutine(view.PunchPiece(to, 0.10f, 0.10f));
        }
    }

    // 승진 확정: 승진 사운드 + 목적지 말 펀치, 체크면 체크 효과음 덧붙임
    public static void PlayPromoteFx(BoardView2D view, Vector2Int at, bool isCheckAfter = false) {
        if (I == null || view == null) return;
        PlayPromote();
        view.StartCoroutine(view.PunchPiece(at, 0.14f, 0.12f));
        if (isCheckAfter) PlayCheck();
    }

    // 게임 종료: 승리/무승부 사운드만(시각 연출은 이후 확장)
    public static void PlayGameOverFx(bool humanWinOrAnyWin, bool draw) {
        if (I == null) return;
        if (draw) PlayGameOverDraw();
        else if (humanWinOrAnyWin) PlayGameOverWin();
        else PlayGameOverDraw(); // 패배 전용 음원이 없다면 Draw 재활용
    }
}