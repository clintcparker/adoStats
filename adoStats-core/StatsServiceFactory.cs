namespace adoStats_core
{

    public class StatsServiceFactory
    {
        private readonly StatsService statsService;
        public StatsServiceFactory(StatsService _statsService)
        {
            statsService = _statsService;
        }

        public StatsService Create(Settings settings)
        {
            statsService.Init(settings);
            return statsService;
        }

    }
}