namespace ModSink.Common.Models.Client
{
    public class ModEntry
    {
        public ModEntry(DTO.Repo.ModEntry me)
        {
            Default = me.Default;
            Required = me.Required;
            Mod = new Mod(me.Mod);
        }

        public bool Default { get; set; }
        public Mod Mod { get; set; }

        public bool Required { get; set; }
    }
}