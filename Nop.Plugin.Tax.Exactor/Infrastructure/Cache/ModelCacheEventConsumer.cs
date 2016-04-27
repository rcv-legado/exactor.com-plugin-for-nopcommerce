using System;
using Nop.Core.Caching;
using Nop.Core.Domain.Common;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Services.Events;

namespace Nop.Plugin.Tax.Exactor.Infrastructure.Cache
{
    /// <summary>
    ///  Model cache event consumer
    /// </summary>
    public partial class ModelCacheEventConsumer : IConsumer<EntityUpdated<Address>>
    {
        private readonly ICacheManager _cacheManager;
        public const string TAXRATE_KEY = "Nop.plugins.tax.exactor.taxbyaddresscategory-{0}-{1}";

        public ModelCacheEventConsumer()
        {
            //TODO inject static cache manager using constructor
            this._cacheManager = EngineContext.Current.ContainerManager.Resolve<ICacheManager>("nop_cache_static");
        }

        public void HandleEvent(EntityUpdated<Address> eventMessage)
        {
            _cacheManager.RemoveByPattern(String.Format(TAXRATE_KEY, eventMessage.Entity.Id, "\\d"));
        }
    }
}
