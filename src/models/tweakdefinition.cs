using System;
using System.Collections.Generic;

namespace WinTweaker.Models
{
    public class TweakItem
    {
        public string id { get; set; }
        public string name_ru { get; set; }
        public string description_ru { get; set; }
        public string category { get; set; }
        public string group { get; set; }
        public string type { get; set; }
        public string risk { get; set; }
        public List<string> presets { get; set; }
        public List<RegistryActionItem> registry { get; set; }
        public SourceReference source { get; set; }
        public int? min_build { get; set; }
        public bool needs_review { get; set; }

        public TweakItem()
        {
            this.presets = new List<string>();
            this.registry = new List<RegistryActionItem>();
            this.source = new SourceReference();
        }
    }

    public class RegistryActionItem
    {
        public string hive { get; set; }
        public string path { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public object apply { get; set; }
        public string apply_block { get; set; }
        public RevertSpec revert { get; set; }

        public RegistryActionItem()
        {
            this.revert = new RevertSpec();
        }
    }

    public class RevertSpec
    {
        public string mode { get; set; } // "snapshot", "set", "delete"
        public object value { get; set; }
    }

    public class SourceReference
    {
        public string file { get; set; }
        public int line_from { get; set; }
        public int line_to { get; set; }
    }
}
