using System;

namespace Models
{
    public class Muscle : BaseEntity
    {
        private string muscleName;
        private string bodyRegion;
        private int diagramZone;

        public string MuscleName
        {
            get => muscleName;
            set => muscleName = value;
        }

        public string BodyRegion
        {
            get => bodyRegion;
            set => bodyRegion = value;
        }

        public int DiagramZone
        {
            get => diagramZone;
            set => diagramZone = value;
        }
    }
}
