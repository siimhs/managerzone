using NodaTime;
using System.Collections.Generic;

namespace ManagerzoneConsole
{
    public class Player
    {
        public enum Foot
        {
            Left,
            Right,
            Both
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Nationality { get; set; }
        public int BirthSeason { get; set; }
        public int Value { get; set; }
        public int Salary { get; set; }

        public int Height { get; set; }
        public int Weight { get; set; }
        public Foot PreferredFoot { get; set; }

        public IEnumerable<TrainingResult> TrainingHistory { get; set; }

        public int Speed { get; set; }
        public int Stamina { get; set; }
        public int PlayIntelligence { get; set; }
        public int Passing { get; set; }
        public int Shooting { get; set; }
        public int Heading { get; set; }
        public int Keeping { get; set; }
        public int BallControl { get; set; }
        public int Tackling { get; set; }
        public int AerialPassing { get; set; }
        public int SetPlays { get; set; }
        public int Experience { get; set; }
        public int Form { get; set; }
    }

    public class TrainingResult
    {
        public enum TrainingType
        {
            OnField,
            OnFieldWithCoach,
            InRegularCamp,
            InConditionalCamp
        }

        public string TrainedSkill { get; set; }
        public int ImprovementRate { get; set; }
        public bool HasGainedNextLevel { get; set; }
        public TrainingType Type { get; set; }
        public LocalDate Date { get; set; }
    }
}
