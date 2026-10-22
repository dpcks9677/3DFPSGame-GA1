using UnityEngine;
using System.Collections.Generic;

public class ListTest : MonoBehaviour
{
    private void Start()
    {
        int[] ages = new int[] { 22, 21, 20, 35 };

        ages[0] = 17;
        ages[1] = 40;
        ages[2] = 27;
        ages[3] = 32;

        List<int> ages2 = new List<int>() { 2, 21, 20, 35 };

        ages2[0] = 17;
        ages2[1] = 40;
        ages2[2] = 27;
        ages2[3] = 32;

        ages2.Add(24);
        ages2.Insert(1, 12);
        ages2.Remove(40);
        ages2.RemoveAt(3);

        foreach (int age in ages2)
        {
            Debug.Log(age);
        }
    }

    private void Update()
    {

    }
}
