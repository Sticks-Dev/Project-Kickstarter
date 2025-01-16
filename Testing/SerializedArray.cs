using UnityEngine;

public class SerializedArray : MonoBehaviour
{
    [SerializeField, EnumData(typeof(MyEnum))] private string[] stings;

    private enum MyEnum
    {
        One,
        Three,
        Two,
    }
}
