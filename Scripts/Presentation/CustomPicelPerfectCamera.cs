using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class IndependentPixelPerfectCamera : MonoBehaviour
{
    [Header("Pixel Perfect Settings")]
    public int referenceResolutionX = 320;
    public int referenceResolutionY = 180;
    public int pixelsPerUnit = 100;
    public bool snapToPixelGrid = true;

    [Header("Frame Options")]
    public bool cropX = false;
    public bool cropY = false;
    public bool stretchFill = false;

    private Camera cam;

    void OnEnable()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true; // 필수
    }

    void LateUpdate()
    {
        if (snapToPixelGrid)
        {
            float unitsPerPixel = 1f / pixelsPerUnit;
            Vector3 pos = cam.transform.position;
            pos.x = Mathf.Round(pos.x / unitsPerPixel) * unitsPerPixel;
            pos.y = Mathf.Round(pos.y / unitsPerPixel) * unitsPerPixel;
            cam.transform.position = pos;
        }
    }

    void OnPreCull()
    {
        // 화면 비율에 맞는 스케일 계산
        float scaleX = Mathf.Floor(Screen.width / (float)referenceResolutionX);
        float scaleY = Mathf.Floor(Screen.height / (float)referenceResolutionY);
        float scale = 1f;

        if (stretchFill)
        {
            // 화면 전체를 채움
            scale = Mathf.Max(scaleX, scaleY);
        }
        else
        {
            // Crop 옵션 조합
            if (cropX && cropY)
                scale = Mathf.Min(scaleX, scaleY); // 비율 유지하며 Crop
            else if (cropX)
                scale = scaleY; // 세로 해상도 고정, 가로 잘림
            else if (cropY)
                scale = scaleX; // 가로 해상도 고정, 세로 잘림
            else
                scale = Mathf.Min(scaleX, scaleY); // 비율 유지, 여백 가능
        }

        scale = Mathf.Max(1f, scale); // 최소 1배율

        // GL.Viewport로 Letterbox/Pillarbox 처리
        int viewportWidth = Mathf.RoundToInt(referenceResolutionX * scale);
        int viewportHeight = Mathf.RoundToInt(referenceResolutionY * scale);
        int viewportX = (Screen.width - viewportWidth) / 2;
        int viewportY = (Screen.height - viewportHeight) / 2;

        GL.Viewport(new Rect(viewportX, viewportY, viewportWidth, viewportHeight));
    }
}
