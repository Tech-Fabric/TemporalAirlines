namespace TemporalAirlinesConcept.Api.Configuration
{
    public class CheckDbOptions
    {
        public byte MaxAttemtCount { get; set; } = 9;

        public double Interval { get; set; } = 4;

        public double Multiplier { get; set; } = 1.5;
    }
}
