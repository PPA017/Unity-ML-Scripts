using UnityEngine;

public class BrakeLight : MonoBehaviour
{
    public Material brakeLightMaterial;
    public Color defaultEmissionColor = Color.red;
    public Color brakeOffColor = Color.black;
    public CarAgent carAgent;
    void Update()
    {
        if (carAgent.brakeInput > 0.1f)
        {
            brakeLightMaterial.SetColor("_EmissionColor", defaultEmissionColor * 50f);
        }
        else
        {
            brakeLightMaterial.SetColor("_EmissionColor", brakeOffColor * 50f);
        }
    }
}
