using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public static class Util
{
    public static void Log(object message)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(message);
#endif
    }

    public static T ParseEnumFromString<T>(string value)
    {
        return (T)Enum.Parse(typeof(T), value);
    }

    public static bool SetActive(Button button, bool isOn)
    {
        return SetActive(button.gameObject, isOn);
    }

    public static bool SetActive(GameObject gameObject, bool isOn)
    {
        if (gameObject != null && gameObject.activeSelf != isOn)
        {
            gameObject.SetActive(isOn);
            return true;
        }
        return false;
    }

    public static void FlipDirectionX(GameObject gameObject)
    {
        if (gameObject.GetComponent<PolygonCollider2D>() != null)
            FlipLocalScaleX(gameObject.GetComponent<PolygonCollider2D>());
        else
        {
            FlipDirectionX(gameObject.transform);
            FlipLocalScaleX(gameObject.transform);
        }
    }

    public static void FlipDirectionX(Transform transform)
    {
        Vector3 pos = transform.localPosition;
        transform.localPosition = new Vector3(-pos.x, pos.y, pos.z);
    }

    public static void FlipLocalScaleX(GameObject gameObject)
    {
        if (gameObject.GetComponent<PolygonCollider2D>() != null)
            FlipLocalScaleX(gameObject.GetComponent<PolygonCollider2D>());
        else
            FlipLocalScaleX(gameObject.transform);
    }

    public static void FlipLocalScaleX(PolygonCollider2D polygonCollider)
    {
        if (polygonCollider != null)
        {
            Vector2[] points = polygonCollider.points;
            for (int i = 0; i < points.Length; i++)
            {
                points[i].x = -points[i].x;
            }
            polygonCollider.points = points;
        }
    }

    public static void FlipLocalScaleX(Transform transform)
    {
        Vector3 scale = transform.localScale;
        scale.x = -scale.x; // x 값의 부호 반전
        transform.localScale = scale;
    }

    public static T FindComponentInHierarchy<T>(GameObject root) where T : Component
    {
        // 현재 GameObject에서 컴포넌트 찾기
        T component = root.GetComponent<T>();
        if (component != null)
        {
            return component;
        }

        // 자식 오브젝트에서 재귀적으로 탐색
        foreach (Transform child in root.transform)
        {
            component = FindComponentInHierarchy<T>(child.gameObject);
            if (component != null)
            {
                return component;
            }
        }

        return null; // 컴포넌트를 찾지 못한 경우
    }

    public static Vector2 GetLocalSize(SpriteRenderer spriteRenderer)
    {
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            Vector2 pixelSize = spriteRenderer.sprite.rect.size; // 픽셀 크기
            float pixelsPerUnit = spriteRenderer.sprite.pixelsPerUnit; // PPU 값

            return pixelSize / pixelsPerUnit; // 로컬 크기 반환
        }

        Debug.LogError("SpriteRenderer or Sprite is null!");
        return Vector2.zero;
    }

    public static GameObject GetMonsterGameObject(Collider2D collision)
    {
        // TODO: 보스때문에 임시로 수정해놨는데 나중에 다시 수정해야함 - KMJ
        if (collision.gameObject.transform.parent == null) return collision.gameObject;
        else return collision.gameObject.transform.parent?.gameObject;
    }

    public static Vector3 GetMousePointWithPerspectiveCamera()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Mathf.Abs(Camera.main.transform.position.z);
        return Camera.main.ScreenToWorldPoint(mousePosition); ;
    }

    public static bool IsEditor => Application.isEditor;

    public static bool IsRootGameObject(GameObject gameObject)
    {
        return gameObject.transform.parent == null;
    }
}