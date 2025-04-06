using System;

namespace Game
{
    public class HoleState
    {
        public bool HasAbility => Ability != null;
        public CMSEntity Ability;

        public bool HasEvent => Event != null;
        public CMSEntity Event;

        public HoleView View;
    }
}