using UnityEngine;

public class FriesBag : MonoBehaviour
{
    [Header("ESTADO")]
    public bool isFilled = false;

    [Header("MODELOS")]
    public GameObject emptyModel;
    public GameObject fullModel;

    public bool IsFilled => isFilled;


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


    private void UpdateBagVisual()
    {
        if (emptyModel != null)
        {
            emptyModel.SetActive(
                !isFilled
            );
        }

        if (fullModel != null)
        {
            fullModel.SetActive(
                isFilled
            );
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