using UnityEngine;

namespace IcarusGame.CleanDuel
{
    public sealed class CleanDuelZoneTarget : MonoBehaviour
    {
        public CleanDuelBootstrap Runtime;
        public CleanPlayerId Owner;
        public CleanZoneKind Zone;
        public int Index;

        private void OnMouseDown()
        {
            if (Runtime != null)
            {
                Runtime.HandleZoneClicked(Owner, Zone, Index);
            }
        }
    }
}
