namespace HoyoVERSE.Models
{
    // Persisted custom-game entry (a user-added executable shown in the sidebar
    // alongside the HYP-API games).
    public class CustomGame
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ExePath { get; set; }
        public string IconPath { get; set; }
        public string Args { get; set; }
    }

    public enum HypRegion { Global, China }
}
