namespace Kenedia.Modules.Characters.Classes
{
    using System.Collections.Generic;

    public class JsonMap
    {
        public Names Names { get; set; } = new Names();

        public int Id { get; set; }

        public int APIId { get; set; }

        public IReadOnlyList<int> Floors { get; set; }

        public int DefaultFloor { get; set; }

        public int ContinentId { get; set; }
    }
}
