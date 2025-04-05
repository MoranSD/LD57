using System;

namespace Game
{
    [Serializable]
    public class HoleState
    {
        public bool HasAbility => Ability != null;
        public CMSEntity Ability;

        public HoleView View;
    }
}