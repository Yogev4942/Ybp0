using System;
using System.Collections.Generic;

namespace Models
{
    public class Muscle : BaseEntity
    {
        private string muscleName;
        private string bodyRegion;
        private int diagramZone;
        private string bodyMapKey;

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

        public string BodyMapKey
        {
            get => bodyMapKey;
            set => bodyMapKey = value;
        }

        public List<Exercise> PrimaryExercises { get; set; } = new List<Exercise>();
        public List<Exercise> SecondaryExercises { get; set; } = new List<Exercise>();
    }
}
