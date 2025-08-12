using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class FixedAspectEnforcer : MonoBehaviour {
    public Vector2 targetAspect = new Vector2(16, 9); // 원하는 비율
    public Color barColor = Color.black;

    Camera cam;

    void OnEnable() {
        cam = GetComponent<Camera>();
        Apply();
    }
    void Update() {
        Apply();
    }

    void Apply() {
        if (cam == null) cam = GetComponent<Camera>();
        float t = targetAspect.x / targetAspect.y;
        float w = (float)Screen.width / Screen.height;

        // 배경색이 바(bar)로 보입니다.
        cam.backgroundColor = barColor;

        if (w > t) {
            // 화면이 더 넓음 → 좌우 필러박스
            float width = t / w;
            cam.rect = new Rect((1f - width) * 0.5f, 0f, width, 1f);
        } else {
            // 화면이 더 높음 → 상하 레터박스
            float height = w / t;
            cam.rect = new Rect(0f, (1f - height) * 0.5f, 1f, height);
        }
    }
}