using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Extension_ChildTagHelper {
    public static List<T> FindComponentsInChildWithTag<T>(this GameObject parent, string tag) where T : Component {
        Transform t = parent.transform;
        List<T> componentList = new List<T>();

        foreach (Transform tr in t) {
            if (tr.tag == tag) {
                componentList.Add(tr.GetComponent<T>());
            }
        }
        return componentList;
    }
}
