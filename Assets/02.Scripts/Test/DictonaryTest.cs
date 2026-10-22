using System.Collections.Generic;
using UnityEngine;

public class DictonaryTest : MonoBehaviour
{
    private void Start()
    {
        List<int> ages = new List<int>() { 22, 21, 20, 35 };

        Dictionary<string, int> students = new Dictionary<string, int>() { { "오나현", 22 }, { "박철규", 21 }, { "최재원", 20 } };

        Debug.Log(students["오나현"]);
        students["박철규"] = 23;

        foreach (KeyValuePair<string, int> item in students)
        {
            Debug.Log($"{item.Key}의 나이: {item.Value}");
        }
    }
}
