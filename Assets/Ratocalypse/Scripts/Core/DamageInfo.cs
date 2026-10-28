using UnityEngine;

namespace Ratocalypse
{
    public readonly struct DamageInfo
    {
        public readonly float value;
        public readonly DamageType type;
        public readonly bool isCritical;
        public readonly Vector3 knockback;
        public readonly GameObject source;

        public DamageInfo(float value, DamageType type, bool isCritical, Vector3 knockback, GameObject source)
        {
            this.value = value;
            this.type = type;
            this.isCritical = isCritical;
            this.knockback = knockback;
            this.source = source;
        }
    }
}
