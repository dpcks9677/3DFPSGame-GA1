using UnityEngine;


public class Cup<T>
{
    public T Drink;
    public Cup(T drink)
    {
        Drink = drink;
    }
}

public class GenericTest : MonoBehaviour
{
    private void Start()
    {
        Cup<int> cup1 = new Cup<int>(17);
        Cup<float> cup2 = new Cup<float>(0.2f);
        Cup<string> cup3 = new Cup<string>("coke");
        Cup<bool> cup4 = new Cup<bool>(true);


    }
}
