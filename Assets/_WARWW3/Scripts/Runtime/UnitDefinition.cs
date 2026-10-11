using UnityEngine;
using WarWW3.Core;

namespace WarWW3
{
    [CreateAssetMenu(menuName = "WAR WW3/Unit definition")]
    public sealed class UnitDefinition : ScriptableObject
    {
        public string unitId = "bulwark";
        public string nameKey = "unit.bulwark";
        public int health = 180, armor = 3, damage = 23;
        public float speed = 5.5f, range = 20, fireInterval = 1.1f;
        public GameObject visualPrefab;
        public UnitStats ToStats() { return new UnitStats(unitId, health, armor, damage, speed, range, fireInterval); }
    }
}
