using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class AcceptableSizeSpecification : IDecisionEngineSpecification
    {
        private readonly IQualityDefinitionService _qualityDefinitionService;
        private readonly IEpisodeService _episodeService;
        private readonly Logger _logger;

        public AcceptableSizeSpecification(IQualityDefinitionService qualityDefinitionService, IEpisodeService episodeService, Logger logger)
        {
            _qualityDefinitionService = qualityDefinitionService;
            _episodeService = episodeService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public Decision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            _logger.Debug("Beginning size check for: {0}", subject);

            var quality = subject.ParsedEpisodeInfo.Quality.Quality;

            if (subject.Release.Size == 0)
            {
                _logger.Debug("Release has unknown size, skipping size check");
                return Decision.Accept();
            }

            // Try to use runtime from episode/episodes first and fallback to series
            var runtime = ((int?)subject.Episodes.Where(x => x.Runtime == 0).Average(x => (int?)x.Runtime)) ?? 0;

            if (runtime == 0)
            {
                runtime = subject.Series.Runtime;
            }

            if (runtime == 0)
            {
                runtime = 30;
            }

            var qualityDefinition = _qualityDefinitionService.Get(quality);

            if (qualityDefinition.MinSize.HasValue)
            {
                var minSize = qualityDefinition.MinSize.Value.Megabytes();

                // Multiply maxSize by Series.Runtime
                minSize = minSize * runtime * subject.Episodes.Count;

                // If the parsed size is smaller than minSize we don't want it
                if (subject.Release.Size < minSize)
                {
                    var runtimeMessage = subject.Episodes.Count == 1 ? $"{runtime}min" : $"{subject.Episodes.Count}x {runtime}min";

                    _logger.Debug("Item: {0}, Size: {1} is smaller than minimum allowed size ({2} bytes for {3}), rejecting.", subject, subject.Release.Size, minSize, runtimeMessage);
                    return Decision.Reject("{0} is smaller than minimum allowed {1} (for {2})", subject.Release.Size.SizeSuffix(), minSize.SizeSuffix(), runtimeMessage);
                }
            }

            if (!qualityDefinition.MaxSize.HasValue || qualityDefinition.MaxSize.Value == 0)
            {
                _logger.Debug("Max size is unlimited, skipping size check");
            }
            else
            {
                var maxSize = qualityDefinition.MaxSize.Value.Megabytes();

                // Multiply maxSize by Series.Runtime
                maxSize = maxSize * runtime * subject.Episodes.Count;

                if (subject.Episodes.Count == 1)
                {
                    var firstEpisode = subject.Episodes.First();
                    var seasonEpisodes = GetSeasonEpisodes(subject, searchCriteria);

                    // Ensure that this is either the first episode
                    // or is the last episode in a season that has 10 or more episodes
                    if (seasonEpisodes.First().Id == firstEpisode.Id || (seasonEpisodes.Count() >= 10 && seasonEpisodes.Last().Id == firstEpisode.Id))
                    {
                        _logger.Debug("Possible double episode, doubling allowed size.");
                        maxSize = maxSize * 2;
                    }
                }

                // If the parsed size is greater than maxSize we don't want it
                if (subject.Release.Size > maxSize)
                {
                    var runtimeMessage = subject.Episodes.Count == 1 ? $"{runtime}min" : $"{subject.Episodes.Count}x {runtime}min";

                    _logger.Debug("Item: {0}, Size: {1} is greater than maximum allowed size ({2} for {3}), rejecting", subject, subject.Release.Size, maxSize, runtimeMessage);
                    return Decision.Reject("{0} is larger than maximum allowed {1} (for {2})", subject.Release.Size.SizeSuffix(), maxSize.SizeSuffix(), runtimeMessage);
                }
            }

            _logger.Debug("Item: {0}, meets size constraints", subject);
            return Decision.Accept();
        }

        private List<Episode> GetSeasonEpisodes(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            var firstEpisode = subject.Episodes.First();

            if (searchCriteria is SeasonSearchCriteria seasonSearchCriteria && !seasonSearchCriteria.Series.UseSceneNumbering && seasonSearchCriteria.Episodes.Any(v => v.Id == firstEpisode.Id))
            {
                return seasonSearchCriteria.Episodes;
            }

            return _episodeService.GetEpisodesBySeason(firstEpisode.SeriesId, firstEpisode.SeasonNumber);
        }
    }
}
