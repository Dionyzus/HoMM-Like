using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(StringStringDictionary))]
[CustomPropertyDrawer(typeof(StringBoolDictionary))]
[CustomPropertyDrawer(typeof(StringUnitControllerDictionary))]
[CustomPropertyDrawer(typeof(InteractionHookTransformDictionary))]
[CustomPropertyDrawer(typeof(ItemAmountDictionary))]
[CustomPropertyDrawer(typeof(ItemSlotAmountDictionary))]
[CustomPropertyDrawer(typeof(ObjectColorDictionary))]
[CustomPropertyDrawer(typeof(StringColorArrayDictionary))]
public class AnySerializableDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer {}

[CustomPropertyDrawer(typeof(ColorArrayStorage))]
public class AnySerializableDictionaryStoragePropertyDrawer: SerializableDictionaryStoragePropertyDrawer {}
