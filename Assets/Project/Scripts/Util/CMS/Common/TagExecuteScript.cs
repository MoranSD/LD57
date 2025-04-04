using System;
using System.Collections;

namespace Common
{
    public class TagExecuteScript : EntityComponentDefinition
    {
        public Func<IEnumerator> Script;
    }
}