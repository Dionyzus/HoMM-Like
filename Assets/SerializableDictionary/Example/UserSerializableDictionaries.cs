using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class StringStringDictionary : SerializableDictionary<string, string> {}
[Serializable]
public class StringBoolDictionary : SerializableDictionary<string, bool> {}
[Serializable]
public class StringUnitControllerDictionary : SerializableDictionary<string, HOMM_BM.UnitController> { }
[Serializable]
public class InteractionHookTransformDictionary : SerializableDictionary<HOMM_BM.InteractionHook, Transform> { }

[Serializable]
public class ObjectColorDictionary : SerializableDictionary<UnityEngine.Object, Color> {}

[Serializable]
public class ColorArrayStorage : SerializableDictionary.Storage<Color[]> {}

[Serializable]
public class StringColorArrayDictionary : SerializableDictionary<string, Color[], ColorArrayStorage> {}

[Serializable]
public class MyClass
{
    public int i;
    public string str;
}

[Serializable]
public class QuaternionMyClassDictionary : SerializableDictionary<Quaternion, MyClass> {}