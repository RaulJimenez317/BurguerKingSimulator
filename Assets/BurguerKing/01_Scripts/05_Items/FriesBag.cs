using UnityEngine;

public class FriesBag : MonoBehaviour
{
    public bool isFilled = false;

    private Renderer bagRenderer;
    private Color originalColor = Color.white;
    private bool hasOriginalColor = false;

    public bool IsFilled => isFilled;


    private void Awake()
    {
        FindRenderer();
        CaptureOriginalColor();
    }


    private void Start()
    {
        UpdateBagVisual();
    }


    public void FillBag()
    {
        if (isFilled)
        {
            return;
        }

        isFilled = true;

        UpdateBagVisual();

        Debug.Log(
            "🍟 Bolsa de papas llena."
        );
    }


    public void SetFilled(
        bool filled)
    {
        isFilled =
            filled;

        UpdateBagVisual();
    }


    public SaveManager.WorldItemSaveData
        CreateSaveData()
    {
        SaveManager.WorldItemSaveData data =
            new SaveManager.WorldItemSaveData();

        data.itemType =
            "Fries";

        data.objectName =
            GetCleanObjectName(
                gameObject.name
            );

        data.prefabName =
            data.objectName;

        data.active =
            gameObject.activeSelf;

        data.position =
            new SaveManager.Vector3Data(
                transform.position
            );

        data.rotation =
            new SaveManager.QuaternionData(
                transform.rotation
            );

        data.scale =
            new SaveManager.Vector3Data(
                transform.localScale
            );

        data.isFilled =
            isFilled;

        return data;
    }


    public bool RestoreFromSaveData(
        SaveManager.WorldItemSaveData data)
    {
        if (data == null)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(
            data.itemType) &&
            data.itemType != "Fries")
        {
            return false;
        }

        if (data.position != null)
        {
            transform.position =
                data.position.ToVector3();
        }

        if (data.rotation != null)
        {
            transform.rotation =
                data.rotation.ToQuaternion();
        }

        if (data.scale != null)
        {
            transform.localScale =
                data.scale.ToVector3();
        }

        FindRenderer();

        isFilled =
            data.isFilled;

        UpdateBagVisual();

        gameObject.SetActive(
            data.active
        );

        Rigidbody rb =
            GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity =
                Vector3.zero;

            rb.angularVelocity =
                Vector3.zero;
        }

        Debug.Log(
            "💾 Papas restauradas correctamente."
        );

        return true;
    }


    private void FindRenderer()
    {
        if (bagRenderer != null)
        {
            return;
        }

        bagRenderer =
            GetComponent<Renderer>();

        if (bagRenderer == null)
        {
            bagRenderer =
                GetComponentInChildren
                <Renderer>();
        }
    }


    private void CaptureOriginalColor()
    {
        if (bagRenderer == null)
        {
            return;
        }

        originalColor =
            bagRenderer.material.color;

        hasOriginalColor =
            true;
    }


    private void UpdateBagVisual()
    {
        FindRenderer();

        if (bagRenderer == null)
        {
            return;
        }

        if (isFilled)
        {
            bagRenderer.material.color =
                Color.yellow;
        }
        else if (hasOriginalColor)
        {
            bagRenderer.material.color =
                originalColor;
        }
    }


    private string GetCleanObjectName(
        string objectName)
    {
        if (string.IsNullOrEmpty(
            objectName))
        {
            return "";
        }

        return objectName
            .Replace(
                "(Clone)",
                ""
            )
            .Trim();
    }
}