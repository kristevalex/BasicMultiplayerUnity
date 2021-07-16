using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireButton : MonoBehaviour
{
    public static bool fired = false;

    public void Pressed()
    {
        fired = true;
    }
}
