
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CatGodData", menuName = "Scriptable Objects/CatGodData")]
public class CatGodData : ScriptableObject
{
    [System.Serializable]
    public class CatGodMapping
    {
        public CatGodType catGodType;
        public ObjectType objectType;
    }

    public List<CatGodMapping> catGodMappings;

    private Dictionary<CatGodType, ObjectType> _catGodTypeToObjectDict;

    public void OnEnable()
    {
        _catGodTypeToObjectDict = new Dictionary<CatGodType, ObjectType>();
        foreach (var catGodMapping in catGodMappings)
        {
            _catGodTypeToObjectDict[catGodMapping.catGodType] = catGodMapping.objectType;
        }
    }

    public ObjectType GetObjectType(CatGodType catGodType)
    {
        if (_catGodTypeToObjectDict.TryGetValue(catGodType, out ObjectType objectType))
        {
            return objectType;
        }
        return ObjectType.None;
    }
}
