using TMPro;
using UnityEngine;

public class RaycastInfo : MonoBehaviour
{
    [Header("TMP Raycast")]
    public TextMeshProUGUI tmp;

    [Space(10)]
    [Header("TMP Info Panel")]
    public GameObject objInfoDebug;

    [Space(10)]
    [Header("Material")]
    public Material highlightMaterial;

    [Space(10)]
    [SerializeField]
    private Material originalMaterial;

    [Space(10)]
    [SerializeField]
    private GameObject lastHitGameObject; 

    void Update()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit) && hit.distance > 0f && hit.distance < 9f)
        {
            GameObject hitObject = hit.collider.gameObject;

            if (hit.collider.CompareTag("Plane"))
            {
                return;
            }
             
            HighlightObject(hitObject);

            if (hit.collider.GetComponent<TextMeshProUGUI>() != null)
            {
                objInfoDebug.SetActive(true);
                tmp.text = hit.collider.GetComponent<TextMeshProUGUI>().text;
            }
            else
            {
                objInfoDebug.SetActive(true);
                tmp.text = hitObject.name;
            }
        }
        else
        {
            objInfoDebug.SetActive(false);
            UnhighlightLastObject();
        }
    }
    
    private void HighlightObject(GameObject obj)
    {
        if (lastHitGameObject != obj)
        {
            UnhighlightLastObject();

            lastHitGameObject = obj;
            if (obj.GetComponent<Renderer>())
            {
                originalMaterial = obj.GetComponent<Renderer>().material;
                obj.GetComponent<Renderer>().material = highlightMaterial;
            }
        }
    }

    private void UnhighlightLastObject()
    {
        if (lastHitGameObject != null && originalMaterial != null)
        {
            if (lastHitGameObject.GetComponent<Renderer>())
            {
                lastHitGameObject.GetComponent<Renderer>().material = originalMaterial;
            }

            lastHitGameObject = null;
        }
    }

    private void OnDestroy()
    {
        UnhighlightLastObject();
    }
}
