using System.ComponentModel;
using System.Configuration;

namespace Aurora.Configs
{
    public class CustomConfigSection : ConfigurationSection
    {
        [Browsable(false)]
        public new bool LockItem { get { return base.LockItem; } set { base.LockItem = value; } }

        [Browsable(false)]
        public new ElementInformation ElementInformation { get { return base.ElementInformation; } }

        [Browsable(false)]
        public new ConfigurationLockCollection LockAllAttributesExcept { get { return base.LockAllAttributesExcept; } }

        [Browsable(false)]
        public new ConfigurationLockCollection LockAllElementsExcept { get { return base.LockAllElementsExcept; } }

        [Browsable(false)]
        public new ConfigurationLockCollection LockAttributes { get { return base.LockAttributes; } }

        [Browsable(false)]
        public new ConfigurationLockCollection LockElements { get { return base.LockElements; } }

        [Browsable(false)]
        public new SectionInformation SectionInformation { get { return base.SectionInformation; } }
    }
}
