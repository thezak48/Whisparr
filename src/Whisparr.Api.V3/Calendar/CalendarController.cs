using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Tv;
using NzbDrone.SignalR;
using Whisparr.Api.V3.Episodes;
using Whisparr.Http;

namespace Whisparr.Api.V3.Calendar
{
    [V3ApiController]
    public class CalendarController : EpisodeControllerWithSignalR
    {
        public CalendarController(IBroadcastSignalRMessage signalR,
                            IEpisodeService episodeService,
                            ISeriesService seriesService,
                            IUpgradableSpecification qualityUpgradableSpecification,
                            ICustomFormatCalculationService formatCalculator)
            : base(episodeService, seriesService, qualityUpgradableSpecification, formatCalculator, signalR)
        {
        }

        [HttpGet]
        [Produces("application/json")]
        public List<EpisodeResource> GetCalendar(DateTime? start, DateTime? end, bool unmonitored = false, bool includeSeries = false, bool includeEpisodeFile = false, bool includeEpisodeImages = false)
        {
            var startUse = start ?? DateTime.Today;
            var endUse = end ?? DateTime.Today.AddDays(2);

            var resources = MapToResource(_episodeService.EpisodesBetweenDates(startUse, endUse, unmonitored), includeSeries, includeEpisodeFile, includeEpisodeImages);

            return resources.OrderBy(e => e.AirDateUtc).ToList();
        }
    }
}
