using UnityEngine;

public class ToggleHighlight : MonoBehaviour
{
    [SerializeField] public Material highlightMaterial;
    
    [SerializeField] private Texture2D texture;
    [SerializeField] private Material originalMaterial;
    [SerializeField] private Material highlightCopy;
    [SerializeField] public float timer = 9999999;
    MeshRenderer rend;
    void Start()
    {
        if (TryGetComponent(out rend))
        {
            originalMaterial = rend.material;
            texture = (Texture2D) originalMaterial.mainTexture;
        
            highlightCopy = new Material(highlightMaterial);
            highlightCopy.SetTexture("_Texture", texture);
        }
        

        for (int i = 0; i < transform.childCount; ++i)
        {
            ToggleHighlight script;
            if(!transform.GetChild(i).gameObject.TryGetComponent(out script))
            {
                script = transform.GetChild(i).gameObject.AddComponent<ToggleHighlight>();
            }
            script.highlightMaterial = highlightMaterial;
        }
        Toggle(true,timer);
        
    }

    public void Toggle(bool _bool, float _timer)
    {
        timer = _timer;
        if (_bool && rend != null)
        {
            rend.material = highlightCopy;
        }
        else if (rend != null)
        {
            rend.material = originalMaterial;
        }
        
        for (int i = 0; i < transform.childCount; ++i)
        {
            ToggleHighlight script;
            if(!transform.GetChild(i).gameObject.TryGetComponent(out script))
            {
                script = transform.GetChild(i).gameObject.AddComponent<ToggleHighlight>();
            }
            script.Toggle(_bool,_timer);
        }
        
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer < 0)
        {
            timer = 99999999;
            Toggle(false,timer);
        }
    }
}
