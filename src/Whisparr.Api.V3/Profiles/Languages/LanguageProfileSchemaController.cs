using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Languages;
using Whisparr.Http;
using Whisparr.Http.REST;

namespace Whisparr.Api.V3.Profiles.Languages
{
    [V3ApiController("languageprofile/schema")]
    [Obsolete("Deprecated")]
    public class LanguageProfileSchemaController : RestController<LanguageProfileResource>
    {
        [HttpGet]
        [Produces("application/json")]
        public LanguageProfileResource GetSchema()
        {
            return new LanguageProfileResource
            {
                Id = 1,
                Name = "Deprecated",
                UpgradeAllowed = true,
                Cutoff = Language.English,
                Languages = new List<LanguageProfileItemResource>
                {
                    new LanguageProfileItemResource
                    {
                        Language = Language.English,
                        Allowed = true
                    }
                }
            };
        }

        protected override LanguageProfileResource GetResourceById(int id)
        {
            throw new global::System.NotImplementedException();
        }
    }
}
