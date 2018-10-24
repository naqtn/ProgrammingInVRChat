using UnityEngine;

/*
 * For Test/Research/Investigate/spike etc.
 */
public class SpikeComponent : MonoBehaviour
{
    public string debugString;

    void OnEnable()
    {
        Debug.Log("SpikeComponent:OnEnable:" + debugString);
    }

    void Start()
    {
        Debug.Log("SpikeComponent:Start:" + debugString);
    }

}
