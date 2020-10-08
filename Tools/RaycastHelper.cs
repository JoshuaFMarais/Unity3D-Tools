using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class RaycastHelper
{
    public RaycastHit Raycast(Vector3 start, Vector3 direction)
    {
        RaycastHit hit;
        Physics.Raycast(new Ray(start, direction), out hit, Mathf.Infinity);
        return hit;
    }
    public RaycastHit RaycastIgnorTriggers(Vector3 start, Vector3 direction,float dis)
    {
        LayerMask m = ~0;
        
        RaycastHit hit;
        Physics.Raycast(start,direction, out hit, dis, m, QueryTriggerInteraction.Ignore);
        return hit;
    }
}

