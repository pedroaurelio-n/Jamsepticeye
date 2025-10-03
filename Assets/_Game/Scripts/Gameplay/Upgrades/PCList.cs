using System.Collections.Generic;
using UnityEngine;

public class PCList : MonoBehaviour
{
    [field: SerializeField] public List<PCController> ActivePcs { get; private set; }
    
    [SerializeField] List<PCController> pcList;

    public void ActivateNewPC ()
    {
        foreach (PCController pc in pcList)
        {
            if (pc.Initialized)
                continue;
            
            pc.gameObject.SetActive(true);
            pc.Initialize(GameManager.Instance.CurrentPCCount - 1);
            ActivePcs.Add(pc);
            break;
        }
    }
}